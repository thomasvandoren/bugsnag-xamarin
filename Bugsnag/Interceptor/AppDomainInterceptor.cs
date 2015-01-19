using System;
using Bugsnag.Data;

namespace Bugsnag.Interceptor
{
    internal sealed class AppDomainInterceptor : IDisposable
    {
        private readonly BugsnagClient client;

        public AppDomainInterceptor (BugsnagClient client)
        {
            if (client == null)
                throw new ArgumentNullException ("client");
            this.client = client;

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        }

        public void Dispose ()
        {
            AppDomain.CurrentDomain.UnhandledException -= HandleUnhandledException;
        }

        private void HandleUnhandledException (object sender, UnhandledExceptionEventArgs e)
        {
            if (!client.AutoNotify)
                return;

            if (e.IsTerminating) {
                // At this point in time we don't want to attempt an HTTP connection, thus we only store
                // the event to disk and hope that the user opens the application again to send the
                // errors to Bugsnag.
                client.Notifier.StoreOnly = true;
            }

            var ex = e.ExceptionObject as Exception;
            if (ex == null) {
                ex = new Exception (String.Format ("Non-exception: {0}", e.ExceptionObject));
            }
            client.Notify (ex, e.IsTerminating ? ErrorSeverity.Fatal : ErrorSeverity.Error);
        }
    }
}
