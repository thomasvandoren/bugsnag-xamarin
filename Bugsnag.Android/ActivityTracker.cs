using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;

namespace Bugsnag
{
    internal class ActivityTracker
    {
        private static readonly TimeSpan IdleTimeForSessionEnd = TimeSpan.FromSeconds (10);

        private readonly List<WeakReference> activityStack = new List<WeakReference> ();
        private readonly DateTime appStartTime = DateTime.UtcNow;
        private WeakReference topActivity;
        private DateTime sessionPauseTime;
        private DateTime sessionStartTime;

        public void OnCreate (Context ctx)
        {
            activityStack.Add (new WeakReference (ctx));
        }

        public void OnResume (Context ctx)
        {
            topActivity = new WeakReference (ctx);

            if (DateTime.UtcNow - sessionPauseTime > IdleTimeForSessionEnd) {
                sessionStartTime = DateTime.UtcNow;
            }
        }

        public void OnPause (Context ctx)
        {
            topActivity = null;
            sessionPauseTime = DateTime.UtcNow;
        }

        public void OnDestroy (Context ctx)
        {
            activityStack.RemoveAll ((w) => !w.IsAlive || w.Target == ctx);
        }

        public bool InForeground {
            get { return topActivity != null; }
        }

        public string TopActivity {
            get {
                if (topActivity == null)
                    return null;

                var ctx = topActivity.Target as Context;
                if (ctx == null)
                    return null;

                return ctx.GetType ().Name;
            }
        }

        public List<string> Activities {
            get {
                return activityStack.Select ((w) => w.Target as Context)
                    .Where ((ctx) => ctx != null)
                    .Select ((ctx) => ctx.GetType ().Name)
                    .ToList ();
            }
        }

        public TimeSpan SessionLength {
            get {
                if (InForeground) {
                    return DateTime.UtcNow - sessionStartTime;
                } else {
                    return TimeSpan.Zero;
                }
            }
        }

        public TimeSpan RunningTime {
            get { return DateTime.UtcNow - appStartTime; }
        }
    }
}
