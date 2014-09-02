using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bugsnag.Data
{
    [JsonObject (MemberSerialization.OptIn)]
    public class ExceptionInfo
    {
        [JsonProperty ("errorClass")]
        public string Name { get; set; }

        [JsonProperty ("message")]
        public string Message { get; set; }

        [JsonProperty ("stacktrace")]
        public List<StackInfo> Stack { get; set; }
    }
}
