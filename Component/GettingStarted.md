# Bugsnag Error Monitoring Service

Bugsnag is a web dashboard to see your application errors in real-time. It helps you understand and fix them quickly. To use this library, you need a project API key:

1. Go to [bugsnag.com](https://bugsnag.com/?r=Rse0tz) & sign up
2. Add a new project
3. Under project settings you can find your API key

# Using the Library

Using the library is a matter of creating a new BugsnagClient and keeping it's reference around for the duration of the application lifetime.

## Android Quickstart

Easiest way to integrate Bugsnag error tracking is by creating a BugsnagClient in your `Application.OnCreate` method.

```
public override void OnCreate ()
{
    base.OnCreate ();

    if (bugsnagClient == null) {
        // create new BugsnagClient which will monitor for errors and send them to the server
        bugsnagClient = new BugsnagClient (this, "MY-BUGSNAG-APIKEY-HERE") {
            DeviceId = GetInstalId (),
            ProjectNamespaces = new List<string> () { "Sample." },
        };
    }
}
```

Check out the bundled *Android Sample* for more a complete example. The sample also shows how to track the top-most activity to automatically set the Context (see Configuration section below).

## iOS Quickstart

Easiest way to integrate Bugsnag error tracking is by creating a BugsnagClient in your `UIApplicationDelegate.FinishedLaunching` method.

```
public override bool FinishedLaunching (UIApplication app, NSDictionary options)
{
    // create new BugsnagClient which will monitor for errors and send them to the server
    bugsnagClient = new BugsnagClient ("MY-BUGSNAG-APIKEY-HERE") {
        DeviceId = GetInstalId (),
        ProjectNamespaces = new List<string> () { "Sample." },
        ReleaseStage = "development",
    };

    // ... other initialisation code
}
```

Check out the bundled *iOS Sample* for a more complete example.

## Configuration

There are several instance properties which you can change to configure the behaviour of the library.

- `AutoNotify` - Controls whether the library should automatically notify about any unhandled exceptions it detects.
- `Context` - Bugsnag uses the concept of "contexts" to help display and group your errors. Contexts represent what was happening in your application at the time an error occurs. In a mobile app, it is useful to set this to be the name of the screen the user is currently on.
- `ReleaseStage` - If you would like to distinguish between errors that happen in different stages of the application release process (development, production, etc) you can use this property. On Android the default stage is automatically detected by using the APK debuggable attribute.
- `NotifyReleaseStages` - By default, all exceptions are sent to Bugsnag. If you would like to specifiy which release stages interest you, you can use this property.
- `Filters` - Sets the strings to filter out from the extra metadata objects before sending them to Bugsnag. Use this if you want to ensure you don't send sensitive data such as passwords, and credit card numbers to Bugsnag servers. Any keys which contain these strings will be filtered.
- `IgnoredExceptions` - List of exception Type's which the library should ignore and not send to Bugsnag.
- `ProjectNamespaces` - Sets which namespaces the library should consider as "inProject". Bugsnag marks stacktrace lines as in-project if they originate from any of these namespaces.

## User Tracking

You can also specify the current user by either setting the `UserId`, `UserEmail`, `UserName` properties. Alternatively you can use the `SetUser(id, email, name)` convenience method.

## Extra Metadata

Bugsnag allows you to display extra information related to errors on their dashboard. For every error they have various tabs with specific information. You can create one or more custom tabs which display key-value based information on them.

For example, to create a new "Family" tab, which shows logged in users relations, we can use the following code snippet:

```
var extras = new Metadata();
extras.AddToTab("Family", "Kevin Doe", "Brother");
extras.AddToTab("Family", "Jane Doe", "Wife");
extras.AddToTab("Family", "Mary Doe", "Child");

App.BugsnagClient.Notify(new Exception("Accident"), extraMetadata: extras);
```

You can also set global metadata for all errors reported from your application:

```
App.BugsnagClient.AddToTab("Family", "Kevin Doe", "Brother");
App.BugsnagClient.AddToTab("Family", "Jane Doe", "Wife");
App.BugsnagClient.AddToTab("Family", "Mary Doe", "Child");
```

# Problems? Contributions?

If you find any bugs with this library, please report them on Github: [github.com/toggl/bugsnag-xamarin](https://github.com/toggl/bugsnag-xamarin). All of the code is available there as well, if you want to get your hands dirty all pull requests are accepted as well!
