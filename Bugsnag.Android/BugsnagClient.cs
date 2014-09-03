using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Util;
using Bugsnag.Data;
using Bugsnag.Interceptor;

namespace Bugsnag
{
    public class BugsnagClient : IBugsnagClient
    {
        internal const string Tag = "Bugsnag";

        private readonly string apiKey;
        private readonly bool sendMetrics;
        private readonly StateCacher state;
        private readonly ActivityTracker activityTracker;
        private readonly ExceptionConverter exceptionConverter;
        private readonly Notifier notifier;
        private readonly Context androidContext;
        private readonly UserInfo userInfo = new UserInfo ();
        private readonly Metadata metadata = new Metadata ();
        private IDisposable[] interceptors;
        private bool isInitialised;

        public BugsnagClient (Context context, string apiKey, bool enableMetrics = true)
        {
            if (context == null)
                throw new ArgumentNullException ("context");
            if (apiKey == null)
                throw new ArgumentNullException ("apiKey");

            this.apiKey = apiKey;
            sendMetrics = enableMetrics;
            androidContext = context.ApplicationContext;
            ReleaseStage = GuessReleaseStage (context);

            // Install exception handlers
            interceptors = new IDisposable[] {
                new AppDomainInterceptor (this),
                new TaskSchedulerInterceptor (this),
                new AndroidInterceptor (this),
            };

            state = new StateCacher (new StateReporter (this, context));
            activityTracker = new ActivityTracker ();
            exceptionConverter = new ExceptionConverter (this);
            notifier = new Notifier (this, MakeErrorCacheDir (context));
        }

        ~BugsnagClient ()
        {
            Dispose (false);
        }

        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (disposing) {
                // Dispose error interceptors
                foreach (var obj in interceptors.Reverse()) {
                    obj.Dispose ();
                }
            }
        }

        internal string ApiKey {
            get { return apiKey; }
        }

        internal StateCacher State {
            get { return state; }
        }

        internal Notifier Notifier {
            get { return notifier; }
        }

        internal ActivityTracker ActivityTracker {
            get { return activityTracker; }
        }

        internal bool ShouldNotify {
            get {
                if (NotifyReleaseStages == null)
                    return true;
                return NotifyReleaseStages.Contains (ReleaseStage);
            }
        }

        public string DeviceId { get; set; }

        public bool AutoNotify { get; set; }

        public string Context { get; set; }

        public string ReleaseStage { get; set; }

        public List<string> NotifyReleaseStages { get; set; }

        public List<string> Filters { get; set; }

        public List<Type> IgnoredExceptions { get; set; }

        public List<string> ProjectNamespaces { get; set; }

        public string UserId {
            get { return userInfo.Id; }
            set { userInfo.Id = value; }
        }

        public string UserEmail {
            get { return userInfo.Email; }
            set { userInfo.Email = value; }
        }

        public string UserName {
            get { return userInfo.Name; }
            set { userInfo.Name = value; }
        }

        public void SetUser (string id, string email = null, string name = null)
        {
            UserId = id;
            UserEmail = email;
            UserName = name;
        }

        private UserInfo GetUserInfo ()
        {
            if (String.IsNullOrEmpty (userInfo.Id)) {
                userInfo.Id = DeviceId;
            }
            return userInfo;
        }


        public void AddToTab (string tabName, string key, object value)
        {
            metadata.AddToTab (tabName, key, value);
        }

        public void ClearTab (string tabName)
        {
            metadata.ClearTab (tabName);
        }

        public void TrackUser ()
        {
            notifier.TrackUser (new UserMetrics () {
                ApiKey = apiKey,
                User = GetUserInfo (),
                App = state.GetApplicationInfo (),
                System = state.GetSystemInfo (),
            });
        }

        public void Notify (Exception e, ErrorSeverity severity = ErrorSeverity.Error, Metadata extraMetadata = null)
        {
            if (!ShouldNotify)
                return;
            if (IgnoredExceptions != null && IgnoredExceptions.Contains (e.GetType ()))
                return;

            var md = new Metadata (metadata);
            if (extraMetadata != null) {
                md.Merge (extraMetadata);
            }

            notifier.Notify (new Event () {
                User = GetUserInfo (),
                App = state.GetApplicationInfo (),
                AppState = state.GetApplicationState (),
                System = state.GetSystemInfo (),
                SystemState = state.GetSystemState (),
                Context = Context,
                Severity = severity,
                Exceptions = exceptionConverter.Convert (e),
                Metadata = md,
            });
        }

        public void OnActivityCreated (Context ctx)
        {
            activityTracker.OnCreate (ctx);
        }

        public void OnActivityResumed (Context ctx)
        {
            activityTracker.OnResume (ctx);
            Context = activityTracker.TopActivity;

            if (!isInitialised) {
                if (sendMetrics) {
                    TrackUser ();
                }
                notifier.Flush ();
                isInitialised = true;
            }
        }

        public void OnActivityPaused (Context ctx)
        {
            activityTracker.OnPause (ctx);
        }

        public void OnActivityDestroyed (Context ctx)
        {
            activityTracker.OnDestroy (ctx);
        }

        private static string GuessReleaseStage (Context ctx)
        {
            bool debuggable = false;
            try {
                var app = ctx.PackageManager.GetApplicationInfo (ctx.PackageName, 0);
                debuggable = (app.Flags & Android.Content.PM.ApplicationInfoFlags.Debuggable) != 0;
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (Tag, ex, "Failed automatic release stage detection.");
            }
            return debuggable ? "development" : "production";
        }

        private static string MakeErrorCacheDir (Context ctx)
        {
            var path = Path.Combine (ctx.CacheDir.AbsolutePath, "bugsnag-events");
            if (!Directory.Exists (path)) {
                try {
                    Directory.CreateDirectory (path);
                } catch (Exception ex) {
                    Log.Error (BugsnagClient.Tag, String.Format ("Failed to create cache dir: {0}", ex));
                    path = null;
                }
            }

            return path;
        }
    }
}
