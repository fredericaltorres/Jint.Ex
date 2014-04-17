using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint.Ex
{
    public enum KeyValuePairAttribute
    {
        Value=1,
        Object=2,
        Unknown=4
    }

    public class KeyValuePair
    {
        public string K;
        public object V;
        public KeyValuePairAttribute A;

        public override string ToString()
        {
            return string.Format("[{2}]{0}='{1}';",this.K, this.V, this.A);
        }
    }
    
    public class KeyValuePairStore : Dictionary<string, KeyValuePair>
    {
        public KeyValuePairStore()
        {

        }
        public List<KeyValuePair> GetAsList()
        {
            return this.Values.ToList();
        }
        public void Set(List<KeyValuePair> l)
        {
            foreach (var kv in l)
                this.Add(kv.K, kv);
        }
    }

    /// <summary>
    /// http://diveintohtml5.info/storage.html
    /// 
    /// For the file system implementation this class use the jtdOs.File abstraction
    /// that should be portable across operating system
    /// </summary>
    public class LocalStorage
    {
        private Jint.Engine _engine;

        public bool Trace = false;
        public string FileName { get; private set; }
        public bool AutoSave = true;

        private readonly KeyValuePairStore _store = new KeyValuePairStore();
        
        internal LocalStorage(KeyValuePairStore store, Jint.Engine engine):this(engine)
        {
            this._store = store;
        }

        public LocalStorage(Jint.Engine engine)
        {
            this._engine = engine;
        }
        
        public static LocalStorage FromJson(string json, Jint.Engine engine)
        {
            return new LocalStorage(JsonConvert.DeserializeObject<KeyValuePairStore>(json), engine);
        }

        public static LocalStorage FromFile(string fileName, Jint.Engine engine, bool clear = false)
        {
            var absFileName = MakeFileName(fileName);
            LocalStorage ls = null;

            if (System.IO.File.Exists(absFileName))
            {
                if (clear)
                {
                    ls = new LocalStorage(engine) { FileName = absFileName };
                    ls.clear();
                }
                ls = FromJson(System.IO.File.ReadAllText(absFileName), engine);
            }
            else
                ls = new LocalStorage(engine);

            ls.FileName = absFileName;
            return ls;
        }

        private static string MakeFileName(string fileName)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var absFileName = Path.Combine(folder, fileName + ".localstorage.js");
            return absFileName;
        }

        public void Delete(bool deleteFile = false)
        {
            this._store.Clear();
            this.Save();
            if (deleteFile)
                this.DeleteFile();
        }

        public void clear()
        {
            this._store.Clear();
            if(AutoSave)
                this.Save();
        }

        public int Count
        {
            get { return this._store.Count; }
        }

        public int getCount()
        {
            return this.Count;
        }
        
        private void DeleteFile()
        {
            if (System.IO.File.Exists(FileName))
                System.IO.File.Delete(FileName);
        }

        public void Save(string fileName = null)
        {
            if (fileName != null)
                this.FileName = MakeFileName(fileName);

            if(this.FileName == null)
                throw new ArgumentException(string.Format("LocalStorage filename not defined"));

            this.DeleteFile();
            System.IO.File.WriteAllText(this.FileName, this.ToJSON());
        }

        public bool existItem(string key)
        {
            return this._store.ContainsKey(key);
        }

        public KeyValuePair GetPair(string key)
        {
            return this._store[key];
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this._store, Formatting.Indented);
        }

        public void removeItem(string key)
        {
            if (this._store.ContainsKey(key))
            {
                this._store.Remove(key);
                if (AutoSave)
                    this.Save();
            }
        }

        public Jint.Native.Array.ArrayInstance getKeys()
        {
            List<object> keys = this.GetKeys();
            return Jint.Ex.HelperClass.ToJavaScriptArray(keys, _engine);
        }

        public List<object> GetKeys()
        {
            return this._store.Keys.Cast<object>().ToList();
        }

        public object this[string key]
        {
            get { return this.getItem(key); }
        }

        public object getItem(string key)
        {
            object v = null;
            if (this._store.ContainsKey(key))
            {
                var a = GetItemAttribute(key);
                v = this._store[key].V;
                if (a == KeyValuePairAttribute.Object)
                {
                    v = Jint.Ex.HelperClass.MakeObjectInstance(v as IDictionary<string, object>, _engine);
                }
            }
            else
            {
                v = null;
            }
            return v;
        }
        
        public KeyValuePairAttribute GetItemAttribute(string key)
        {
            if (this._store.ContainsKey(key))
            {
                return this._store[key].A;
            }
            else
            {
                return KeyValuePairAttribute.Unknown;
            }
        }

        public void setItem(string key, object value)
        {
            var a = KeyValuePairAttribute.Value;
            if (value is IDictionary<string, object>)
                a = KeyValuePairAttribute.Object;

            SetItem(key, value, a);
        }

        public void SetItem(string key, object value, KeyValuePairAttribute attributes)
        {
            if (this._store.ContainsKey(key))
                this._store.Remove(key);

            this._store[key] = new KeyValuePair() { K = key, V = value, A = attributes };
            if (AutoSave)
                this.Save();
        }
    }
}
