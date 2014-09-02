using Newtonsoft.Json;

namespace Bugsnag.Data
{
    internal class TouchApplicationInfo : ApplicationInfo
    {
        [JsonProperty ("id")]
        public string Id { get; set; }

        [JsonProperty ("bundleVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string BundleVersion { get; set; }

        [JsonProperty ("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }
}
