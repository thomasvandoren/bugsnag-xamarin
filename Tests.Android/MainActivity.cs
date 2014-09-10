using System;
using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Util;

namespace Bugsnag.Test
{
    [Activity (Label = "Bugsnag Tests", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const string Tag = "Bugsnag";
        private BugsnagClient bugsnagClient;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.MainActivity);

            FindViewById<Button> (Resource.Id.UnitTestButton).Click += delegate {
                StartActivity (typeof(NUnitActivity));
            };

            FindViewById<Button> (Resource.Id.TestManagedButton).Click += delegate {
                SetupBugsnag ();
                CauseManagedException ();
            };

            FindViewById<Button> (Resource.Id.TestJavaButton).Click += delegate {
                SetupBugsnag ();
                CauseJavaException ();
            };

            var statusTextView = FindViewById<TextView> (Resource.Id.StatusTextView);
            if (CheckErrors ()) {
                statusTextView.Text = "Found errors from previous crash";
                statusTextView.SetBackgroundColor (Color.DarkGreen);
            } else {
                statusTextView.Text = "No logged errors found";
                statusTextView.SetBackgroundColor (Color.DarkRed);
            }
        }

        private void SetupBugsnag ()
        {
            if (bugsnagClient == null) {
                bugsnagClient = new BugsnagClient (this, "testing", false) {
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

        private void CauseJavaException ()
        {
            StartActivity (new Intent (this, Java.Lang.Class.ForName ("bugsnag.test.CrashingJavaActivity")));
        }

        private bool CheckErrors ()
        {
            var errorsDir = BugsnagClient.MakeErrorCacheDir (this);
            var files = Directory.GetFiles (errorsDir);
            if (files.Length == 0)
                return false;

            foreach (var file in files) {
                Log.Error (Tag, file);
                Log.Error (Tag, File.ReadAllText (file));

                File.Delete (file);
            }

            return true;
        }
    }
}
