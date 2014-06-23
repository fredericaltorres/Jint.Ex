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
    public class DynamicConfigUnitTests
    {
        private string GetTextResource(string fileName)
        {
            return DynamicSugarDup.DS.Resources.GetTextResource(fileName, Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        public void ConfigFile_Read_GlobalVariableBoolNumberStringAndArray()
        {
            var config = DynamicConfig.Load(GetTextResource("Config.1.js"));

            Assert.AreEqual(true                   , config.Debug);
            Assert.AreEqual(true                   , config.T);
            Assert.AreEqual(false                  , config.F);
            Assert.AreEqual(123.0                  , config.Count);
            Assert.AreEqual("http://www.google.com", config.Uri);
            Assert.AreEqual(1.0                    , config.Values[0]);
            Assert.AreEqual(2.0                    , config.Values[1]);
            Assert.AreEqual(3.0                    , config.Values[2]);
            Assert.AreEqual(true                   , config.Values[3]);
            Assert.AreEqual("abc"                  , config.Values[4]);
        }
        [TestMethod]
        public void ConfigFile_Read_NestedObjects()
        {
            var config = DynamicConfig.Load(GetTextResource("Config.2.js"));
            var pa = config.PageAppearance;
            Assert.AreEqual("TimesNewRoman", pa.Font.Name);
            Assert.AreEqual(18.0, pa.Font.Size);
            Assert.AreEqual("000000", pa.Color.Background);
            Assert.AreEqual("FFFFFF", pa.Color.Foreground);
            Assert.AreEqual(123.0, pa.getSomething());
        }
        [TestMethod]
        public void ConfigFile_Read_CallFunction()
        {
            var config = DynamicConfig.Load(GetTextResource("Config.3.js"));
            Assert.AreEqual(true                   , config.getDebug());
            Assert.AreEqual(123.0                  , config.getCount());
            Assert.AreEqual("http://www.google.com", config.getUri());
            Assert.AreEqual(1.0                    , config.getValues(0));
            Assert.AreEqual(2.0                    , config.getValues(1));
            Assert.AreEqual(3.0                    , config.getValues(2));
            Assert.AreEqual(true                   , config.getValues(3));
            Assert.AreEqual("abc"                  , config.getValues(4));
        }
    }
}
