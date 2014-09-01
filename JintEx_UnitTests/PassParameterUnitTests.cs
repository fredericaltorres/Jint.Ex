using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Jint.Ex;
using Jint.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Collections.Generic;

namespace JintEx_UnitTests
{
    public class Extension1
    {
        public static double d;
        public static bool b;
        public static string s;

        private AsyncronousEngine _asyncronousEngine;

        public Extension1(AsyncronousEngine asyncronousEngine)
        {
            this._asyncronousEngine = asyncronousEngine;
        }
       
        public bool passObject(object o)
        {
            if (o is IDictionary<string, object>)
            {
                var dic      = o as IDictionary<string, object>;
                Extension1.d = (double)dic["d"];
                Extension1.b = (bool)dic["b"];
                Extension1.s = (string)dic["s"];
            }
            return true;
        }
    }
    
    [TestClass]
    public class PassParameterUnitTests
    {
        private AsyncronousEngine _asyncronousEngine;

        private string GetJSVariable(string name)
        {
            var v = Jint.Ex.HelperClass.ConvertJsValueToNetValue(_asyncronousEngine.Engine.Execute(name).GetCompletionValue());
            return v as string;
        }
        private void Init()
        {
            _asyncronousEngine = new AsyncronousEngine();
            _asyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            _asyncronousEngine.Engine.SetValue("extension1", new Extension1(_asyncronousEngine));
        }
        private void RunScript(string script)
        {
            Init();
            _asyncronousEngine.RequestFileExecution(script, block: true);
        }
        [TestMethod]
        public void PassObject_Pass()
        {
            RunScript("PassObject.js");
            Assert.AreEqual(1.0, Extension1.d);
            Assert.AreEqual(true, Extension1.b);
            Assert.AreEqual("ok", Extension1.s);
        }
    }
}
