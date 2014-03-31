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
            this.Delay = (int)delay;
            this.Id = _timeOutIdCounter++;
            this.Type = type;
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
        internal static CallBackInfos       _callBackQueue      = new CallBackInfos();
        private static bool                 _continueRunning    = true;
        private static bool                 _callBackLoopRuning = false;
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

        private static Engine AllocateNewJintInstance()
        {
            var e = new Engine();
            e.SetValue("print"        , new Action<object>(Print));
            e.SetValue("setTimeout"   ,new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setTimeout__));
            e.SetValue("clearTimeout" , new Action<int>(__clearTimeout__));
            e.SetValue("setInterval"  ,new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setInterval__));
            e.SetValue("clearInterval", new Action<int>(__clearInterval__));
            return e;
        }

        public static void LoadLibrary(string name, StringBuilder source)
        {
            if (System.IO.File.Exists(name))
                source.Append(System.IO.File.ReadAllText(name)).AppendLine();
            else
                source.Append(DS.Resources.GetTextResource(name, EmbedScriptAssembly)).AppendLine();
        }

        private static JsValue ExecuteCallBack(
            Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction)
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

            Engine.Execute(source); // Execute the main script

            ScheduleCallBackEvents(); // Schedule all call back

            _callBackLoopRuning = true;
            
            // Wait until there is no more call back, setInterval will loop for ever
            while (_continueRunning && !NoMoreEvent())
            {
                System.Threading.Thread.Sleep(150);
            }
            return e;
        }

        private static void OnIntervalEvent(object source, ElapsedEventArgs e)
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

        private static bool NoMoreEvent()
        {
            return _callBackQueue.All(c => c.Timer == null);
        }

        /// <summary>
        /// Remove from the queue the call back that have been executed and are now disabled
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

        private static Thread           __StartThread = null;
        private static string           __StartSource;
        private static ExecutionEndType __StartResult;

        private static void __BackgroundThread()
        {
            var jint = AsyncronousEngine.AllocateNewJintInstance();
            var source = new StringBuilder();
            __StartResult = AsyncronousEngine.Execute(__StartSource);
            BackgroundThreadDone();
        }

        private static void BackgroundThreadDone()
        {
            __StartThread = null; // We are done
        }

        public static bool IsBackgroundThreadRunning
        {
            get { return __StartThread != null; }
        }

        /// <summary>
        /// Wait until all asynchronous event are processed
        /// </summary>
        public static void Run()
        {
            if (__StartThread != null)
            {
                while (__StartThread.IsAlive)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        public static void ClearQueue()
        {
            _callBackQueue.Stop();
            CleanCallBackEvents();
        }
        
        /// <summary>
        /// Request the event loop to stop;
        /// </summary>
        public static void Stop()
        {
            _callBackQueue.Stop();
            _continueRunning = false;
        }

        /// <summary>
        /// Kill the event loop. Kill the background thread.
        /// </summary>
        public static void Kill()
        {
            if (__StartThread != null)
                __StartThread.Abort();
        }

        /// <summary>
        /// Start the execution of multiple scripts as the Main Script, in the Jint.Ex
        /// background thread. The method returns right away.
        /// </summary>
        /// <param name="fileNames"></param>
        public static bool Start(params string[] fileNames)
        {
            if (IsBackgroundThreadRunning)
            {
                return false;
            }
            else
            {
                var source = new StringBuilder();

                foreach (var fileName in fileNames)
                    AsyncronousEngine.LoadLibrary(fileName, source);

                __StartSource      = source.ToString();
                __StartResult      = ExecutionEndType.Undefined;
                __StartThread      = new Thread(new ThreadStart(__BackgroundThread));
                __StartThread.Name = "Jint.Ex.BackgroundExecutionThread";
                __StartThread.Start();
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
