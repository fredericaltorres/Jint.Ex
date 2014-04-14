using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Reflection;

namespace Jint.Ex.iOSApp
{
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        UINavigationController navigation;
        UIWindow window;
        
        Section _outputSection;

        AsyncronousEngine _asyncronousEngine;

        private void InitializeAsyncronousEngine()
        {
            _asyncronousEngine = new AsyncronousEngine();
            _asyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            // Expose to the JavaScript world the function setUserMessage which add a a message to the UI
            _asyncronousEngine.Engine.SetValue("setUserMessage", new Action<string, bool>(__setUserMessage__));
        }
      
        private void __setUserMessage__(string s, bool replace)
        {
            InvokeOnMainThread(delegate
            {
                if (replace)
                {
                    if (_outputSection.Elements.Count == 0)
                    {
                        _outputSection.Insert(0, UITableViewRowAnimation.None, new StringElement(s));
                    }
                    _outputSection.RemoveRange(0, 1);
                    _outputSection.Insert(0, UITableViewRowAnimation.None, new StringElement(s));    
                }
                else
                {
                    _outputSection.Insert(0, UITableViewRowAnimation.None, new StringElement(s));    
                }
                
            });
        }

        public void SynchronousExecution()
        {
            _asyncronousEngine.RequestFileExecution("SynchronousExecution.js");
        }

        public void AsynchronousExecution()
        {
            _asyncronousEngine.RequestFileExecution("AsynchronousExecution.js");
        }

        public void Timer()
        {
            _asyncronousEngine.RequestFileExecution("Timer.js");
        }

        public void MultipleTimers()
        {
            _asyncronousEngine.RequestFileExecution("MultipleTimers.js");
        }

        public void ClearOuput()
        {
            _outputSection.RemoveRange(0, _outputSection.Elements.Count);
        }

        public void ClearEventQueue()
        {
            ClearOuput();
            _asyncronousEngine.RequestClearQueue();
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            this.InitializeAsyncronousEngine();
            this._outputSection = new Section("Output");

            var menu = new RootElement("Jint and Jint.Ex iOS Demo") {
                new Section ("Menu") {
                    new StringElement ("Synchronous Execution" , SynchronousExecution),
                    new StringElement ("Asynchronous Execution", AsynchronousExecution),
                    new StringElement ("Timer"                 , Timer),
                    new StringElement ("Multiple Timer"        , MultipleTimers),
                    new StringElement ("Clear Output"          , ClearOuput),
                    new StringElement ("Clear Event Queue"     , ClearEventQueue),
                },
                _outputSection
            };

            var dv     = new DialogViewController(menu) { Autorotate = true };
            navigation = new UINavigationController();
            navigation.PushViewController(dv, true);

            // On iOS5 we use the new window.RootViewController, on older versions, we add the subview
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            window.MakeKeyAndVisible();
            if (UIDevice.CurrentDevice.CheckSystemVersion(5, 0))
                window.RootViewController = navigation;
            else
                window.AddSubview(navigation.View);
            return true;
        }
    }
}

