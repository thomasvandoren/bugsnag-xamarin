using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Bugsnag;

namespace Sample
{
    [Application]
    public class AndroidApp : Application
    {
        private static BugsnagClient bugsnagClient;

        public AndroidApp (IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer) : base (javaReference, transfer)
        {
        }

        public override void OnCreate ()
        {
            base.OnCreate ();

            if (bugsnagClient == null) {
                // create new BugsnagClient which will monitor for errors and send them to the server
                bugsnagClient = new BugsnagClient (this, "MY-BUGSNAG-APIKEY-HERE") {
                    DeviceId = GetInstalId (),
                    ProjectNamespaces = new List<string> () { "Sample." },
                    // By default Android library can guess the release stage between "production" and "development"
                    // ReleaseStage = "development",
                };
            }

            // You can associate errors with a specific user, if you want
            bugsnagClient.SetUser ("1234", "john@example.com", "John Doe");
        }

        private string GetInstalId ()
        {
            var prefs = GetSharedPreferences ("prefs", FileCreationMode.Private);

            var id = prefs.GetString ("InstallId", null);
            if (String.IsNullOrEmpty (id)) {
                id = Guid.NewGuid ().ToString ();
                prefs.Edit ().PutString ("InstallId", id).Commit ();
            }

            return id;
        }

        public static IBugsnagClient BugsnagClient {
            get { return bugsnagClient; }
        }
    }
}
