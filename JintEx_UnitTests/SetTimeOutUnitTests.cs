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
            AsyncronousEngine.Start("setIntervalSetTimeoutNested.js");
            AsyncronousEngine.Run(); // Wait until all events are processed
            Assert.AreEqual(8.0, GetJSVariable("counter"));
        }
        
        [TestMethod]
        public void setTimeoutNestedNested()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("SetTimeoutNestedNested.js");
            AsyncronousEngine.Run(); // Wait until all events are processed
            Assert.AreEqual(3.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeoutNested()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("SetTimeoutNested.js");
            AsyncronousEngine.Run(); // Wait until all events are processed
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeout()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("setTimeout.js");
            AsyncronousEngine.Run(); // Wait until all events are processed
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }
         
        [TestMethod]
        public void clearTimeout()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("clearTimeout.js");
            AsyncronousEngine.Run(); // Wait until all events are processed
            Assert.AreEqual(0.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setInterval()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("setInterval.js");
            AsyncronousEngine.Run(); // Wait until all events are processed
            var v = Jint.Ex.HelperClass.ConvertJsValueToNetValue(AsyncronousEngine.Engine.Execute("counter").GetCompletionValue());
            Assert.AreEqual(8.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void clearInterval()
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Start("clearInterval.js");
            AsyncronousEngine.Run(); // Wait until all events are processed
            Assert.AreEqual(0.0, GetJSVariable("counter"));
        }
    }
}
