using System;
using Bugsnag.Json;
using Newtonsoft.Json;

namespace Bugsnag.Data
{
    internal class TouchApplicationState : ApplicationState
    {
        [JsonProperty ("durationInForeground"), JsonConverter (typeof(TimeSpanConverter))]
        public TimeSpan SessionLength { get; set; }

        [JsonProperty ("timeSinceMemoryWarning", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter (typeof(TimeSpanConverter))]
        public TimeSpan? TimeSinceMemoryWarning { get; set; }

        [JsonProperty ("inForeground")]
        public bool InForeground { get; set; }

        [JsonProperty ("activeScreen")]
        public string CurrentScreen { get; set; }
    }
}
