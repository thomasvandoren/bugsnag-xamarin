using System;
using System.Collections.Generic;
using Bugsnag.Json;
using Newtonsoft.Json;

namespace Bugsnag.Data
{
    internal class AndroidApplicationState : ApplicationState
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
