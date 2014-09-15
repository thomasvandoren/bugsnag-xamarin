# Xamarin/.NET Bugsnag error notifier

This repository contains the source code for [Toggl's](https://toggl.com/) [Xamarin.Android](http://xamarin.com/platform#android)/[Xamarin.iOS](http://xamarin.com/platform#ios) exception reporting libraries for [Bugsnag](https://bugsnag.com/) error monitoring service.

NB! This is a community developed Bugsnag notifier library.

## Usage

The package can be easily added to your project from Nuget.

### Android usage

You should initialize a copy of `BugsnagClient` in your `Application` object:

```c#
[Application]
class AndroidApp : Application
{
    private BugsnagClient bugsnagClient;

    public AndroidApp () : base ()
    {
    }

    public AndroidApp (IntPtr javaRef, Android.Runtime.JniHandleOwnership transfer) : base (javaRef, transfer)
    {
    }

    public override void OnCreate ()
    {
        base.OnCreate ();

        bugsnagClient = new BugsnagClient (this, "API key here") {
            DeviceId = InstallId,
            ProjectNamespaces = new List<string> () { "MyCompany." },
        };
    }

    public string InstallId {
        get { throw NotImplementedException(); }
    }

    public BugsnagClient BugsnagClient {
        get { return bugsnagClient; }
    }
}
```

In your activities you should also implement some hooks to let the Bugsnag library know what is the current context. Best to do it in your common base activity.

```c#
public class MyBaseActivity : Activity
{
    private BugsnagClient BugsnagClient {
        get { return ((AndroidApp)Application).BugsnagClient; }
    }

    protected override void OnCreate (Bundle state)
    {
        base.OnCreate (state);
        BugsnagClient.OnActivityCreated (this);
    }

    protected override void OnResume ()
    {
        base.OnResume ();
        BugsnagClient.OnActivityResumed (this);
    }

    protected override void OnPause ()
    {
        base.OnPause ();
        BugsnagClient.OnActivityPaused (this);
    }

    protected override void OnDestroy ()
    {
        base.OnDestroy ();
        BugsnagClient.OnActivityDestroyed (this);
    }
}
```

### iOS usage

On iOS you just need to create the `BugsnagClient` in `AppDelegate.FinishedLaunching` method:

```c#
public partial class AppDelegate : UIApplicationDelegate
{
    private BugsnagClient bugsnagClient;

    public override bool FinishedLaunching (UIApplication app, NSDictionary options)
    {
        bugsnagClient = new BugsnagClient ("API key here") {
            DeviceId = InstallId,
            ProjectNamespaces = new List<string> () { "MyCompany." },
            NotifyReleaseStages = new List<string> () { "production" },
            ReleaseStage = "development",
        };
        // Rest of the code for initialising the application
    }

    public string InstallId {
        get { throw NotImplementedException(); }
    }
}
```

## Contributing

Want to contribute? Great! Just [fork](https://github.com/toggl/bugsnag-xamarin/fork) the project, make your
changes and submit a [pull request](https://github.com/toggl/bugsnag-xamarin/pulls).

## License

The code in this repository is licensed under the [BSD 3-clause license](https://github.com/toggl/bugsnag-xamarin/blob/master/LICENSE).
