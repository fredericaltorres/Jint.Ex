using System;
using System.Reflection;
using Jint.Ex;
using Jint.Native;
using JintEx_UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JintEx_UnitTests
{
    [TestClass]
    public class LocalStorageUnitTests
    {
        const string PopulationJsonRepresentation = @"{'bar1':{'K':'bar1','V':'foo1','A':null},'bar2':{'K':'bar2','V':'foo2','A':null}}";
        
        private const string XXXX__Set_Get_Remove_Exists_Clear = @"

            var getType = function(v) {

                var type;
                if (v === null)
                    return 'null';
                if (typeof v === 'undefined')
                    return 'undefined';
                if (typeof v === 'undefined')
                    return 'undefined';

                type = Object.prototype.toString.call(v);
                type = type.replace('[object ', '');
                type = type.replace(']', '');
                return type;
            }
            var isDate = function(v) { 
                return getType(v) === 'Date'; 
            }

            localStorage.setItem('bar1', v1);
            localStorage.setItem('bar2', v2);
            if(!localStorage.existItem('bar1')) return 1;
            if(!localStorage.existItem('bar2')) return 2;
            if(localStorage.existItem('bar3')) return 3;
        
            if(isDate(v1)) {
                if(localStorage.getItem('bar1').valueOf() !== v1.valueOf()) return 4;
                if(localStorage.getItem('bar2').valueOf() !== v2.valueOf()) return 5;
            }
            else {
                if(localStorage.getItem('bar1') !== v1) return 6;
                if(localStorage.getItem('bar2') !== v2) return 7;
            }
        
            localStorage.clear();
            if(localStorage.Count != 0) return 8;
            if(localStorage.Count !== 0) return 9;
        
            localStorage.setItem('bar1', v1);
            localStorage.setItem('bar2', v2);
            if(localStorage.Count !== 2) return 10;

           if(isDate(v1)) {
                if(localStorage.getItem('bar1').valueOf() !== v1.valueOf()) return 11;
                if(localStorage.getItem('bar2').valueOf() !== v2.valueOf()) return 12;
            }
            else {
                if(localStorage.getItem('bar1') !== v1) return 13;
                if(localStorage.getItem('bar2') !== v2) return 14;
            }
        
            localStorage.removeItem('bar1');
            localStorage.removeItem('bar2');
            if(localStorage.Count !== 0) return 15;
           
            return true;
";

        private AsyncronousEngine _asyncronousEngine;

        private string GetJSVariable(string name)
        {
            var v = Jint.Ex.HelperClass.ConvertJsValueToNetValue(_asyncronousEngine.Engine.Execute(name).GetCompletionValue());
            return v as string;
        }

        private const string jsTestFunc = @"
function test() { 
    [0] 
} 
test();
";

        private object Execute(string script)
        {
            _asyncronousEngine = new AsyncronousEngine();
            _asyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            _asyncronousEngine.Engine.SetValue("localStorage", LocalStorage.FromFile("Jint.Ex_UnitTests", _asyncronousEngine.Engine));
            
            var o = Jint.Ex.HelperClass.ConvertJsValueToNetValue(_asyncronousEngine.Execute(script));
            return o;
        }

        private object ExecuteTest(string script)
        {
            var source = jsTestFunc.Replace("[0]", script);
            return Execute(source);
        }


        [TestMethod]
        public void TypeOfLocalStorage()
        {
            var source = @"
                return typeof(localStorage);
            ";
            Assert.AreEqual("object", this.ExecuteTest(source));
        }

        [TestMethod]
        public void String__Set_Get_Remove_Exists_Clear()
        {
            var source = @"

                    var v1 = 'foo1';
                    var v2 = 'foo2';                 
            " + XXXX__Set_Get_Remove_Exists_Clear;

            Assert.AreEqual(true, this.ExecuteTest(source));
        }

        [TestMethod]
        public void Number__Set_Get_Remove_Exists_Clear()
        {
            var source = @"

                    var v1 = 12.34;
                    var v2 = 123;  
            " + XXXX__Set_Get_Remove_Exists_Clear;

            Assert.AreEqual(true, this.ExecuteTest(source));
        }

        [TestMethod]
        public void Boolean__Set_Get_Remove_Exists_Clear()
        {
            var source = @"

                    var v1 = true;
                    var v2 = false;  
            " + XXXX__Set_Get_Remove_Exists_Clear;

            Assert.AreEqual(true, this.ExecuteTest(source));
        }

        [TestMethod]
        public void Date__Set_Get_Remove_Exists_Clear()
        {
            var source = @"
                    var v1 = new Date(1990, 0, 1, 1, 2, 3);
                    var v2 = new Date(1991, 0, 1, 1, 2, 3);
            " + XXXX__Set_Get_Remove_Exists_Clear;

            var r = this.ExecuteTest(source);
            Assert.AreEqual(true, r);
        }

        [TestMethod]
        public void verifySerializerForDate()
        {
            var source = @"
                var o1 = { d: new Date(1990, 0, 1, 1, 2, 3) };
                return JSON.stringify(o1);
            ";
            var v = this.ExecuteTest(source);
            Assert.AreEqual(@"{'d':'1990-01-01T06:02:03.000Z'}".Replace("'", @""""), v);
        }

        [TestMethod]
        public void SaveObject()
        {
            var source = @"
                var o1 = { a:1, b:true, c:'hello', d: new Date(1990, 0, 1, 1, 2, 3) };
                localStorage.setItem('o1', o1);
                var oo1 = localStorage.getItem('o1');
                return JSON.stringify(oo1);
            ";

            const string expected = "{'a':1,'b':true,'c':'hello','d':'1990-01-01T06:02:03.000Z'}";

            var v = this.ExecuteTest(source);
            Assert.AreEqual(expected.Replace("'", @""""), v);
        }

        [TestMethod]
        public void GetKeys()
        {
            var source = @"
                localStorage.clear();
                localStorage.setItem('a1', 1);
                localStorage.setItem('a2', true);
                var keys = localStorage.getKeys();
                return JSON.stringify(keys);
            ";
            var v = this.ExecuteTest(source);
            Assert.AreEqual(@"[""a1"",""a2""]", v);
        }


        //[TestMethod]
        public void Indexers()
        {
            var source = @"
                localStorage.clear();
                localStorage.setItem('a1', 1);
                return localStorage['a1'];
            ";
            var v = this.ExecuteTest(source);
            Assert.AreEqual(1.0, v);
        }
    }
}
