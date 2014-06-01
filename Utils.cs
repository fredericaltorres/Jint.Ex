using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
//1using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Jint.Native;
using Jint.Native.Array;
using Jint.Native.Json;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;

namespace Jint.Ex
{
    public static class HelperClass
    {
        public static int GetLength(this Jint.Native.Array.ArrayInstance a)
        {
            var l = a.Get("length");
            return (int)l.AsNumber();
        }

        public static object GetItem(this Jint.Native.Array.ArrayInstance a, int index)
        {
            JsValue l = a.Get(index.ToString());

            if (l.IsNull()) return null;
            if (l.IsNumber()) return l.AsNumber();
            if (l.IsBoolean()) return l.AsBoolean();
            if (l.IsString()) return l.AsString();
            if (l.IsNumber()) return l.AsNumber();
            if (l.IsUndefined()) return "undefined";
            if (l.IsObject()) return l.AsObject();
            if (l.IsRegExp()) return l.AsObject();

            throw new ArgumentException(string.Format("Cannot deal with JsValue {0}", l));
        }

        public static List<object> AsListOfObjects(this Jint.Native.Array.ArrayInstance a)
        {
            var values = new List<object>();
            for (var i = 0; i < a.GetLength(); i++)
            {
                values.Add(a.GetItem(i));
            }
            return values;
        }

        public static List<string> AsListOfString(this Jint.Native.Array.ArrayInstance a)
        {
            var values = new List<string>();
            for (var i = 0; i < a.GetLength(); i++)
            {
                values.Add(a.Get(i.ToString()).AsString());
            }
            return values;
        }

        public static T ConvertJsValueToNetValue<T>(object arg0, T defaultValue)
        {
            var v = ConvertJsValueToNetValue(arg0);

            if (v is T)
            {
                T t = (T) Convert.ChangeType(v, typeof (T));
                return t;
            }
            else 
                return defaultValue;
        }

        public static object ConvertJsValueToNetValue(object arg0)
        {
            object r = null;

            if (arg0 is double || arg0 is bool || arg0 is string)
            {
                r = arg0;
            }
            else if (arg0 is JsValue)
            {
                var jsa = (JsValue)arg0;
                     if (jsa.IsNull())      r = null;
                else if (jsa.IsUndefined()) r = null;
                else if (jsa.IsString())    r = jsa.AsString();
                else if (jsa.IsNumber())    r = jsa.AsNumber();
                else if (jsa.IsBoolean())   r = jsa.AsBoolean();
                else if (jsa.IsArray())     r = jsa.AsArray().AsListOfObjects();

            }
            else if (arg0 is Jint.Native.RegExp.RegExpInstance)
            {
                var regexp = arg0 as Jint.Native.RegExp.RegExpInstance;
                r = regexp.Value;
            }
            else if (arg0 is Jint.Native.Date.DateInstance)
            {
                var date = arg0 as Jint.Native.Date.DateInstance;
                r = date.ToDateTime();
            }
            else if (arg0 is Jint.Native.Array.ArrayInstance)
            {
                var array0 = arg0 as Jint.Native.Array.ArrayInstance;
                r = array0.AsListOfObjects();
            }
            else if (arg0 is Jint.Native.Object.ObjectInstance)
            {
                var obj0 = arg0 as Jint.Native.Object.ObjectInstance;
                r = obj0;
            }
            else 
                r = arg0;

            return r;
        }

        public static Jint.Native.Object.ObjectInstance MakeObjectInstance(IDictionary<string, object> dic, Jint.Engine engine)
        {        
            var o = new Jint.Native.Object.ObjectInstance(engine);
            foreach (var i in dic)
            {
                o.FastAddProperty(i.Key, MakeJsValue(i.Value, engine), true, true, true);
            }
            return o;
        }

#if !__IOS__
        public static Jint.Native.Object.ObjectInstance MakeObjectInstance(System.Dynamic.ExpandoObject expandoO, Jint.Engine engine)
        {
            var dic = expandoO as IDictionary<string, object>;
            return MakeObjectInstance(dic, engine);
        }
#endif

        public static JsValue MakeJsValue(object o, Jint.Engine engine)
        {
            var j = JsValue.FromObject(engine, o);
            return j;
        }

        public static ObjectInstance ObjectFromJson(string json, Jint.Engine engine)
        {
            var p = new JsonParser(engine);
            var jsValue = p.Parse(json);
            var o = jsValue.TryCast<ObjectInstance>();
            if (o == null)
                throw new ArgumentException(string.Format("json cannot parsed into an object. json:{0}", json));
            return o;
        }

        public static string MakeJsonString(string s)
        {
            return String.Format(@"""{0}""", s.Replace(@"""", @"\"""));
        }

        public static Jint.Native.Array.ArrayInstance ToJavaScriptArray(List<string> values, Jint.Engine engine)
        {
            var lo = new List<object>();
            foreach (var v in values)
                lo.Add(v);
            return ToJavaScriptArray(lo, engine);
        }

        public static Jint.Native.Array.ArrayInstance ToJavaScriptArray(List<object> values, Jint.Engine engine)
        {
            var jsValues = new List<JsValue>();
            foreach (var v in values)
                jsValues.Add(JsValue.FromObject(engine, v));
            return (ArrayInstance)engine.Array.Construct(jsValues.ToArray());
        }

        public static string ToJson(ObjectInstance objInstance, Jint.Engine jintEngine)
        {
            var o = objInstance as Jint.Native.Object.ObjectInstance;
            var s = new JsonSerializer(jintEngine);
            var ojson = s.Serialize(o, JsValue.Undefined, JsValue.Undefined).AsString();
            return ojson;
        }

        public static string ToJson(JsValue j, Jint.Engine jintEngine)
        {
            var s = new JsonSerializer(jintEngine);
            var ojson = s.Serialize(j, JsValue.Undefined, JsValue.Undefined).AsString();
            return ojson;
        }

        public static string ProcessQuoteAndWindowsNewLine(string s)
        {
            if (s == null)
                return null;

            return s.Replace(@"'", @"\'").
                        Replace(@"""", @"\""").
                        Replace("\r", @"\r").
                        Replace("\n", @"\n");
        }
    }
}
