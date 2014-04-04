﻿using System;
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

        /// <summary>
        /// Execute the loading of the string in a background thread and then push a request to
        /// execute the callback function
        /// </summary>
        private void __BackgroundThread()
        {
            var s = read();
            AsyncronousEngine.RequestCallbackExecution(_callBackFunction, new List<JsValue>() { s });
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
            _callBackFunction = callBackFunction;
            var t = new Thread(new ThreadStart(__BackgroundThread));
            t.Start();
            return null;
        }
    }


    [TestClass]
    public class AsyncApiUnitTests
    {
        private object GetJSVariable(string name)
        {
            var v = Jint.Ex.HelperClass.ConvertJsValueToNetValue(AsyncronousEngine.Engine.Execute(name).GetCompletionValue());
            return v;
        }
        private void RunScript(string script)
        {
            AsyncronousEngine.Reset();
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Engine.SetValue("storage", new Storage());
            AsyncronousEngine.RequestExecution(script, true);
        }
        [TestMethod]
        public void Storage_Sync()
        {
            RunScript("storage.sync.js");
            var s = GetJSVariable("s") as String;
            Assert.AreEqual(409800, s.Length);
        }
        [TestMethod]
        public void Storage_Async()
        {
            RunScript("storage.async.js");
            var s = GetJSVariable("s") as String;
            Assert.AreEqual(409800, s.Length);
        }
    }
}