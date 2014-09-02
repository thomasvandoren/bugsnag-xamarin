using System;
using Android.Runtime;
using Bugsnag.Data;

namespace Bugsnag.Interceptor
{
    internal sealed class AndroidInterceptor : IDisposable
    {
        private readonly BugsnagClient client;

        public AndroidInterceptor (BugsnagClient client)
        {
            if (client == null)
                throw new ArgumentNullException ("client");
            this.client = client;

            AndroidEnvironment.UnhandledExceptionRaiser += HandleUnhandledManagedException;
            JavaExceptionHandler.Install (client);
        }

        public void Dispose ()
        {
            JavaExceptionHandler.CleanUp ();
            AndroidEnvironment.UnhandledExceptionRaiser -= HandleUnhandledManagedException;
        }

        private void HandleUnhandledManagedException (object sender, RaiseThrowableEventArgs e)
        {
            if (!client.AutoNotify)
                return;

            // Cache the app and system states, since filling them when the exception bubles to the global
            // app domain unhandled exception is impossible.
            client.State.Store ();
        }

        private class JavaExceptionHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
        {
            private readonly IBugsnagClient client;
            private readonly Java.Lang.Thread.IUncaughtExceptionHandler nextHandler;

            private JavaExceptionHandler (IBugsnagClient client, Java.Lang.Thread.IUncaughtExceptionHandler original)
            {
                this.client = client;
                this.nextHandler = original;
            }

            public void UncaughtException (Java.Lang.Thread thread, Java.Lang.Throwable ex)
            {
                if (client.AutoNotify) {
                    client.Notify (ex, ErrorSeverity.Fatal);
                }
                if (nextHandler != null) {
                    nextHandler.UncaughtException (thread, ex);
                }
            }

            public static void Install (BugsnagClient client)
            {
                var current = Java.Lang.Thread.DefaultUncaughtExceptionHandler;
                var self = current as JavaExceptionHandler;
                if (self != null) {
                    current = self.nextHandler;
                }

                Java.Lang.Thread.DefaultUncaughtExceptionHandler = new JavaExceptionHandler (client, current);
            }

            public static void CleanUp ()
            {
                var current = Java.Lang.Thread.DefaultUncaughtExceptionHandler as JavaExceptionHandler;
                if (current != null) {
                    Java.Lang.Thread.DefaultUncaughtExceptionHandler = current.nextHandler;
                }
            }
        }
    }
}

