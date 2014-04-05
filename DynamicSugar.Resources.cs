using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using System.Reflection;


namespace DynamicSugar {

    /// <summary>
    /// Dynamic Sharp Helper Class, dedicated methods to work with text resource file
    /// </summary>
    public static partial class DS {

        public enum TextResourceEncoding {
            Ascii,
            Unicode,
            UTF8
        }

        public static class Resources {
            /// <summary>
            /// Return the fully qualified name of the resource file
            /// </summary>
            /// <param name="resourceFileName">File name of the resource</param>
            /// <returns></returns>
            private static string GetResourceFullName(string resourceFileName, Assembly assembly) {

                foreach (var resource in assembly.GetManifestResourceNames())
                    if (resource.EndsWith("." + resourceFileName) || resource == resourceFileName)
                        return resource;
                throw new System.ApplicationException(string.Format("Resource '{0}' not find in assembly '{1}'", resourceFileName, Assembly.GetExecutingAssembly().FullName));
            }

            /// <summary>
            /// Return the content of a text file embed as a resource.
            /// The function takes care of finding the fully qualify name, in the first
            /// assembly when the resource is found
            /// </summary>
            /// <param name="resourceFileName">The file name of the resource</param>
            /// <param name="assemblies">A list of assemblies in which to search for the resources</param>
            /// <returns></returns>
            public static string GetTextResource(string resourceFileName, List<Assembly> assemblies)
            {
                System.Exception lastEx = null;
                foreach (var a in assemblies)
                {
                    try
                    {
                        return GetTextResource(resourceFileName, a);
                    }
                    catch (System.Exception ex)
                    {
                        lastEx = ex;
                    }
                }
                throw lastEx;
            }

            /// <summary>
            /// Return the content of a text file embed as a resource.
            /// The function takes care of finding the fully qualify name, in the current
            /// assembly.
            /// </summary>
            /// <param name="resourceFileName">The file name of the resource</param>
            /// <returns></returns>
            public static string GetTextResource(string resourceFileName, Assembly assembly) {
                bool gzip = false;
                TextResourceEncoding encoding = TextResourceEncoding.Unicode;
                var resourceFullName = GetResourceFullName(resourceFileName, assembly);

                if(gzip) {
                    var buffer        = GetBinaryResource(resourceFileName, assembly);
                    var text          = string.Empty;
                    var bufferUnziped = buffer;
                    switch(encoding) {
                        case TextResourceEncoding.Ascii  : text = Encoding.ASCII.GetString(buffer, 0, buffer.Length);break;
                        case TextResourceEncoding.Unicode: text = Encoding.Unicode.GetString(buffer, 0, buffer.Length);break;
                        case TextResourceEncoding.UTF8   : text = Encoding.UTF8.GetString(buffer, 0, buffer.Length);break;
                    }
                    
                    return text;
                }
                else {
                    using (var _textStreamReader = new StreamReader(assembly.GetManifestResourceStream(resourceFullName)))
                        return _textStreamReader.ReadToEnd();
                }
            }
            /// <summary>
            /// Return multiple text files embed as a resource in a dictionary.
            /// The key in the resource name, the value is the text data
            /// The function takes care of finding the fully qualify name, in the passed
            /// assembly.
            /// </summary>
            /// <param name="regex">The regular expression to filter the resource by name. The file system '\' are replaced with '.'</param>
            /// <returns></returns>
            public static Dictionary<string, string> GetTextResource(System.Text.RegularExpressions.Regex regex , Assembly assembly) {
                bool gzip = false;
                TextResourceEncoding encoding = TextResourceEncoding.Unicode;

                var dic   = new Dictionary<string, string> ();
                var names = new List<string>();

                foreach (var resource in assembly.GetManifestResourceNames())
                    if (regex.IsMatch(resource))
                        names.Add(resource);

                foreach(var name in names)
                       dic.Add(name, GetTextResource(name, assembly));

                return dic;
            }
            /// <summary>
            /// Return the content of a file embed as a resource.
            /// The function takes care of finding the fully qualify name, in the current
            /// assembly.
            /// </summary>
            /// <param name="resourceFileName"></param>
            /// <param name="assembly"></param>
            /// <returns></returns>
            public static byte[] GetBinaryResource(string resourceFileName, Assembly assembly)
            {
                var resourceFullName = GetResourceFullName(resourceFileName, assembly);            
                var stream           = assembly.GetManifestResourceStream(resourceFullName);
                byte[]  data         = new Byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
                return data;
            }
#if !MONOTOUCH			
            
#endif			
            /// <summary>
            /// Save a buffer of byte into a file
            /// </summary>
            /// <param name="byteArray"></param>
            /// <param name="fileName"></param>
            /// <returns></returns>
            private static bool SaveByteArrayToFile(byte[] byteArray, string fileName) {
                try {
                    using (Stream fileStream = File.Create(fileName)) {
                        fileStream.Write(byteArray, 0, byteArray.Length);
                        fileStream.Close();
                        return true;
                    }
                }
                catch{
                    return false;
                }
            }

            /// <summary>
            /// Save resources as a local files
            /// </summary>
            /// <param name="assembly">Assembly where to get the resource</param>
            /// <param name="path">Local folder</param>
            /// <param name="resourceFileNames">Resource name and filename</param>
            /// <returns></returns>
            public static Dictionary<string, string> SaveBinaryResourceAsFiles(Assembly assembly, string path, params string[] resourceFileNames) {

                var dic = new Dictionary<string, string>();

                foreach (var r in resourceFileNames)
                    dic[r] = SaveBinaryResourceAsFile(assembly, path, r);

                return dic;
            }
            /// <summary>
            /// Save a resource as a local file
            /// </summary>
            /// <param name="resourceFileName">Resource name and filename</param>
            /// <param name="assembly">Assembly where to get the resource</param>
            /// <param name="path">Local folder</param>
            /// <returns></returns>
            public static string SaveBinaryResourceAsFile(Assembly assembly, string path, string resourceFileName) {

                var outputFileName = String.Format(@"{0}\{1}", path, resourceFileName);
                if (System.IO.File.Exists(outputFileName))
                    return outputFileName;

                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                var buffer = GetBinaryResource(resourceFileName, assembly);

                SaveByteArrayToFile(buffer, outputFileName);
                return outputFileName;
            }
        }
    }
}