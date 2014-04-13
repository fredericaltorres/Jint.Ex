using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Reflection;
using System.Diagnostics;

namespace Jint.Ex.iOS {
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate {
        // class-level declarations
        UINavigationController navigation;
        UIWindow window;

        //Jint_Ex_iOSViewController viewController;
 
        private AsyncronousEngine _asyncronousEngine;
        private Section _outputSection;
        Random rnd;

        private void InitAsyncronousEngine() {

            rnd = new Random();
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
                    System.Diagnostics.Debug.WriteLine(s);
                    _outputSection.Insert (0,//rnd.Next (0, _outputSection.Elements.Count),
                                           UITableViewRowAnimation.None, new StringElement (s));
                });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Debugger.Break();
            }
        }

        public void SynchronousExecution() {

            _asyncronousEngine.RequestFileExecution("SynchronousExecution.js");
        }

        public void AsynchronousExecution() {

            _asyncronousEngine.RequestFileExecution("AsynchronousExecution.js");
        }

        public void Timer() {

            _asyncronousEngine.RequestFileExecution("Timer.js");
        }

        public void MultipleTimers() {

            _asyncronousEngine.RequestFileExecution("MultipleTimers.js");
        }

        public void ClearOuput() {

            _outputSection.RemoveRange(0, _outputSection.Elements.Count);
        }

        public void ClearEventQueue() {

            ClearOuput();
            _asyncronousEngine.RequestClearQueue();
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options) {

            InitAsyncronousEngine();

            _outputSection = new Section("Output") {

            };

            var menu = new RootElement ("Jint and Jint.Ex iOS Demo") {
                new Section ("Menu"){
                    new StringElement ("Synchronous Execution", SynchronousExecution),
                    new StringElement ("Asynchronous Execution", AsynchronousExecution),
                    new StringElement ("Timer", Timer),
                    new StringElement ("Multiple Timer", MultipleTimers),
                    new StringElement ("Clear Output", ClearOuput),
                    new StringElement ("Clear Event Queue", ClearEventQueue),
                },
                _outputSection
            };

            var dv = new DialogViewController (menu) {
                Autorotate = true
            };
            navigation = new UINavigationController ();
            navigation.PushViewController (dv, true);               

            // On iOS5 we use the new window.RootViewController, on older versions, we add the subview
            window = new UIWindow (UIScreen.MainScreen.Bounds);
            window.MakeKeyAndVisible ();
            if (UIDevice.CurrentDevice.CheckSystemVersion (5, 0))
                window.RootViewController = navigation; 
            else
                window.AddSubview (navigation.View);

            return true;
        }
        /* public override bool FinishedLaunching(UIApplication app, NSDictionary options) {


            window = new UIWindow(UIScreen.MainScreen.Bounds);
			
            viewController = new Jint_Ex_iOSViewController();
            window.RootViewController = viewController;
            window.MakeKeyAndVisible();
			
            return true;
        }*/
    }

}

