using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
//1using System.Timers;
using DynamicSugar;
using Jint;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Jint.Native;

namespace Jint.Ex
{
    public class AsyncronousEngine
    {
        private  readonly CallBackEventQueue _eventQueue                 = new CallBackEventQueue();
        private  Thread                      _mainThread                 = null;
        private  int                         _mainThreadRunningSemaphore = 0;
        private  bool                        _runBackgroundThread        = true;
        private  Jint.Engine                 _engine                     = null;

        /// <summary>
        /// Reference the assembly that embed the JavaScript scripts.
        /// </summary>
        public List<Assembly> EmbedScriptAssemblies = new List<Assembly>();

        /// <summary>
        /// Allocate a Jint instance and registered all the standard methods
        /// setTimeout, clearTimeout, setInterval, clearInterval, print
        /// </summary>
        /// <returns></returns>
        private  Engine AllocateNewJintInstance()
        {
            var e = new Engine();
            
            e.SetValue("setTimeout"   , new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setTimeout__));
            e.SetValue("clearTimeout" , new Action<int>(__clearTimeout__));
            e.SetValue("setInterval"  , new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setInterval__));
            e.SetValue("clearInterval", new Action<int>(__clearInterval__));

            e.SetValue("print", new Action<object>(Print)); // << TODO: Parameterize the method

            return e;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            this.EmbedScriptAssemblies.Clear();
            this.Stop();
            _engine = null;
        }
        /// <summary>
        /// The instance of Jint
        /// </summary>
        public Jint.Engine Engine
        {
            get
            {
                if (this._engine == null)
                    this._engine = AllocateNewJintInstance();
                return this._engine;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        private static void Print(object s)
        {
            if (s == null)
                s = "null";
            Console.WriteLine(s.ToString());
        }
        /// <summary>
        /// Load a file from the file system or as an embed resource
        /// </summary>
        /// <param name="name"></param>
        /// <param name="source"></param>
        public void LoadScript(string name, StringBuilder source)
        {
            if (System.IO.File.Exists(name))
                source.Append(System.IO.File.ReadAllText(name)).AppendLine();
            else
                source.Append(DS.Resources.GetTextResource(name, EmbedScriptAssemblies)).AppendLine();
        }
        /// <summary>
        /// Request the execution of a JavaScript callback function. This method should be called by 
        /// C# custom object that want to implement asynchronous api.
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <param name="parameters"></param>
        public void RequestCallbackExecution(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, List<JsValue>  parameters)
        {
            this._eventQueue.Enqueue(new CallBackEvent(callBackFunction, parameters));
        }
        /// <summary>
        /// Execute a JavaScript callback function
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private JsValue ExecuteCallBack(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, List<JsValue> parameters = null)
        {
            if(parameters == null)
                parameters = new List<JsValue>();

            JsValue r = callBackFunction.Invoke( // Call the callback function
                JsValue.Undefined,               // Pass this as undefined
                parameters.ToArray()             // Pass the parameter data
                );
            return r;
        }
        /// <summary>
        /// Execute the JavaScrip source in a blocking way
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private ExecutionEndType Execute(string source)
        {
            this.Engine.Execute(source);
            return ExecutionEndType.Undefined;
        }
        /// <summary>
        /// Background thread dedicated to execute the MainScript
        /// </summary>
        private void __BackgroundThread()
        {
            const int sleepTime = 32;

            while (this._runBackgroundThread)
            {
                Debug.WriteLine(string.Format("_runBackgroundThread:{0} Queue:{1}", Environment.TickCount, _eventQueue.Count));
                if (_eventQueue.Count > 0)
                {
                    // Execute first all events with no delay
                    var tmpQ = this._eventQueue.GetEventsWithNoDelay();
                    while(tmpQ.Count > 0)
                    {
                        this.ExecuteEvent(tmpQ.Peek(), tmpQ);
                    }

                    // Deal with timer event now
                    Thread.Sleep(sleepTime); // Sleep minimal time
                    tmpQ = this._eventQueue.CloneEventWithDelay();
                    foreach(var e in tmpQ)
                    {
                        if (e.ReadyForExecution(sleepTime))
                        {
                            this.ExecuteEvent(e, null);
                        }
                    }
                    this._eventQueue.RemoveDisabledEvents();
                }
                else 
                    Thread.Sleep(sleepTime);
            }
            Interlocked.Decrement(ref _mainThreadRunningSemaphore);
        }

        private void ExecuteEvent(CallBackEvent c, CallBackEventQueue tmpQ)
        {
            if (c.Enabled)
            {
                c.Enabled = false;
                switch (c.Type)
                {
                    case CallBackType.ClearQueue:
                        this._eventQueue.Clear();
                        break;
                    case CallBackType.ScriptExecution:
                        this.Execute(c.Source);
                        break;
                    case CallBackType.UserCallback:
                        this.ExecuteCallBack(c.Function, c.Parameters);
                        break;
                    case CallBackType.TimeOut:
                        this.ExecuteCallBack(c.Function);
                        break;
                    case CallBackType.Interval:
                        c.Enabled = true;
                        this.ExecuteCallBack(c.Function);
                        break;
                }
            }
            this._eventQueue.RemoveTopBecauseProcessed(c, true);
            if (tmpQ != null)
                tmpQ.RemoveTopBecauseProcessed(c, false);
        }
        
        /// <summary>
        /// Return true if the main thread is running (thread safe)
        /// </summary>
        private bool IsMainThreadRunning
        {
            get { return this._mainThreadRunningSemaphore > 0; }
        }

        /// <summary>
        /// Clear the event queue
        /// </summary>
        public void RequestClearQueue()
        {
            this._eventQueue.Enqueue(new CallBackEvent(CallBackType.ClearQueue));
        }

        /// <summary>
        /// Wait until the event queue is empty
        /// </summary>
        public void Wait()
        {
            while (this._eventQueue.Count > 0)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Stop the event loop
        /// </summary>
        public void Stop(Action sleepMethod = null)
        {
            if (this._mainThread != null)
            {
                this._runBackgroundThread = false;
                while (this._mainThread.IsAlive)
                {
                    if (sleepMethod == null)
                        Thread.Sleep(100);
                    else 
                        sleepMethod();
                }
                _mainThread = null;
            }
        }

        /// <summary>
        /// Kill the event loop
        /// </summary>
        public void Kill()
        {
            if (this._mainThread != null)
            {
                this._runBackgroundThread = false;
                this._mainThread.Abort();
                this._mainThread = null;
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Request the execution of one javaScript script file by the event loop. 
        /// The method returns right away. 
        /// Start the AsyncronousEngine if needed.
        /// </summary>
        /// <param name="fileName">The filename or resource name to load and execute</param>
        /// <param name="block">If true after the execution, block until the event queue is empty</param>
        public bool RequestFileExecution(string fileName, bool block = false)
        {
            var source = new StringBuilder();
            this.LoadScript(fileName, source);
            return this.RequestScriptExecution(source.ToString(), block);
        }
        /// <summary>
        /// Request the execution of one javaScript source by the event loop. 
        /// The method returns right away. 
        /// Start the AsyncronousEngine if needed.
        /// </summary>
        /// <param name="fileName">The filename or resource name to load and execute</param>
        /// <param name="block">If true after the execution, block until the event queue is empty</param>
        public bool RequestScriptExecution(string source, bool block = false)
        {
            if (!this.IsMainThreadRunning)
                this.Start();

            this._eventQueue.RequestScriptExecution(source.ToString());

            if (block)
            {
                this.Wait();
                this.Stop();
            }

            return true;
        }

        /// <summary>
        /// Start the event loop
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (this.IsMainThreadRunning)
            {
                return false;
            }
            else
            {
                Interlocked.Increment(ref _mainThreadRunningSemaphore);
                this._mainThread = new Thread(new ThreadStart(__BackgroundThread)) { Name = "Jint.Ex.BackgroundExecutionThread" };
                this._runBackgroundThread = true;
                this._mainThread.Start();                
                return true;
            }
        }

        #region JavaScriptFunction

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/API/Window.setInterval
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int __setInterval__(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, double delay)
        {
            return this._eventQueue.Enqueue(new CallBackEvent(callBackFunction, delay, CallBackType.Interval)).Id;
        }

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/API/Window.setTimeout
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private int __setTimeout__(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, double delay)
        {
            return this._eventQueue.Enqueue(new CallBackEvent(callBackFunction, delay, CallBackType.TimeOut)).Id;
        }

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/API/window.clearTimeout
        /// </summary>
        /// <param name="id"></param>
        private void __clearTimeout__(int id)
        {
            this._eventQueue.ClearCallBackEvent(id);
        }

        private void __clearInterval__(int id)
        {
            this._eventQueue.ClearCallBackEvent(id);
        }
        
        #endregion
    }
}
