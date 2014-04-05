using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jint.Ex
{
    internal class CallBackEventQueue : IEnumerable<CallBackEvent>
    {
        private static readonly Object obj = new Object();
        private List<CallBackEvent> _queue = new List<CallBackEvent>();

        System.Collections.IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<CallBackEvent> GetEnumerator()
        {
            return ((IEnumerable<CallBackEvent>) _queue).GetEnumerator();
        }

        public int Count {
            get { return this._queue.Count; }
        }

        public CallBackEventQueue Clone()
        {
            lock (obj)
            {
                var q = new CallBackEventQueue();
                foreach (var e in _queue)
                    _queue.Add(e);
                return q;
            }
        }
        public void RemoveDisabledEvents()
        {
            lock (obj)
            {
                var goOn = true;
                while (goOn)
                {
                    goOn = false;
                    foreach (var e in this._queue)
                    {
                        if (e.Disabled)
                        {
                            this._queue.Remove(e);
                            goOn = true;
                            break;
                        }
                    }
                }
            }
        }
        public CallBackEventQueue GetEventsWithNoDelay()
        {
            lock (obj)
            {
                var q = new CallBackEventQueue();
                foreach (var e in _queue)
                    if (e.Delay == 0 && e.Enabled)
                        q.EnqueueNotSafe(e);
                return q;
            }
        }
        public CallBackEventQueue CloneEventWithDelay()
        {
            lock (obj)
            {
                var q = new CallBackEventQueue();
                foreach (var e in _queue)
                    if (e.Delay > 0 && e.Enabled)
                        q.EnqueueNotSafe(e);
                return q;
            }
        }
        public void Clear()
        {
            // No need to lock because this method is only called by
            // the event loop
            this._queue.Clear();            
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
        internal void EnqueueNotSafe(CallBackEvent c)
        {
            this._queue.Add(c);            
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
        public void RemoveTopBecauseProcessed(CallBackEvent c, bool mainQueue)
        {
            lock (obj)
            {
                var cc = this._queue.Find(e => e == c);
                if (cc != null)
                    this._queue.Remove(cc);

                if (mainQueue)
                    if (c.Type == CallBackType.Interval) // for Interval we re add the event at then end of the queue
                        this.Enqueue(c);
            }
        }

    }
}