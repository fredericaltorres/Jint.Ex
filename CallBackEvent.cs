using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
//1using System.Timers;
using DynamicSugar;
using Jint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint.Native;

namespace Jint.Ex
{
    internal class CallBackEvent
    {
        private static int _timeOutIdCounter = 0;

        internal Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> Function;
        internal List<JsValue> Parameters;
        internal int Delay;
        internal int Id;
        internal CallBackType Type;
        internal bool Enabled = true;
        internal string Source;
        private int _alreadyWaited;

        internal bool ReadyForExecution(int waitedMilliSecond)
        {
            this._alreadyWaited += waitedMilliSecond;            
            if (this._alreadyWaited > this.Delay)
            {
                _alreadyWaited = 0;
                return true;
            }
            else return false;
        }
        
        internal bool Disabled
        {
            get { return !this.Enabled; }
        }

        private CallBackEvent()
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

        public CallBackEvent(CallBackType callBackType) : this()
        {
            this.Type = callBackType;
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

}
