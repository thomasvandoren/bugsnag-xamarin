using System;
using Bugsnag.Data;

namespace Bugsnag
{
    internal class StateCacher
    {
        private static readonly TimeSpan TimeToLive = TimeSpan.FromSeconds (1);

        private readonly StateReporter reporter;
        private DateTime? cacheTime;
        private ApplicationInfo appInfo;
        private ApplicationState appState;
        private SystemInfo sysInfo;
        private SystemState sysState;

        public StateCacher (StateReporter reporter)
        {
            if (reporter == null)
                throw new ArgumentNullException ("reporter");

            this.reporter = reporter;
        }

        public void Store ()
        {
            // Old cache still valid
            if (cacheTime.HasValue && cacheTime.Value + TimeToLive > DateTime.UtcNow)
                return;

            cacheTime = null;
            GetApplicationInfo ();
            appState = GetApplicationState ();
            GetSystemInfo ();
            sysState = GetSystemState ();
            cacheTime = DateTime.UtcNow;
        }

        public ApplicationInfo GetApplicationInfo ()
        {
            if (appInfo == null) {
                appInfo = reporter.GetApplicationInfo ();
            }
            return appInfo;
        }

        public ApplicationState GetApplicationState ()
        {
            if (cacheTime.HasValue && appState != null) {
                if (cacheTime.Value + TimeToLive > DateTime.UtcNow) {
                    return appState;
                } else {
                    cacheTime = null;
                    appState = null;
                }
            }

            return reporter.GetApplicationState ();
        }

        public SystemInfo GetSystemInfo ()
        {
            if (sysInfo == null) {
                sysInfo = reporter.GetSystemInfo ();
            }
            return sysInfo;
        }

        public SystemState GetSystemState ()
        {
            if (cacheTime.HasValue && sysState != null) {
                if (cacheTime.Value + TimeToLive > DateTime.UtcNow) {
                    return sysState;
                } else {
                    cacheTime = null;
                    sysState = null;
                }
            }

            return reporter.GetSystemState ();
        }
    }
}
