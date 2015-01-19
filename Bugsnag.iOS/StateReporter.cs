using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Bugsnag.Data;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Bugsnag
{
    internal class StateReporter
    {
        private readonly BugsnagClient client;
        private readonly DateTime appStartTime = DateTime.UtcNow;

        public StateReporter (BugsnagClient client)
        {
            if (client == null)
                throw new ArgumentNullException ("client");

            this.client = client;
        }

        public TouchApplicationInfo GetApplicationInfo ()
        {
            var bundle = NSBundle.MainBundle;
            var version = (string)(NSString)bundle.ObjectForInfoDictionary ("CFBundleShortVersionString");
            var bundleVersion = (string)(NSString)bundle.ObjectForInfoDictionary ("CFBundleVersion");
            var name = (string)(NSString)bundle.ObjectForInfoDictionary ("CFBundleDisplayName");

            return new TouchApplicationInfo () {
                Id = bundle.BundleIdentifier,
                Version = version,
                BundleVersion = bundleVersion,
                Name = name,
                ReleaseStage = client.ReleaseStage,
            };
        }

        public TouchApplicationState GetApplicationState ()
        {
            return new TouchApplicationState () {
                SessionLength = client.StateTracker.SessionLength,
                TimeSinceMemoryWarning = client.StateTracker.TimeSinceLastMemoryWarning,
                InForeground = client.StateTracker.InForeground,
                CurrentScreen = TopMostViewController,
                RunningTime = DateTime.UtcNow - appStartTime,
                // TODO: Implement memory usage recording
            };
        }

        public TouchSystemInfo GetSystemInfo ()
        {
            return new TouchSystemInfo () {
                Id = client.DeviceId,
                Manufacturer = "Apple",
                Model = Model,
                ScreenDensity = (float)UIScreen.MainScreen.Scale,
                ScreenResolution = ScreenResolution,
                TotalMemory = TotalMemory,
                OperatingSystem = "iOS",
                OperatingSystemVersion = NSProcessInfo.ProcessInfo.OperatingSystemVersionString,
                IsJailbroken = UIApplication.SharedApplication.CanOpenUrl (new NSUrl ("cydia://")),
                Locale = NSLocale.CurrentLocale.LocaleIdentifier,
                DiskSize = FileSystemAttributes.Size, 
            };
        }

        public TouchSystemState GetSystemState ()
        {
            return new TouchSystemState () {
                FreeMemory = FreeMemory,
                Orientation = client.StateTracker.Orientation,
                BatteryLevel = client.StateTracker.BatteryLevel,
                IsCharging = client.StateTracker.IsCharging,
                AvailableDiskSpace = FileSystemAttributes.FreeSize,
                // TODO: Implement LocationStatus reporting
                // TODO: Implement NetworkStatus reporting
            };
        }

        private static NSFileSystemAttributes FileSystemAttributes {
            get {
                var paths = NSSearchPath.GetDirectories (NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, true);
                return NSFileManager.DefaultManager.GetFileSystemAttributes (paths.Last ());
            }
        }

        private static string Model {
            get {
                long size = 0;
                if (sysctlbyname ("hw.machine", null, ref size, IntPtr.Zero, 0) == 0) {
                    var buf = new byte[size];
                    if (sysctlbyname ("hw.machine", buf, ref size, IntPtr.Zero, 0) == 0) {
                        return Encoding.UTF8.GetString (buf, 0, (int)size);
                    }
                }
                return null;
            }
        }

        private static ulong TotalMemory {
            get {
                var buf = new byte[sizeof(ulong)];
                var size = buf.LongLength;
                if (sysctlbyname ("hw.memsize", buf, ref size, IntPtr.Zero, 0) == 0) {
                    return BitConverter.ToUInt64 (buf, 0);
                }
                return 0;
            }
        }

        private static ulong FreeMemory {
            get {
                ulong pageSize;
                ulong pagesFree;

                var buf = new byte[sizeof(ulong)];
                var size = buf.LongLength;
                if (sysctlbyname ("vm.page_free_count", buf, ref size, IntPtr.Zero, 0) == 0) {
                    pagesFree = BitConverter.ToUInt64 (buf, 0);
                    if (sysctlbyname ("hw.pagesize", buf, ref size, IntPtr.Zero, 0) == 0) {
                        pageSize = BitConverter.ToUInt64 (buf, 0);
                        return pagesFree * pageSize;
                    }
                }
                return 0;
            }
        }

        private static string ScreenResolution {
            get {
                var size = UIScreen.MainScreen.Bounds.Size;
                var scale = UIScreen.MainScreen.Scale;
                return String.Format ("{0}x{1}", (int)(size.Width * scale), (int)(size.Height * scale)); 
            }
        }

        [DllImport (Constants.SystemLibrary)]
        private static extern int sysctlbyname ([MarshalAs (UnmanagedType.LPStr)] string property, byte[] output, ref long oldLen, IntPtr newp, uint newlen);

        private static string TopMostViewController {
            get {
                return InvokeOnMainThread (delegate {
                    UIViewController viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;

                    if (viewController is UINavigationController) {
                        viewController = ((UINavigationController)viewController).VisibleViewController;
                    }

                    var depth = 0;

                    while (viewController != null && depth <= 30) {
                        var presentedController = viewController.PresentedViewController;

                        if (presentedController == null) {
                            return viewController.GetType ().ToString ();
                        } else if (presentedController is UINavigationController) {
                            viewController = ((UINavigationController)presentedController).VisibleViewController;
                        } else {
                            viewController = presentedController;
                        }

                        depth++;
                    }

                    return viewController != null ? viewController.GetType ().ToString () : null;
                });
            }
        }

        private static T InvokeOnMainThread<T> (Func<T> action)
        {
            T ret = default(T);

            UIApplication.SharedApplication.InvokeOnMainThread (delegate {
                ret = action ();
            });

            return ret;
        }
    }
}
