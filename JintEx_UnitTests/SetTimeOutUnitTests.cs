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
            AsyncronousEngine.RequestExecution("setIntervalSetTimeoutNested.js", true);
            Assert.AreEqual(8.0, GetJSVariable("counter"));
        }
        
        [TestMethod]
        public void setTimeoutNestedNested()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.RequestExecution("SetTimeoutNestedNested.js", true);
            Assert.AreEqual(3.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeoutNested()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.RequestExecution("SetTimeoutNested.js", true);
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeout_1()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.RequestExecution("setTimeout.1.js", true);
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeout_3()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.RequestExecution("setTimeout.3.js", true);
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }
         
        [TestMethod]
        public void clearTimeout()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.RequestExecution("clearTimeout.js", true);
            Assert.AreEqual(0.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setInterval()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.RequestExecution("setInterval.js", true);
            var v = Jint.Ex.HelperClass.ConvertJsValueToNetValue(AsyncronousEngine.Engine.Execute("counter").GetCompletionValue());
            Assert.AreEqual(8.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void clearInterval()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.RequestExecution("clearInterval.js", true);
            Assert.AreEqual(0.0, GetJSVariable("counter"));
        }
    }
}
