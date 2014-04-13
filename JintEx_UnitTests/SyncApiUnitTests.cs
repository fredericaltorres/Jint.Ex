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
    
    [TestClass]
    public class SyncApiUnitTests
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
            _asyncronousEngine.Engine.SetValue("storage", new Storage(_asyncronousEngine));
        }
        private void RunScript(string script)
        {
            Init();
            _asyncronousEngine.RequestFileExecution(script, block: true);
        }
        //[TestMethod]
        public void Storage_Sync()
        {
            RunScript("storage.sync.js");
            Assert.AreEqual(409800, GetJSVariable("s").Length);
        }
        [TestMethod]
        public void LoadScript()
        {
            const int MinSize = 35;
            Init();
            var text = _asyncronousEngine.LoadScript("storage.sync.js");
            Assert.IsTrue(text.Length > MinSize);

            var s = new StringBuilder();
            _asyncronousEngine.LoadScript("storage.sync.js", s);
            Assert.IsTrue(s.ToString().Length > MinSize);
        }

    }
}
