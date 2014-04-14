//#define WSCRIPT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jint.Ex;

namespace JintSetTimeoutDemo
{
    class Program
    {
        static void SetIntervalDemo()
        {
            Console.WriteLine("Jint setInterval() demo");

            var ae = new AsyncronousEngine();

            ae.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            ae.RequestFileExecution("setIntervalSetTimeoutNested.js");
            
            Console.WriteLine("Hit a key to stop");
            Console.ReadKey();
            ae.RequestClearQueue();

            Console.WriteLine("*** Done ***");
            Console.ReadKey();
        }

        static void SetTimeoutDemo()
        {
            Console.WriteLine("Jint setTimeout() demo");

            var ae = new AsyncronousEngine();

            ae.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            ae.RequestFileExecution("setTimeout.js");

            Console.WriteLine("Hit a key tp stop");
            Console.ReadKey();

            ae.RequestClearQueue();

            Console.WriteLine("*** Done ***");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            //SetTimeoutDemo();
            SetIntervalDemo();
        }
    }
}
