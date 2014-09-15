Toggl's Bugsnag error notifier is a simple library to send errors in your application to [Bugsnag](https://bugsnag.com/?r=Rse0tz) error monitoring service. You can send errors manually and the library also automatically logs application crashes due to errors you didn't expect.

This library was developed by [Toggl](https://toggl.com/) to track errors in the Toggl Timer Android and iOS applications. The library exposes more or less identical functionality to the official Bugsnag libraries for Java/objC.

Using it is a matter of creating a new instance of BugsnagClient, like so:

```
using Bugsnag;

// ...

var bugsnagClient = new BugsnagClient ("BUGSNAG-APIKEY");
```
