// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace Jint.Ex.iOS
{
	[Register ("Jint_Ex_iOSViewController")]
	partial class Jint_Ex_iOSViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton butAsynchronousExecution { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton butClearEventQueue { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton butMultipleTimers { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton butSynchronousExecution { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton butTimer { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextView txtOut { get; set; }

		[Action ("butAsynchronousExecution_Click:")]
		partial void butAsynchronousExecution_Click (MonoTouch.Foundation.NSObject sender);

		[Action ("butClearEventQueue_Click:")]
		partial void butClearEventQueue_Click (MonoTouch.Foundation.NSObject sender);

		[Action ("butMultipleTimers_Click:")]
		partial void butMultipleTimers_Click (MonoTouch.Foundation.NSObject sender);

		[Action ("butSynchronousExecution_Click:")]
		partial void butSynchronousExecution_Click (MonoTouch.Foundation.NSObject sender);

		[Action ("butTimer_Click:")]
		partial void butTimer_Click (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (butClearEventQueue != null) {
				butClearEventQueue.Dispose ();
				butClearEventQueue = null;
			}

			if (butAsynchronousExecution != null) {
				butAsynchronousExecution.Dispose ();
				butAsynchronousExecution = null;
			}

			if (butMultipleTimers != null) {
				butMultipleTimers.Dispose ();
				butMultipleTimers = null;
			}

			if (butSynchronousExecution != null) {
				butSynchronousExecution.Dispose ();
				butSynchronousExecution = null;
			}

			if (butTimer != null) {
				butTimer.Dispose ();
				butTimer = null;
			}

			if (txtOut != null) {
				txtOut.Dispose ();
				txtOut = null;
			}
		}
	}
}
