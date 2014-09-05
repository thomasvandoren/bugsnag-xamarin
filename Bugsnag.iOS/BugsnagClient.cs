using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bugsnag.Data;
using Bugsnag.Util;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Bugsnag.Interceptor;

namespace Bugsnag
{
    public class BugsnagClient : IBugsnagClient
    {
        private readonly string apiKey;
        private readonly bool sendMetrics;
        private readonly StateCacher state;
        private readonly StateTracker stateTracker;
        private readonly ExceptionConverter exceptionConverter;
        private readonly Notifier notifier;
        private readonly UserInfo userInfo = new UserInfo ();
        private readonly Metadata metadata = new Metadata ();
        private IDisposable[] interceptors;
        private NSObject notifApplicationDidBecomeActive;

        public BugsnagClient (string apiKey, bool enableMetrics = true)
        {
            if (apiKey == null)
                throw new ArgumentNullException ("apiKey");

            this.apiKey = apiKey;
            sendMetrics = enableMetrics;
            AutoNotify = true;

            // Install exception handlers
            interceptors = new IDisposable[] {
                new AppDomainInterceptor (this),
                new TaskSchedulerInterceptor (this),
            };

            state = new StateCacher (new StateReporter (this));
            stateTracker = new StateTracker ();
            exceptionConverter = new ExceptionConverter (this);
            notifier = new Notifier (this, MakeErrorCacheDir ());

            // Register observers init observer
            notifApplicationDidBecomeActive = NSNotificationCenter.DefaultCenter.AddObserver (
                UIApplication.DidBecomeActiveNotification, OnApplicationDidBecomeActive);
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
                // Remove observers
                if (notifApplicationDidBecomeActive != null) {
                    NSNotificationCenter.DefaultCenter.RemoveObserver (notifApplicationDidBecomeActive);
                    notifApplicationDidBecomeActive = null;
                }

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

        internal StateTracker StateTracker {
            get { return stateTracker; }
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

        private void OnApplicationDidBecomeActive (NSNotification notif)
        {
            Linker.CheckStripped ();
            if (sendMetrics) {
                TrackUser ();
            }
            notifier.Flush ();

            // We're not interested in any more became active events
            if (notifApplicationDidBecomeActive != null) {
                NSNotificationCenter.DefaultCenter.RemoveObserver (notifApplicationDidBecomeActive);
                notifApplicationDidBecomeActive = null;
            }
        }

        private static string MakeErrorCacheDir ()
        {
            var path = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "bugsnag-events");
            if (!Directory.Exists (path)) {
                try {
                    Directory.CreateDirectory (path);
                } catch (Exception ex) {
                    Log.WriteLine ("Failed to create cache dir: {0}", ex);
                    path = null;
                }
            }

            return path;
        }
    }
}
