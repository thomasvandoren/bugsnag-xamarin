using Newtonsoft.Json;

namespace Bugsnag.Data
{
    internal class TouchSystemState : SystemState
    {
        [JsonProperty ("orientation")]
        public string Orientation { get; set; }

        [JsonProperty ("batteryLevel")]
        public float BatteryLevel { get; set; }

        [JsonProperty ("charging")]
        public bool IsCharging { get; set; }

        [JsonProperty ("locationStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LocationStatus { get; set; }

        [JsonProperty ("networkAccess", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string NetworkStatus { get; set; }
    }
}
