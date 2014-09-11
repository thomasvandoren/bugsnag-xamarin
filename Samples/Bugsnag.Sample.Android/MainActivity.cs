using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Bugsnag.Data;

namespace Sample
{
    [Activity (Label = "Bugsnag Sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            FindViewById<Button> (Resource.Id.NotifyButton).Click += delegate {
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
                    AndroidApp.BugsnagClient.Notify (e, ErrorSeverity.Warning, md);
                }
            };

            FindViewById<Button> (Resource.Id.CrashButton).Click += delegate {
                // When the application crashes, Bugsnag library intercepts the error, writes it to disk and
                // submits it next time the application starts.
                throw new NotSupportedException ("Crash ahoy!");
            };
        }
    }
}
