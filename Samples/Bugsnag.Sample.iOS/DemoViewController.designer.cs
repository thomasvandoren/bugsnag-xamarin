// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Sample
{
	[Register ("DemoViewController")]
	partial class DemoViewController
	{
		[Outlet]
		UIKit.UIButton CrashButton { get; set; }

		[Outlet]
		UIKit.UIButton NotifyButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (NotifyButton != null) {
				NotifyButton.Dispose ();
				NotifyButton = null;
			}

			if (CrashButton != null) {
				CrashButton.Dispose ();
				CrashButton = null;
			}
		}
	}
}
