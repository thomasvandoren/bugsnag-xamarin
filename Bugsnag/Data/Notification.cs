using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bugsnag.Data
{
    [JsonObject (MemberSerialization.OptIn)]
    public class Notification
    {
        [JsonProperty ("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty ("notifier")]
        public NotifierInfo Notifier { get; set; }

        [JsonProperty ("events")]
        public List<Event> Events { get; set; }
    }
}
