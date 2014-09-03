using System;
using Bugsnag.Data;

namespace Bugsnag
{
    internal class StateCacher
    {
        private static readonly TimeSpan TimeToLive = TimeSpan.FromSeconds (1);

        private readonly StateReporter reporter;
        private DateTime? cacheTime;
        private AndroidApplicationInfo appInfo;
        private AndroidApplicationState appState;
        private AndroidSystemInfo sysInfo;
        private AndroidSystemState sysState;

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

        public AndroidApplicationInfo GetApplicationInfo ()
        {
            if (appInfo == null) {
                appInfo = reporter.GetApplicationInfo ();
            }
            return appInfo;
        }

        public AndroidApplicationState GetApplicationState ()
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

        public AndroidSystemInfo GetSystemInfo ()
        {
            if (sysInfo == null) {
                sysInfo = reporter.GetSystemInfo ();
            }
            return sysInfo;
        }

        public AndroidSystemState GetSystemState ()
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
