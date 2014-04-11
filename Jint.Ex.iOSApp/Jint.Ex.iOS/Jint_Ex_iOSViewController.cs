using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Reflection;
using System.Diagnostics;

namespace Jint.Ex.iOS {
    public partial class Jint_Ex_iOSViewController : UIViewController {

        private AsyncronousEngine _asyncronousEngine;

        public Jint_Ex_iOSViewController() : base("Jint_Ex_iOSViewController", null) {

        }

        public override void DidReceiveMemoryWarning() {

            base.DidReceiveMemoryWarning();
        }

        public override void ViewDidLoad() {

            base.ViewDidLoad();
			
            txtOut.Text = string.Empty;

            _asyncronousEngine = new AsyncronousEngine();
            _asyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            // Expose to the JavaScript the function setUserMessage which add a string 
            // in the Listbox
            _asyncronousEngine.Engine.SetValue("setUserMessage", new Action<string>(__setUserMessage__));
        }

        private void __setUserMessage__(string s)
        {
            try
            {
                InvokeOnMainThread (delegate {  
                    txtOut.Text += s + Environment.NewLine;
                });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Debugger.Break();
            }
        }

        private void Trace(string s) {

            System.Diagnostics.Debug.WriteLine(s);
        }

        partial void butAsynchronousExecution_Click(NSObject sender) {
           
            _asyncronousEngine.RequestFileExecution("AsynchronousExecution.js");
        }

        partial void butSynchronousExecution_Click(NSObject sender) {

            _asyncronousEngine.RequestFileExecution("SynchronousExecution.js");
        }

        partial void butMultipleTimers_Click(NSObject sender) {

            _asyncronousEngine.RequestFileExecution("MultipleTimers.js");
        }

        partial void butTimer_Click(NSObject sender) {

            _asyncronousEngine.RequestFileExecution("Timer.js");
        }

        partial void butClearEventQueue_Click(NSObject sender) {

            txtOut.Text = string.Empty;
            _asyncronousEngine.RequestClearQueue();
        }
         
    }
}


 