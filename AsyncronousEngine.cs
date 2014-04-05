using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using DynamicSugar;
using Jint;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Jint.Native;

namespace Jint.Ex
{
    public static class AsyncronousEngine
    {
        private static readonly CallBackEventQueue _eventQueue                 = new CallBackEventQueue();
        private static Thread                      _mainThread                 = null;
        private static int                         _mainThreadRunningSemaphore = 0;
        private static bool                        _runBackgroundThread        = true;
        private static Jint.Engine                 _engine                     = null;

        /// <summary>
        /// Reference the assembly that embed the JavaScript scripts.
        /// </summary>
        public static List<Assembly> EmbedScriptAssemblies = new List<Assembly>();

        /// <summary>
        /// Allocate a Jint instance and registered all the standard methods
        /// setTimeout, clearTimeout, setInterval, clearInterval, print
        /// </summary>
        /// <returns></returns>
        private static Engine AllocateNewJintInstance()
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
        public static void Reset()
        {
            AsyncronousEngine.EmbedScriptAssemblies.Clear();
            AsyncronousEngine.Stop();
            _engine = null;
        }
        /// <summary>
        /// The instance of Jint
        /// </summary>
        public static Jint.Engine Engine
        {
            get
            {
                if (_engine == null)
                    _engine = AllocateNewJintInstance();
                return _engine;
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
        public static void LoadScript(string name, StringBuilder source)
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
        public static void RequestCallbackExecution(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, List<JsValue>  parameters)
        {
            _eventQueue.Enqueue(new CallBackEvent(callBackFunction, parameters));
        }
        /// <summary>
        /// Execute a JavaScript callback function
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static JsValue ExecuteCallBack(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, List<JsValue> parameters = null)
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
        private static ExecutionEndType Execute(string source)
        {
            Engine.Execute(source);
            return ExecutionEndType.Undefined;
        }
        /// <summary>
        /// Background thread dedicated to execute the MainScript
        /// </summary>
        private static void __BackgroundThread()
        {
            const int sleepTime = 32;

            while (_runBackgroundThread)
            {
                Debug.WriteLine(string.Format("_runBackgroundThread:{0} Queue:{1}", Environment.TickCount, _eventQueue.Count));
                if (_eventQueue.Count > 0)
                {
                    // Execute first all events with no delay
                    var tmpQ = _eventQueue.GetEventsWithNoDelay();
                    while(tmpQ.Count > 0)
                    {
                        ExecuteEvent(tmpQ.Peek(), tmpQ);
                    }

                    // Deal with timer event now
                    Thread.Sleep(sleepTime); // Sleep minimal time
                    tmpQ = _eventQueue.CloneEventWithDelay();
                    foreach(var e in tmpQ)
                    {
                        if (e.ReadyForExecution(sleepTime))
                        {
                            ExecuteEvent(e, null);
                        }
                    }
                    _eventQueue.RemoveDisabledEvents();
                }
                else 
                    Thread.Sleep(sleepTime);
            }
            Interlocked.Decrement(ref _mainThreadRunningSemaphore);
        }

        private static void ExecuteEvent(CallBackEvent c, CallBackEventQueue tmpQ)
        {
            if (c.Enabled)
            {
                c.Enabled = false;
                switch (c.Type)
                {
                    case CallBackType.ClearQueue:
                        _eventQueue.Clear();
                        break;
                    case CallBackType.ScriptExecution:
                        Execute(c.Source);
                        break;
                    case CallBackType.UserCallback:
                        ExecuteCallBack(c.Function, c.Parameters);
                        break;
                    case CallBackType.TimeOut:
                        ExecuteCallBack(c.Function);
                        break;
                    case CallBackType.Interval:
                        c.Enabled = true;
                        ExecuteCallBack(c.Function);
                        break;
                }
            }
            _eventQueue.RemoveTopBecauseProcessed(c, true);
            if (tmpQ != null)
                tmpQ.RemoveTopBecauseProcessed(c, false);
        }
        
        /// <summary>
        /// Return true if the main thread is running (thread safe)
        /// </summary>
        private static bool IsMainThreadRunning
        {
            get { return _mainThreadRunningSemaphore > 0; }
        }

        /// <summary>
        /// Clear the event queue
        /// </summary>
        public static void RequestClearQueue()
        {
            _eventQueue.Enqueue(new CallBackEvent(CallBackType.ClearQueue));
        }

        /// <summary>
        /// Wait until the event queue is empty
        /// </summary>
        public static void Wait()
        {
            while (_eventQueue.Count > 0)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Stop the event loop
        /// </summary>
        public static void Stop(Action sleepMethod = null)
        {
            if (_mainThread != null)
            {
                _runBackgroundThread = false;
                while (_mainThread.IsAlive)
                {
                    if (sleepMethod == null)
                        Thread.Sleep(100);
                    else 
                        sleepMethod();
                }
                //_mainThread.Join();
                _mainThread = null;
            }
        }

        /// <summary>
        /// Kill the event loop
        /// </summary>
        public static void Kill()
        {
            if (_mainThread != null)
            {
                _runBackgroundThread = false;
                _mainThread.Abort();
                _mainThread = null;
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
        public static bool RequestScriptFileExecution(string fileName, bool block = false)
        {
            var source = new StringBuilder();
            AsyncronousEngine.LoadScript(fileName, source);
            return RequestScriptExecution(source.ToString(), block);
        }
        /// <summary>
        /// Request the execution of one javaScript source by the event loop. 
        /// The method returns right away. 
        /// Start the AsyncronousEngine if needed.
        /// </summary>
        /// <param name="fileName">The filename or resource name to load and execute</param>
        /// <param name="block">If true after the execution, block until the event queue is empty</param>
        public static bool RequestScriptExecution(string source, bool block = false)
        {
            if (!IsMainThreadRunning)
                Start();

            _eventQueue.RequestScriptExecution(source.ToString());

            if (block)
            {
                Wait();
                AsyncronousEngine.Stop();
            }

            return true;
        }

        /// <summary>
        /// Start the event loop
        /// </summary>
        /// <returns></returns>
        public static bool Start()
        {
            if (IsMainThreadRunning)
            {
                return false;
            }
            else
            {
                Interlocked.Increment(ref _mainThreadRunningSemaphore);
                _mainThread          = new Thread(new ThreadStart(__BackgroundThread)) { Name = "Jint.Ex.BackgroundExecutionThread" };
                _runBackgroundThread = true;
                _mainThread.Start();                
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
        public static int __setInterval__(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, double delay)
        {
            return _eventQueue.Enqueue(new CallBackEvent(callBackFunction, delay, CallBackType.Interval)).Id;
        }

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/API/Window.setTimeout
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private static int __setTimeout__(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, double delay)
        {
            return _eventQueue.Enqueue(new CallBackEvent(callBackFunction, delay, CallBackType.TimeOut)).Id;
        }

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/API/window.clearTimeout
        /// </summary>
        /// <param name="id"></param>
        private static void __clearTimeout__(int id)
        {
            _eventQueue.ClearCallBackEvent(id);
        }

        private static void __clearInterval__(int id)
        {
            _eventQueue.ClearCallBackEvent(id);
        }
        
        #endregion
    }
}
