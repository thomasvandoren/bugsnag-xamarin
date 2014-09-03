using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Util;
using Bugsnag.Data;

namespace Bugsnag
{
    internal class StateReporter
    {
        private readonly BugsnagClient client;
        private readonly Context ctx;

        public StateReporter (BugsnagClient client, Context ctx)
        {
            if (client == null)
                throw new ArgumentNullException ("client");
            if (ctx == null)
                throw new ArgumentNullException ("ctx");

            this.client = client;
            this.ctx = ctx;
        }

        public AndroidApplicationInfo GetApplicationInfo ()
        {
            return new AndroidApplicationInfo () {
                Id = ctx.PackageName,
                Package = ctx.PackageName,
                Version = GetAppVersion (),
                Name = GetAppName (),
                ReleaseStage = client.ReleaseStage,
            };
        }

        public AndroidApplicationState GetApplicationState ()
        {
            return new AndroidApplicationState () {
                SessionLength = client.ActivityTracker.SessionLength,
                HasLowMemory = CheckMemoryLow (),
                InForeground = client.ActivityTracker.InForeground,
                ActivityStack = client.ActivityTracker.Activities,
                CurrentActivity = client.ActivityTracker.TopActivity,
                RunningTime = client.ActivityTracker.RunningTime,
                MemoryUsage = GetMemoryUsedByApp (),
            };
        }

        public AndroidSystemInfo GetSystemInfo ()
        {
            return new AndroidSystemInfo () {
                Id = client.DeviceId,
                Manufacturer = Build.Manufacturer,
                Model = Build.Model,
                ScreenDensity = ctx.Resources.DisplayMetrics.Density,
                ScreenResolution = GetScreenResolution (),
                TotalMemory = (ulong)GetMemoryAvailable (),
                OperatingSystem = "android",
                OperatingSystemVersion = Build.VERSION.Release,
                ApiLevel = (int)Build.VERSION.SdkInt,
                IsRooted = CheckRoot (),
                Locale = Java.Util.Locale.Default.ToString (),
            };
        }

        public AndroidSystemState GetSystemState ()
        {
            return new AndroidSystemState () {
                FreeMemory = (ulong)GetFreeMemory (),
                Orientation = GetOrientation (),
                BatteryLevel = GetBatteryLevel (),
                IsCharging = CheckBatteryCharging (),
                AvailableDiskSpace = (ulong)GetAvailableDiskSpace (),
                LocationStatus = GetGpsStatus (),
                NetworkStatus = GetNetworkStatus (),
            };
        }

        private long GetMemoryUsedByApp ()
        {
            try {
                var rt = Java.Lang.Runtime.GetRuntime ();
                return rt.TotalMemory () - rt.FreeMemory ();
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to calculate memory used by the application.");
                return 0;
            }
        }

        private string GetScreenResolution ()
        {
            try {
                var dm = ctx.Resources.DisplayMetrics;
                return String.Format ("{0}x{1}",
                    Math.Max (dm.WidthPixels, dm.HeightPixels),
                    Math.Min (dm.WidthPixels, dm.HeightPixels));
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to get screen resolution.");
                return null;
            }
        }

        private long GetMemoryAvailable ()
        {
            try {
                var rt = Java.Lang.Runtime.GetRuntime ();
                if (rt.MaxMemory () != long.MaxValue) {
                    return rt.MaxMemory ();
                } else {
                    return rt.TotalMemory ();
                }
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to retrieve available memory amount.");
                return 0;
            }
        }

        private long GetFreeMemory ()
        {
            return GetMemoryAvailable () - GetMemoryUsedByApp ();
        }

        private bool CheckRoot ()
        {
            return CheckTestKeysBuild () || CheckSuperUserAPK ();
        }

        private static bool CheckTestKeysBuild ()
        {
            var tags = Build.Tags;
            return tags != null && tags.Contains ("test-keys");
        }

        private static bool CheckSuperUserAPK ()
        {
            try {
                return System.IO.File.Exists ("/system/app/Superuser.apk");
            } catch {
                return false;
            }
        }

        private Orientation GetOrientation ()
        {
            try {
                return ctx.Resources.Configuration.Orientation;
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to determine device orientation.");
                return Orientation.Undefined;
            }
        }

        private bool CheckBatteryCharging ()
        {
            try {
                var filter = new IntentFilter (Intent.ActionBatteryChanged);
                var intent = ctx.RegisterReceiver (null, filter);

                var status = (BatteryStatus)intent.GetIntExtra ("status", -1);
                return status == BatteryStatus.Charging || status == BatteryStatus.Full;
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to determine if the battery is charging.");
                return false;
            }
        }

        private float GetBatteryLevel ()
        {
            try {
                var filter = new IntentFilter (Intent.ActionBatteryChanged);
                var intent = ctx.RegisterReceiver (null, filter);

                int level = intent.GetIntExtra ("level", -1);
                int scale = intent.GetIntExtra ("scale", -1);

                return level / (float)scale;
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to determine battery level.");
                return 0;
            }
        }

        private long GetAvailableDiskSpace ()
        {
            try {
                var externalStat = new StatFs (Android.OS.Environment.ExternalStorageDirectory.Path);
                var externalAvail = (long)externalStat.BlockSize * (long)externalStat.BlockCount;

                var internalStat = new StatFs (Android.OS.Environment.DataDirectory.Path);
                var internalAvail = (long)internalStat.BlockSize * (long)internalStat.BlockCount;

                return Math.Min (externalAvail, internalAvail);
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to determine available disk space.");
                return 0;
            }
        }

        private string GetGpsStatus ()
        {
            try {
                var cr = ctx.ContentResolver;
                var providers = Settings.Secure.GetString (
                                    cr, Settings.Secure.LocationProvidersAllowed);
                if (!String.IsNullOrEmpty (providers)) {
                    return "allowed";
                } else {
                    return "disallowed";
                }
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to determine GPS status.");
                return null;
            }
        }

        private string GetNetworkStatus ()
        {
            try {
                var cm = (ConnectivityManager)ctx.GetSystemService (
                             Context.ConnectivityService);
                var activeNetwork = cm.ActiveNetworkInfo;
                if (activeNetwork != null && activeNetwork.IsConnectedOrConnecting) {
                    switch (activeNetwork.Type) {
                    case ConnectivityType.Wifi:
                        return "wifi";
                    case ConnectivityType.Ethernet:
                        return "ethernet";
                    default:
                        return "cellular";
                    }
                } else {
                    return "none";
                }
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to determine network status.");
                return null;
            }
        }

        private bool CheckMemoryLow ()
        {
            try {
                var am = (ActivityManager)ctx.GetSystemService (
                             Context.ActivityService);
                var memInfo = new ActivityManager.MemoryInfo ();
                am.GetMemoryInfo (memInfo);

                return memInfo.LowMemory;
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to determine low memory state.");
                return false;
            }
        }

        private string GetAppVersion ()
        {
            try {
                var pkg = ctx.PackageManager.GetPackageInfo (ctx.PackageName, 0);
                return pkg.VersionName;
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to get the application version.");
                return null;
            }
        }

        private string GetAppName ()
        {
            try {
                var app = ctx.PackageManager.GetApplicationInfo (ctx.PackageName, 0);
                return app.Name;
            } catch (Java.Lang.Throwable ex) {
                Log.Warn (BugsnagClient.Tag, ex, "Failed to get the application name.");
                return null;
            }
        }
    }
}
