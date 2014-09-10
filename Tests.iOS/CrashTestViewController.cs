using System;
using System.Collections.Generic;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.NUnit.UI;
using MonoTouch.UIKit;

namespace Bugsnag.Test
{
    public partial class CrashTestViewController : UIViewController
    {
        private readonly TouchRunner runner;
        private BugsnagClient bugsnagClient;

        public CrashTestViewController (TouchRunner runner) : base ("CrashTestViewController", null)
        {
            this.runner = runner;

            Title = "Bugsnag Tests";
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            // Perform any additional setup after loading the view, typically from a nib.
            UnitTestButton.TouchUpInside += delegate {
                NavigationController.PushViewController (runner.GetViewController (), true);
            };

            ManagedTestButton.TouchUpInside += delegate {
                SetupBugsnag ();
                CauseManagedException ();
            };

            NSExceptionTestButton.TouchUpInside += delegate {
                SetupBugsnag ();
                CauseNSException ();
            };

            SegFaultTestButton.TouchUpInside += delegate {
                SetupBugsnag ();
                CauseSegFault ();
            };

            if (CheckErrors ()) {
                TestResultLabel.Text = "Found errors from previous crash";
                TestResultLabel.BackgroundColor = UIColor.Green;
            } else {
                TestResultLabel.Text = "No logged errors found";
                TestResultLabel.BackgroundColor = UIColor.Red;
            }
        }

        private void SetupBugsnag ()
        {
            if (bugsnagClient != null) {
                bugsnagClient = new BugsnagClient ("testing", false) {
                    DeviceId = Guid.NewGuid ().ToString (),
                    ProjectNamespaces = new List<string> () { "Bugsnag." },
                    ReleaseStage = "development",
                };
            }

            bugsnagClient.Notifier.StoreOnly = true;
        }

        private void CauseManagedException (String invalid = null)
        {
            invalid.Trim ();
        }

        private void CauseNSException ()
        {
            PerformSelector (new MonoTouch.ObjCRuntime.Selector ("totallyInvalidSelector"), new NSString (), 0.1);
        }

        private void CauseSegFault ()
        {
            // TODO: How to cause this?
            new UIAlertView ("Not Implemented", "This test needs to be implemented still.", null, "Ok").Show ();
        }

        private bool CheckErrors ()
        {
            var errorsDir = BugsnagClient.MakeErrorCacheDir ();
            var files = Directory.GetFiles (errorsDir);
            if (files.Length == 0)
                return false;

            foreach (var file in files) {
                Console.WriteLine ("{0}:", file);
                Console.WriteLine (File.ReadAllText (file));
                Console.WriteLine ("===========");

                File.Delete (file);
            }

            return true;
        }
    }
}
