using System;
using System.Threading.Tasks;
using Bugsnag.Data;

namespace Bugsnag.Interceptor
{
    internal sealed class TaskSchedulerInterceptor : IDisposable
    {
        private readonly IBugsnagClient client;

        public TaskSchedulerInterceptor (IBugsnagClient client)
        {
            if (client == null)
                throw new ArgumentNullException ("client");
            this.client = client;

            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
        }

        public void Dispose ()
        {
            TaskScheduler.UnobservedTaskException -= HandleUnobservedTaskException;
        }

        private void HandleUnobservedTaskException (object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (!client.AutoNotify)
                return;
            client.Notify (e.Exception, ErrorSeverity.Warning);
        }
    }
}
