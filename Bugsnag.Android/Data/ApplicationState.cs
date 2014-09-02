using System;
using Newtonsoft.Json;
using Bugsnag.Json;
using System.Collections.Generic;

namespace Bugsnag.Data
{
    public class ApplicationState : Phoebe.Bugsnag.Data.ApplicationState
    {
        [JsonProperty ("durationInForeground"), JsonConverter (typeof(TimeSpanConverter))]
        public TimeSpan SessionLength { get; set; }

        [JsonProperty ("lowMemory")]
        public bool HasLowMemory { get; set; }

        [JsonProperty ("inForeground")]
        public bool InForeground { get; set; }

        [JsonProperty ("screenStack")]
        public List<string> ActivityStack { get; set; }

        [JsonProperty ("activeScreen")]
        public string CurrentActivity { get; set; }
    }
}
