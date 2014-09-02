using System;
using Newtonsoft.Json;
using Bugsnag.Json;

namespace Bugsnag.Data
{
    public class ApplicationState : Phoebe.Bugsnag.Data.ApplicationState
    {
        [JsonProperty ("durationInForeground"), JsonConverter (typeof(TimeSpanConverter))]
        public TimeSpan SessionLength { get; set; }

        [JsonProperty ("timeSinceMemoryWarning", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TimeSpan? TimeSinceMemoryWarning { get; set; }

        [JsonProperty ("inForeground")]
        public bool InForeground { get; set; }

        [JsonProperty ("activeScreen")]
        public string CurrentScreen { get; set; }
    }
}
