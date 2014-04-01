using System;
using Jint.Ex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace JintEx_UnitTests
{
    [TestClass]
    public class SetTimeOut_SetInterval_UnitTests
    {
        private object GetJSVariable(string name)
        {
            var v = Jint.Ex.HelperClass.ConvertJsValueToNetValue(AsyncronousEngine.Engine.Execute(name).GetCompletionValue());
            return v;
        }
        
        [TestMethod]
        public void setIntervalSetTimeoutNested()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("setIntervalSetTimeoutNested.js", true);
            Assert.AreEqual(8.0, GetJSVariable("counter"));
        }
        
        [TestMethod]
        public void setTimeoutNestedNested()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("SetTimeoutNestedNested.js", true);
            Assert.AreEqual(3.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeoutNested()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("SetTimeoutNested.js", true);
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeout()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("setTimeout.js", true);
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }
         
        [TestMethod]
        public void clearTimeout()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("clearTimeout.js", true);
            Assert.AreEqual(0.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setInterval()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("setInterval.js", true);
            var v = Jint.Ex.HelperClass.ConvertJsValueToNetValue(AsyncronousEngine.Engine.Execute("counter").GetCompletionValue());
            Assert.AreEqual(8.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void clearInterval()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("clearInterval.js", true);
            Assert.AreEqual(0.0, GetJSVariable("counter"));
        }
    }
}
