// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace Bugsnag.Test
{
	[Register ("CrashTestViewController")]
	partial class CrashTestViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton ManagedTestButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton NSExceptionTestButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton SegFaultTestButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TestResultLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton UnitTestButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (UnitTestButton != null) {
				UnitTestButton.Dispose ();
				UnitTestButton = null;
			}

			if (ManagedTestButton != null) {
				ManagedTestButton.Dispose ();
				ManagedTestButton = null;
			}

			if (NSExceptionTestButton != null) {
				NSExceptionTestButton.Dispose ();
				NSExceptionTestButton = null;
			}

			if (SegFaultTestButton != null) {
				SegFaultTestButton.Dispose ();
				SegFaultTestButton = null;
			}

			if (TestResultLabel != null) {
				TestResultLabel.Dispose ();
				TestResultLabel = null;
			}
		}
	}
}
