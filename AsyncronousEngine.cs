using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using DynamicSugar;
using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Native;

namespace Jint.Ex
{
    internal enum CallBackType
    {
        TimeOut,
        Interval,
        ScriptExecution,
        UserCallback,
    }

    internal enum ExecutionEndType
    {
        Undefined,
        EndOfScript,
        UserCancel,
        Error
    }

    internal class CallBackEvent
    {
        internal Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> Function;
        internal List<JsValue> Parameters;
        internal int Delay;
        internal int Id;
        internal CallBackType Type;
        internal bool Enabled = true;
        internal string Source;

        private static int _timeOutIdCounter = 0;

        internal bool Disabled
        {
            get { return !this.Enabled; }
        }

        public CallBackEvent()
        {
            this.Id = _timeOutIdCounter++;
        }

        public CallBackEvent(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, List<JsValue> parameters) : this()
        {
            this.Type = CallBackType.UserCallback;
            this.Function = callBackFunction;
            this.Parameters = parameters;
        }
        
        public CallBackEvent(string source) : this()
        {
            this.Source = source;
            this.Type = CallBackType.ScriptExecution;
        }

        public CallBackEvent(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> function, double delay, CallBackType type) : this()
        {
            this.Function = function;
            this.Delay    = (int)delay;
            this.Type     = type;
        }

        public void Disable()
        {
            this.Enabled = false;
        }
    }

    internal class CallBackEventQueue
    {
        private static readonly Object obj = new Object();
        private List<CallBackEvent> _queue = new List<CallBackEvent>();

        public int Count {
            get { return this._queue.Count; }
        }
        public void Clear()
        {
            lock (obj)
            {
                this._queue.Clear();
            }
        }
        public void RequestScriptExecution(string source)
        {
            lock (obj)
            {
                this._queue.Insert(0, new CallBackEvent(source));
            }
        }
        public void EndqueueCallBackExecution(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> function, double delay, CallBackType type)
        {
            lock (obj)
            {
                this._queue.Add(new CallBackEvent(function, delay, type));
            }
        }
        public CallBackEvent Enqueue(CallBackEvent c)
        {
            lock (obj)
            {
                this._queue.Add(c);
            }
            return c;
        }
        public CallBackEvent Dequeue()
        {
            lock (obj)
            {
                var c = this._queue[0];
                this._queue.RemoveAt(0);
                return c;
            }
        }
        public CallBackEvent Peek()
        {
            lock (obj)
            {
                return this._queue[0];
            }
        }
        public void ClearCallBackEvent(int id)
        {
            lock (obj)
            {
                var c = this._queue.FirstOrDefault(e => e.Id == id);
                if (c == null)
                    throw new ArgumentException(string.Format("Cannot find timeout or interval id {0}", id));
                else
                    c.Disable();
            }
        }
        public void RemoveTopBecauseProcessed()
        {
            lock (obj)
            {
                var c = this.Dequeue();
                // for Interval we re add the event at then end of the queue
                if (c.Type == CallBackType.Interval)
                    this.Enqueue(c);
            }
        }                            
    }

    public class AsyncronousEngine
    {
        private static readonly CallBackEventQueue _eventQueue                 = new CallBackEventQueue();
        private static Thread                      _mainThread                 = null;
        private static int                         _mainThreadRunningSemaphore = 0;
        private static bool                        _runBackgroundThread        = true;
        private static Jint.Engine                 _engine                     = null;

        /// <summary>
        /// Reference the assembly that embed the JavaScript scripts.
        /// </summary>
        public static Assembly EmbedScriptAssembly = null;

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
        public static void LoadLibrary(string name, StringBuilder source)
        {
            if (System.IO.File.Exists(name))
                source.Append(System.IO.File.ReadAllText(name)).AppendLine();
            else
                source.Append(DS.Resources.GetTextResource(name, EmbedScriptAssembly)).AppendLine();
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
            while (_runBackgroundThread)
            {
                Debug.WriteLine(string.Format("_runBackgroundThread:{0}", Environment.TickCount));
                if (_eventQueue.Count > 0)
                {
                    var c = _eventQueue.Peek();
                    if (c.Enabled)
                    {
                        switch (c.Type)
                        {
                            case CallBackType.ScriptExecution:
                                Execute(c.Source);
                                _eventQueue.RemoveTopBecauseProcessed();
                                break;
                            case CallBackType.UserCallback:
                                ExecuteCallBack(c.Function, c.Parameters);
                                _eventQueue.RemoveTopBecauseProcessed();
                                break;
                            case CallBackType.TimeOut:
                            case CallBackType.Interval:
                                Thread.Sleep(c.Delay);
                                // I do not know why I have to do that
                                //if (_runBackgroundThread)
                                ExecuteCallBack(c.Function);
                                _eventQueue.RemoveTopBecauseProcessed();
                                break;
                        }
                    }
                    else
                    {
                        _eventQueue.Dequeue();
                    }
                }
                else Thread.Sleep(32);
            }
            Interlocked.Decrement(ref _mainThreadRunningSemaphore);
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
        public static void ClearQueue()
        {
            _eventQueue.Clear();
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
        public static void Stop()
        {
            if (_mainThread != null)
            {
                _runBackgroundThread = false;
                while (_mainThread.IsAlive)
                {
                    Thread.Sleep(100); 
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
        /// RequestExecution the execution of one javaScript script by the event loop. 
        /// The method returns right away. 
        /// Start the AsyncronousEngine if needed.
        /// </summary>
        /// <param name="fileName">The filename or resource name to load and execute</param>
        /// <param name="block">If true after the execution, block until the event queue is empty</param>
        public static bool RequestExecution(string fileName, bool block = false)
        {
            if (!IsMainThreadRunning)
                Start();

            var source = new StringBuilder();
            AsyncronousEngine.LoadLibrary(fileName, source);
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
                ClearQueue();
                Interlocked.Increment(ref _mainThreadRunningSemaphore);
                _mainThread          = new Thread(new ThreadStart(__BackgroundThread));
                _mainThread.Name     = "Jint.Ex.BackgroundExecutionThread";
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
