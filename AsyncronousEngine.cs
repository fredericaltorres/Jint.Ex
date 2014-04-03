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
        ScriptExecution
    }

    internal enum ExecutionEndType
    {
        Undefined,
        EndOfScript,
        UserCancel,
        Error
    }

    internal class CallBackInfo
    {
        internal Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> Function;
        internal int Delay;
        internal int Id;
        internal CallBackType Type;
        internal bool Enabled = true;
        internal string Source;

        internal bool Disabled
        {
            get { return !this.Enabled; }
        }

        private static int _timeOutIdCounter = 0;

        public CallBackInfo()
        {
            this.Id = _timeOutIdCounter++;
        }

        public CallBackInfo(string source) : this()
        {
            this.Source = source;
            this.Type = CallBackType.ScriptExecution;
        }

        public CallBackInfo(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> function, double delay, CallBackType type) : this()
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

    internal class CallBackInfos
    {
        private static readonly Object obj = new Object();

        private List<CallBackInfo> _queue = new List<CallBackInfo>();

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
                this._queue.Insert(0, new CallBackInfo(source));
            }
        }
        public void EndqueueCallBackExecution(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> function, double delay, CallBackType type)
        {
            lock (obj)
            {
                this._queue.Add(new CallBackInfo(function, delay, type));
            }
        }
        public CallBackInfo Enqueue(CallBackInfo c)
        {
            lock (obj)
            {
                this._queue.Add(c);
            }
            return c;
        }
        public CallBackInfo Dequeue()
        {
            lock (obj)
            {
                var c = this._queue[0];
                this._queue.RemoveAt(0);
                return c;
            }
        }
        public CallBackInfo Peek()
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
        /// <summary>
        /// Queue that contain event to be scheduled or currently scheduled and running
        /// </summary>
        private static CallBackInfos _eventQueue = new CallBackInfos();

        /// <summary>
        /// The jint engine
        /// </summary>
        private static Jint.Engine  _engine = null;

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
            e.SetValue("print"        , new Action<object>(Print));
            e.SetValue("setTimeout"   , new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setTimeout__));
            e.SetValue("clearTimeout" , new Action<int>(__clearTimeout__));
            e.SetValue("setInterval"  , new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setInterval__));
            e.SetValue("clearInterval", new Action<int>(__clearInterval__));
            return e;
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
        /// Execute a JavaScript callback function
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <returns></returns>
        private static JsValue ExecuteCallBack(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction)
        {
            JsValue r = callBackFunction.Invoke( // Call the callback function
                JsValue.Undefined,               // Pass this as undefined
                new List<JsValue>() {}.ToArray() // Pass the parameter data
                );
            return r;
        }
        
        private static ExecutionEndType Execute(string source)
        {
            var e = ExecutionEndType.Undefined;
            Engine.Execute(source);     
            return e;
        }

        private static Thread           __MainThread          = null;
        private static int              __MainThreadRunning   = 0;
        private static bool             __RunBackgroundThread = true;
        /// <summary>
        /// Background thread dedicated to execute the MainScript
        /// </summary>
        private static void __BackgroundThread()
        {
            while (__RunBackgroundThread)
            {
                Debug.WriteLine(string.Format("__RunBackgroundThread:{0}", Environment.TickCount));
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
                            case CallBackType.TimeOut:
                            case CallBackType.Interval:
                                Thread.Sleep(c.Delay);
                                // I do not know why I have to do that
                                //if (__RunBackgroundThread)
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
            Interlocked.Decrement(ref __MainThreadRunning);
            Debug.WriteLine(string.Format("__RunBackgroundThread:STOPPED"));
        }
        private static bool IsMainThreadRunning
        {
            get { return __MainThreadRunning > 0; }
        }
        /// <summary>
        /// Clear the event loop queue
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
            if (__MainThread != null)
            {
                __RunBackgroundThread = false;
                while (__MainThread.IsAlive)
                {
                    Thread.Sleep(100); 
                }
                //__MainThread.Join();
                __MainThread = null;
            }
        }

        /// <summary>
        /// Kill the event loop
        /// </summary>
        public static void Kill()
        {
            if (__MainThread != null)
            {
                __RunBackgroundThread = false;
                __MainThread.Abort();
                __MainThread = null;
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// RequestExecution the execution of multiple scripts as the Main Script, in the Jint.Ex
        /// background thread. The method returns right away.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="block"></param>
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
                Interlocked.Increment(ref __MainThreadRunning);
                __MainThread          = new Thread(new ThreadStart(__BackgroundThread));
                __MainThread.Name     = "Jint.Ex.BackgroundExecutionThread";
                __RunBackgroundThread = true;
                __MainThread.Start();                
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
            return _eventQueue.Enqueue(new CallBackInfo(callBackFunction, delay, CallBackType.Interval)).Id;
        }

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/API/Window.setTimeout
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private static int __setTimeout__(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, double delay)
        {
            return _eventQueue.Enqueue(new CallBackInfo(callBackFunction, delay, CallBackType.TimeOut)).Id;
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
