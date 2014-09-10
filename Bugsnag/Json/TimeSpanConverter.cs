using System;
using Newtonsoft.Json;

namespace Bugsnag.Json
{
    public class TimeSpanConverter : JsonConverter
    {
        public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
        {
            TimeSpan? span = null;
            if (value != null) {
                var type = value.GetType ();
                if (type == typeof(TimeSpan?)) {
                    span = (TimeSpan?)value;
                } else if (type == typeof(TimeSpan)) {
                    span = (TimeSpan)value;
                } else {
                    throw new NotSupportedException (String.Format ("Unable to convert {0}.", value));
                }
            }

            if (span == null) {
                writer.WriteNull ();
            } else {
                writer.WriteValue ((long)span.Value.TotalMilliseconds);
            }
        }

        public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var val = reader.Value;
            if (objectType == typeof(TimeSpan?)) {
                if (val == null)
                    return null;
                return (TimeSpan?)TimeSpan.FromMilliseconds ((long)val);
            } else if (objectType == typeof(TimeSpan)) {
                return TimeSpan.FromMilliseconds ((long)val);
            } else {
                throw new NotSupportedException (String.Format ("Unable to convert {0}.", objectType));
            }
        }

        public override bool CanConvert (Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }
    }
}
