using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Bugsnag
{
    internal class StateTracker : IDisposable
    {
        private static readonly TimeSpan IdleTimeForSessionEnd = TimeSpan.FromSeconds (10);

        private NSObject notifApplicationDidBecomeActive;
        private NSObject notifAapplicationDidEnterBackground;
        private NSObject notifDeviceBatteryStateDidChange;
        private NSObject notifDeviceBatteryLevelDidChange;
        private NSObject notifDeviceOrientationDidChange;
        private NSObject notifApplicationDidReceiveMemoryWarning;

        private DateTime sessionPauseTime;
        private DateTime sessionStartTime;
        private DateTime lastMemoryWarning;
        private bool inForeground;
        private float batteryLevel;
        private bool isCharging;
        private UIDeviceOrientation orientation;

        public StateTracker ()
        {
            notifApplicationDidBecomeActive = NSNotificationCenter.DefaultCenter.AddObserver (
                UIApplication.DidBecomeActiveNotification, OnApplicationDidBecomeActive);
            notifAapplicationDidEnterBackground = NSNotificationCenter.DefaultCenter.AddObserver (
                UIApplication.DidEnterBackgroundNotification, OnApplicationDidEnterBackground);
            notifApplicationDidReceiveMemoryWarning = NSNotificationCenter.DefaultCenter.AddObserver (
                UIApplication.DidReceiveMemoryWarningNotification, OnApplicationDidReceiveMemoryWarning);
            notifDeviceBatteryStateDidChange = NSNotificationCenter.DefaultCenter.AddObserver (
                UIDevice.BatteryStateDidChangeNotification, OnBatteryChanged);
            notifDeviceBatteryLevelDidChange = NSNotificationCenter.DefaultCenter.AddObserver (
                UIDevice.BatteryLevelDidChangeNotification, OnBatteryChanged);
            notifDeviceOrientationDidChange = NSNotificationCenter.DefaultCenter.AddObserver (
                UIDevice.OrientationDidChangeNotification, OnOrientationChanged);

            UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications ();
        }

        public void Dispose ()
        {
            if (notifApplicationDidBecomeActive != null) {
                NSNotificationCenter.DefaultCenter.RemoveObserver (notifApplicationDidBecomeActive);
                notifApplicationDidBecomeActive = null;
            }
            if (notifAapplicationDidEnterBackground != null) {
                NSNotificationCenter.DefaultCenter.RemoveObserver (notifAapplicationDidEnterBackground);
                notifAapplicationDidEnterBackground = null;
            }
            if (notifApplicationDidReceiveMemoryWarning != null) {
                NSNotificationCenter.DefaultCenter.RemoveObserver (notifApplicationDidReceiveMemoryWarning);
                notifApplicationDidReceiveMemoryWarning = null;
            }
            if (notifDeviceBatteryStateDidChange != null) {
                NSNotificationCenter.DefaultCenter.RemoveObserver (notifDeviceBatteryStateDidChange);
                notifDeviceBatteryStateDidChange = null;
            }
            if (notifDeviceBatteryLevelDidChange != null) {
                NSNotificationCenter.DefaultCenter.RemoveObserver (notifDeviceBatteryLevelDidChange);
                notifDeviceBatteryLevelDidChange = null;
            }
            if (notifDeviceOrientationDidChange != null) {
                NSNotificationCenter.DefaultCenter.RemoveObserver (notifDeviceOrientationDidChange);
                notifDeviceOrientationDidChange = null;
            }
        }

        private void OnApplicationDidBecomeActive (NSNotification notif)
        {
            inForeground = true;

            if (DateTime.UtcNow - sessionPauseTime > IdleTimeForSessionEnd) {
                sessionStartTime = DateTime.UtcNow;
            }
        }

        private void OnApplicationDidEnterBackground (NSNotification notif)
        {
            inForeground = false;
            sessionPauseTime = DateTime.UtcNow;
        }

        private void OnApplicationDidReceiveMemoryWarning (NSNotification notif)
        {
            lastMemoryWarning = DateTime.UtcNow;
        }

        private void OnBatteryChanged (NSNotification notif)
        {
            batteryLevel = UIDevice.CurrentDevice.BatteryLevel;
            isCharging = UIDevice.CurrentDevice.BatteryState == UIDeviceBatteryState.Charging;
        }

        private void OnOrientationChanged (NSNotification notif)
        {
            orientation = UIDevice.CurrentDevice.Orientation;
        }

        public TimeSpan TimeSinceLastMemoryWarning {
            get { return DateTime.UtcNow - lastMemoryWarning; }
        }

        public bool InForeground {
            get { return inForeground; }
        }

        public TimeSpan SessionLength {
            get { return DateTime.UtcNow - sessionStartTime; }
        }

        public float BatteryLevel {
            get { return batteryLevel; }
        }

        public bool IsCharging {
            get { return isCharging; }
        }

        public UIDeviceOrientation Orientation {
            get { return orientation; }
        }
    }
}
