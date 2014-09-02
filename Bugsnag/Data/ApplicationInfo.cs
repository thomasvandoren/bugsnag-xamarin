using Newtonsoft.Json;

namespace Bugsnag.Data
{
    [JsonObject (MemberSerialization.OptIn)]
    public class ApplicationInfo
    {
        [JsonProperty ("version")]
        public string Version { get; set; }

        [JsonProperty ("releaseStage")]
        public string ReleaseStage { get; set; }
    }
}
