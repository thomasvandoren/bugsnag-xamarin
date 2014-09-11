using System;
using Bugsnag.Data;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace Sample
{
    public partial class DemoViewController : UIViewController
    {
        public DemoViewController () : base ("DemoViewController", null)
        {
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            NotifyButton.TouchUpInside += delegate {
                String empty = null;
                try {
                    empty.Contains ("null exception");
                } catch (Exception e) {
                    // You can add extra metadata to the error report to better debug it.
                    // This information will show up in the Bugsnag web app.
                    var md = new Metadata ();
                    md.AddToTab ("Tab1", "key1", "value1");
                    md.AddToTab ("Tab1", "key2", "value2");

                    // Send the error to bugsnag
                    AppDelegate.BugsnagClient.Notify (e, ErrorSeverity.Warning, md);
                }
            };

            CrashButton.TouchUpInside += delegate {
                // When the application crashes, Bugsnag library intercepts the error, writes it to disk and
                // submits it next time the application starts.
                PerformSelector (new Selector ("invalidSelectorWillCrash"), this, 0.01f);
            };
        }
    }
}
