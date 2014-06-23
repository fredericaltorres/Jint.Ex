using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if !__IOS__

namespace Jint.Ex
{
    /// <summary>
    /// Implement configuration file in JavaScript
    /// Use the Jint v 2.x JavaScript runtime
    /// </summary>
    public class DynamicConfig : DynamicObject
    {
        Engine _engine;

        public static dynamic LoadFile(string fileName = "config.js")
        {
            return Load(System.IO.File.ReadAllText(fileName));
        }

        public static dynamic Load(string source)
        {
            return new DynamicConfig(source);
        }
        
        private DynamicConfig(string source) 
        {
            this._engine = this.GetJint();
            this._engine.Execute(source);
        }

        private static void Print(object s)
        {
            if (s == null)
                s = "null";
            Console.WriteLine(s.ToString());
        }

        private Engine GetJint()
        {
            _engine = new Engine();
            _engine.SetValue("print", new Action<object>(Print));
            return _engine;
        }

        private string GetFullPathConfigJsFilename(string fileName = "config.js")
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(currentPath, fileName);
        }

        private Jint.Native.JsValue __GetJsValue(string property)
        {
            return this._engine.Execute(property).GetCompletionValue();
        }

        internal static object ConvertJsValueToNetObject(Jint.Native.JsValue v)
        {
            if (v.IsBoolean())
                return v.AsBoolean();
            if (v.IsNumber())
                return v.AsNumber();
            if (v.IsNull())
                return null;
            if (v.IsUndefined())
                return "undefined";
            if (v.IsString())
                return v.AsString();
            if (v.IsObject())
                return v.ToObject();
            throw new ArgumentException(string.Format("Cannot get display value for JsValue"));
        }

        internal static JsValue CallFunction(Jint.Native.Function.FunctionInstance f, object[] args, Jint.Engine engine)
        {
            var jsValue = f.Call(JsValue.Undefined, HelperClass.MakeJsValues(args.ToList(), engine).ToArray());
            return jsValue;
        }

        internal static bool IsFunction(JsValue jsValue)
        {
            return jsValue.IsObject() && jsValue.AsObject() is Jint.Native.Function.FunctionInstance;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var pname   = binder.Name;
            var jsValue = __GetJsValue(pname);
            result      = null;

            if (DynamicConfig.IsFunction(jsValue)) 
            {
                var f = jsValue.AsObject() as Jint.Native.Function.FunctionInstance;
                jsValue = DynamicConfig.CallFunction(f, args, this._engine);
                if (jsValue.IsObject())
                    result = new DynamicJsValueForDotNet(jsValue.AsObject(), this._engine);
                else
                    result = DynamicConfig.ConvertJsValueToNetObject(jsValue);
                return true;
            }
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var pname   = binder.Name;
            var jsValue = __GetJsValue(pname);

            if (jsValue.IsObject()) 
            {
                result = new DynamicJsValueForDotNet(jsValue.AsObject(), this._engine);
            }
            else 
                result = ConvertJsValueToNetObject(jsValue);

            return true;
        }
    }

    public class DynamicJsValueForDotNet : DynamicObject
    {
        JsValue _jsValue;
        Jint.Engine _engine;

        public DynamicJsValueForDotNet(JsValue jsValue, Jint.Engine engine)
        {
            this._jsValue = jsValue;
            this._engine = engine;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var pname   = binder.Name;
            var jsValue = __GetJsValue(pname);
            result      = null;

            if (DynamicConfig.IsFunction(jsValue)) 
            {
                var f = jsValue.AsObject() as Jint.Native.Function.FunctionInstance;
                jsValue = DynamicConfig.CallFunction(f, args, this._engine);
                if (jsValue.IsObject())
                    result = new DynamicJsValueForDotNet(jsValue.AsObject(), this._engine);
                else
                    result = DynamicConfig.ConvertJsValueToNetObject(jsValue);
                return true;
            }
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = null;

            var o = this._jsValue.AsObject();

            if (o is Jint.Native.Array.ArrayInstance)
            {
                var ai = o as Jint.Native.Array.ArrayInstance;
                var v = ai.Get(indexes[0].ToString());
                var vo = DynamicConfig.ConvertJsValueToNetObject(v);
                result = vo;
            }
            else
            {
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return true;
        }

        public Jint.Native.JsValue __GetJsValue(string property)
        {
            Jint.Native.Object.ObjectInstance o = this._jsValue.AsObject();
            var jsValue = o.Get(property);
            return jsValue;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var pname   = binder.Name;
            var jsValue = __GetJsValue(pname);

            if (jsValue.IsObject())
                result = new DynamicJsValueForDotNet(jsValue.AsObject(), this._engine);
            else
                result = DynamicConfig.ConvertJsValueToNetObject(jsValue);

            return true;
        }
    }   
}

#endif

/*
 
        public bool Tracing(string property, bool defaultValue = false)
        {
            var jsVal = __Tracing(property);
            if (jsVal.IsBoolean())
                return jsVal.AsBoolean();
            else
                return defaultValue;
        }

        public int Tracing(string property, int defaultValue = 0)
        {
            var jsVal = __Tracing(property);
            if (jsVal.IsNumber())
                return (int)jsVal.AsNumber();
            else
                return defaultValue;
        }

        public double Tracing(string property, double defaultValue = 0)
        {
            var jsVal = __Tracing(property);
            if (jsVal.IsNumber())
                return jsVal.AsNumber();
            else
                return defaultValue;
        }

        public string Tracing(string property, string defaultValue = "")
        {
            var jsVal = __Tracing(property);
            if (jsVal.IsNumber())
                return jsVal.AsString();
            else
                return defaultValue;
        }

        public Jint.Native.JsValue __Tracing(string property) 
        { 
            var code    = String.Format("Tracing.{0};", property);
            var jsValue = this._engine.Execute(code);
            return jsValue;
        }
 
 */

