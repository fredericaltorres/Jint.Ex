using System;
using System.Text;
using System.Threading;
using Jint.Ex;
using Jint.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Collections.Generic;

namespace JintEx_UnitTests
{
    /// <summary>
    /// A test class that implement the method read() in synchronous mode and asynchronous mode
    /// </summary>
    public class Storage
    {
        private Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _callBackFunction;
        private AsyncronousEngine _asyncronousEngine;

        public Storage(AsyncronousEngine asyncronousEngine)
        {
            this._asyncronousEngine = asyncronousEngine;
        }

        /// <summary>
        /// Execute the reading of the string in a background thread and then request the
        /// execution of the callback function
        /// </summary>
        private void __BackgroundThread(object p)
        {
            var s = read();
            this._asyncronousEngine.RequestCallbackExecution(_callBackFunction, new List<JsValue>() { s });
        }
        /// <summary>
        /// Synchronous api
        /// </summary>
        /// <returns></returns>
        public string read()
        {
            var s = new StringBuilder(1000);
            for (var i = 0; i < 100; i++)
            {
                s.AppendFormat("{0}", "".PadLeft(4096, '*')).AppendLine();
            }
            return s.ToString();
        }
        /// <summary>
        /// Asynchronous api
        /// </summary>
        /// <param name="callBackFunction"></param>
        /// <returns></returns>
        public string read(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction)
        {
            this._callBackFunction = callBackFunction;
            ThreadPool.QueueUserWorkItem(new WaitCallback(__BackgroundThread), null);
            return null;
        }
    }

    [TestClass]
    public class AsyncApiUnitTests
    {
        private AsyncronousEngine _asyncronousEngine;

        private string GetJSVariable(string name)
        {
            var v = Jint.Ex.HelperClass.ConvertJsValueToNetValue(_asyncronousEngine.Engine.Execute(name).GetCompletionValue());
            return v as string;
        }
        private void RunScript(string script)
        {
            _asyncronousEngine = new AsyncronousEngine();
            _asyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            _asyncronousEngine.Engine.SetValue("storage", new Storage(_asyncronousEngine));
            _asyncronousEngine.RequestFileExecution(script, block: true);
        }
        [TestMethod]
        public void Storage_Sync()
        {
            RunScript("storage.sync.js");
            Assert.AreEqual(409800, GetJSVariable("s").Length);
        }
        [TestMethod]
        public void Storage_Async()
        {
            RunScript("storage.async.js");
            Assert.AreEqual(409800, GetJSVariable("s").Length);
        }
    }
}
