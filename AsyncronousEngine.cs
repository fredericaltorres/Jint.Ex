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
        Interval
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
        internal System.Timers.Timer Timer;
        internal bool Enabled = true;

        internal bool Disabled
        {
            get { return !this.Enabled; }
        }

        private static int _timeOutIdCounter = 0;

        public CallBackInfo(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> function, double delay, CallBackType type)
        {
            this.Function = function;
            this.Delay    = (int)delay;
            this.Id       = _timeOutIdCounter++;
            this.Type     = type;
        }

        public void Clear()
        {
            this.Enabled = false;
            if (this.Timer != null)
            {
                this.Timer.Enabled = false;
                this.Timer = null;
            }
        }
    }

    internal class CallBackInfos : List<CallBackInfo>
    {
        public void Stop()
        {
            foreach (var c in this)
            {
                if (c.Timer != null)
                {
                    c.Enabled = false;
                    c.Timer.Enabled = false;
                    c.Timer = null;    
                }                
            }
        }
    }

    public class AsyncronousEngine
    {
        /// <summary>
        /// Queue that contain event to be scheduled or currently scheduled and running
        /// </summary>
        internal static CallBackInfos _callBackQueue = new CallBackInfos();

        /// <summary>
        /// Indicate if we at least run the first main script. During the first main script
        /// all asynchronous events are registered but not scheduled. After the main script 
        /// is fully executed than the events are scheduled.
        /// After that is a first generation of asyncrhonous event call setTimeout() for example
        /// the event is registered and scheduled at the same time.
        /// 
        /// We currently have the following problem:
        /// 
        /// MainScript Schedule F1()
        ///     F1() is executed
        ///         F1() setTimeOut(F2, 10);
        ///         Other F1() code
        ///         F1() take 100 to executed
        /// 
        ///     F2 scheduled to execute 10 ms will interrupt F1(), this is not good
        /// 
        /// </summary>
        private static bool                 _callBackLoopRuning = false;

        /// <summary>
        /// The jint engine
        /// </summary>
        private static Jint.Engine          _engine             = null;

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
        /// Reference the assembly that embed the JavaScript scripts.
        /// </summary>
        public static Assembly EmbedScriptAssembly = null;

        private static void Print(object s)
        {
            if (s == null)
                s = "null";
            Console.WriteLine(s.ToString());
        }

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

        /// <summary>
        /// Execute a MainScript
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ExecutionEndType Execute(string source)
        {
            var e = ExecutionEndType.Undefined;
            Engine.Execute(source);     
            ScheduleCallBackEvents();
            return e;
        }

        /// <summary>
        /// Timer event executing all asynchronous JavaScript callback
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnIntervalEvent(object source, ElapsedEventArgs e)
        {
            if (IsMainThreadRunning)
            {
                // If the main thread is running, we do not allow to the run asynchronous event
                // This can happens the client app execute MainScript1 which calls setInterval(F1, 100ms)
                // Then client app execute MainScript2 which take 1000ms to execute
                // During the execution of MainScript2, the timer for the setInterval will trigger
                // In that case the timer will be ignored
            }
            else
            {
                var t     = source as System.Timers.Timer;
                t.Enabled = false;
                var cbe   = _callBackQueue.FirstOrDefault(c => c.Timer == t);
                if (cbe != null)
                {
                    var jsValue = ExecuteCallBack(cbe.Function);
                    if (cbe.Type == CallBackType.TimeOut)
                        cbe.Clear();
                    else
                        t.Enabled = true; // re start timer    
                }
            }
        }

        /// <summary>
        /// Returns true is there is no enabled event in the Queue
        /// </summary>
        /// <returns></returns>
        private static bool NoMoreEvent()
        {
            return _callBackQueue.All(c => c.Timer == null);
        }

        /// <summary>
        /// Remove from the queue the callback that have been executed and are now disabled
        /// </summary>
        private static void CleanCallBackEvents()
        {
            while (true)
            {
                var x = _callBackQueue.FindIndex(e => e.Disabled && e.Timer == null);
                if (x == -1)
                    break;

                _callBackQueue.RemoveAt(x);
            }
        }

        /// <summary>
        /// Schedule CallBack Events Execution and clean queue
        /// </summary>
        private static void ScheduleCallBackEvents()
        {
            foreach(var c in _callBackQueue)
            {
                if(c.Enabled && c.Timer == null)
                {
                    c.Timer           = new System.Timers.Timer(c.Delay);
                    c.Timer.Elapsed  += new ElapsedEventHandler(OnIntervalEvent);
                    c.Timer.Enabled   = true;
                    c.Timer.AutoReset = c.Type == CallBackType.Interval; // AutoReset = true loop forever
                }
            }
            CleanCallBackEvents();
            _callBackLoopRuning = true;
        }

        /// <summary>
        /// Load multiple scripts embed or from the file system. This method must be used the load
        /// libraries and must not be used to execute the main script. This method is synchronous.
        /// </summary>
        /// <param name="fileNames"></param>
        public static void LoadScripts(params string[] fileNames)
        {
            var jint = AsyncronousEngine.AllocateNewJintInstance();
            var source = new StringBuilder();

            foreach (var fileName in fileNames)
                AsyncronousEngine.LoadLibrary(fileName, source);

            Engine.Execute(source.ToString());
        }

        private static Thread           __MainThread = null;
        private static string           __MainSource;
        private static ExecutionEndType __MainResult;
        private static int              __MainThreadRunning = 0;

        /// <summary>
        /// Background thread dedicated to execute the MainScript
        /// </summary>
        private static void __BackgroundThread()
        {
            // TODO we need a try/catch
            var source = new StringBuilder();
            __MainResult = AsyncronousEngine.Execute(__MainSource);
            __MainThread = null; // We are done
            Interlocked.Decrement(ref __MainThreadRunning);
        }
        
        /// <summary>
        /// Return true is a thread safe wayt if the main thread is running
        /// </summary>
        public static bool IsMainThreadRunning
        {
            get { return __MainThreadRunning > 0; }
        }

        /// <summary>
        /// Wait until the main script/main thread is finished and the callback queue is empty
        /// </summary>
        public static void Wait()
        {
            if (__MainThread != null)
            {
                while (__MainThread != null && __MainThread.IsAlive)
                {
                    System.Threading.Thread.Sleep(100);
                }
                while (_callBackQueue.Count > 0)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Clear the callback queue
        /// </summary>
        public static void ClearQueue()
        {
            _callBackQueue.Stop();
            CleanCallBackEvents();
        }

        /// <summary>
        /// Kill the main thread/main script execution
        /// </summary>
        public static void Kill()
        {
            if (__MainThread != null)
                __MainThread.Abort();
        }

        /// <summary>
        /// Start the execution of multiple scripts as the Main Script, in the Jint.Ex
        /// background thread. The method returns right away.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="block"></param>
        public static bool Start(string fileName, bool block = false)
        {
            if (IsMainThreadRunning)
            {
                return false;
            }
            else
            {
                Interlocked.Increment(ref __MainThreadRunning);

                var source = new StringBuilder();

                AsyncronousEngine.LoadLibrary(fileName, source);

                __MainSource      = source.ToString();
                __MainResult      = ExecutionEndType.Undefined;
                __MainThread      = new Thread(new ThreadStart(__BackgroundThread));
                __MainThread.Name = "Jint.Ex.BackgroundExecutionThread";
                __MainThread.Start();
                if(block)
                    AsyncronousEngine.Wait();
                return true;
            }
        }

        private static void ClearCallBackEvent(int id)
        {
            var c = _callBackQueue.FirstOrDefault(e => e.Id == id);
            if (c == null)
                throw new ArgumentException(string.Format("Cannot find timeout or interval id {0}", id));
            else
                c.Clear();
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
            var c = new CallBackInfo(callBackFunction, delay, CallBackType.Interval);
            _callBackQueue.Add(c);
            if (_callBackLoopRuning)
                ScheduleCallBackEvents(); // Schedule call back now
            return c.Id;
        }

        private static void __clearInterval__(int id)
        {
            ClearCallBackEvent(id);
        }

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/API/Window.setTimeout
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private static int __setTimeout__(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, double delay)
        {
            var c = new CallBackInfo(callBackFunction, delay, CallBackType.TimeOut);
            _callBackQueue.Add(c); // Add the callback to the list
            if (_callBackLoopRuning)
                ScheduleCallBackEvents(); // Schedule call back now
            return c.Id; // Return id that can clear the call back
        }

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/API/window.clearTimeout
        /// </summary>
        /// <param name="id"></param>
        private static void __clearTimeout__(int id)
        {
            ClearCallBackEvent(id);
        }
        #endregion
    }
}
