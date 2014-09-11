using System;
using System.Collections.Generic;
using Bugsnag;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Sample
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register ("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;
        BugsnagClient bugsnagClient;

        //
        // This method is invoked when the application has loaded and is ready to run. In this
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            // create new BugsnagClient which will monitor for errors and send them to the server
            bugsnagClient = new BugsnagClient ("MY-BUGSNAG-APIKEY-HERE") {
                DeviceId = GetInstalId (),
                ProjectNamespaces = new List<string> () { "Sample." },
                ReleaseStage = "development",
            };

            // You can associate errors with a specific user, if you want
            bugsnagClient.SetUser ("1234", "john@example.com", "John Doe");

            // create a new window instance based on the screen size
            window = new UIWindow (UIScreen.MainScreen.Bounds);
            
            // If you have defined a root view controller, set it here:
            window.RootViewController = new DemoViewController ();
            
            // make the window visible
            window.MakeKeyAndVisible ();
            
            return true;
        }

        private string GetInstalId ()
        {
            var id = (string)(NSString)NSUserDefaults.StandardUserDefaults ["InstallId"];
            if (String.IsNullOrEmpty (id)) {
                id = Guid.NewGuid ().ToString ();
                NSUserDefaults.StandardUserDefaults ["InstallId"] = (NSString)id;
            }
            return id;
        }

        public static IBugsnagClient BugsnagClient {
            get { return ((AppDelegate)UIApplication.SharedApplication.Delegate).bugsnagClient; }
        }
    }
}
