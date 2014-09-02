using System;
using System.Collections.Generic;
using Bugsnag.Data;

namespace Bugsnag
{
    interface IBugsnagClient : IDisposable
    {
        bool AutoNotify { get; set; }

        string Context { get; set; }

        string ReleaseStage { get; set; }

        List<string> NotifyReleaseStages { get; set; }

        List<string> Filters { get; set; }

        List<Type> IgnoredExceptions { get; set; }

        List<string> ProjectNamespaces { get; set; }

        string UserId { get; set; }

        string UserEmail { get; set; }

        string UserName { get; set; }

        void SetUser (string id, string email = null, string name = null);

        void AddToTab (string tabName, string key, object value);

        void ClearTab (string tabName);

        void TrackUser ();

        void Notify (Exception e, ErrorSeverity severity = ErrorSeverity.Error, Metadata extraMetadata = null);
    }
}
