using Android.App;
using Android.OS;

namespace Sample
{
    public abstract class BaseActivity : Activity
    {
        protected override void OnCreate (Bundle state)
        {
            base.OnCreate (state);
            AndroidApp.BugsnagClient.OnActivityCreated (this);
        }

        protected override void OnResume ()
        {
            base.OnResume ();
            AndroidApp.BugsnagClient.OnActivityResumed (this);
        }

        protected override void OnPause ()
        {
            base.OnPause ();
            AndroidApp.BugsnagClient.OnActivityPaused (this);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy ();
            AndroidApp.BugsnagClient.OnActivityDestroyed (this);
        }
    }
}
