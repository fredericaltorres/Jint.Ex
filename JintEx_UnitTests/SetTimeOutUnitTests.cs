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
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            AsyncronousEngine.RequestScriptFileExecution("setIntervalSetTimeoutNested.js", true);
            Assert.AreEqual(8.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeoutNestedNested()
        {
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            AsyncronousEngine.RequestScriptFileExecution("SetTimeoutNestedNested.js", true);
            Assert.AreEqual(3.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeoutNested()
        {
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            AsyncronousEngine.RequestScriptFileExecution("SetTimeoutNested.js", true);
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeout_1()
        {
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            AsyncronousEngine.RequestScriptFileExecution("setTimeout.1.js", true);
            Assert.AreEqual(1.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setTimeout_3()
        {
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            AsyncronousEngine.RequestScriptFileExecution("setTimeout.3.js", true);
            Assert.AreEqual(2.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void clearTimeout()
        {
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            AsyncronousEngine.RequestScriptFileExecution("clearTimeout.js", true);
            Assert.AreEqual(0.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void setInterval()
        {
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            AsyncronousEngine.RequestScriptFileExecution("setInterval.js", true);
            Assert.AreEqual(4.0, GetJSVariable("counter"));
        }

        [TestMethod]
        public void clearInterval()
        {
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            AsyncronousEngine.RequestScriptFileExecution("clearInterval.js", true);
            Assert.AreEqual(0.0, GetJSVariable("counter"));
        }
    }
}
