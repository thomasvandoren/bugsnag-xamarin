// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace Bugsnag.Test
{
	[Register ("CrashTestViewController")]
	partial class CrashTestViewController
	{
		[Outlet]
		UIKit.UIButton ManagedTestButton { get; set; }

		[Outlet]
		UIKit.UIButton NSExceptionTestButton { get; set; }

		[Outlet]
		UIKit.UIButton SegFaultTestButton { get; set; }

		[Outlet]
		UIKit.UILabel TestResultLabel { get; set; }

		[Outlet]
		UIKit.UIButton UnitTestButton { get; set; }
		
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
