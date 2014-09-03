using System;
using MonoTouch.UIKit;
using Newtonsoft.Json;

namespace Bugsnag.Json
{
    internal class OrientationConverter : JsonConverter
    {
        public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch ((UIDeviceOrientation)value) {
            case UIDeviceOrientation.PortraitUpsideDown:
                writer.WriteValue ("portraitupsidedown");
                break;
            case UIDeviceOrientation.Portrait:
                writer.WriteValue ("portrait");
                break;
            case UIDeviceOrientation.LandscapeRight:
                writer.WriteValue ("landscaperight");
                break;
            case UIDeviceOrientation.LandscapeLeft:
                writer.WriteValue ("landscapeleft");
                break;
            case UIDeviceOrientation.FaceUp:
                writer.WriteValue ("faceup");
                break;
            case UIDeviceOrientation.FaceDown:
                writer.WriteValue ("facedown");
                break;
            case UIDeviceOrientation.Unknown:
            default:
                writer.WriteValue ("unknown");
                break;
            }
        }

        public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch ((string)reader.Value) {
            case "portraitupsidedown":
                return UIDeviceOrientation.PortraitUpsideDown;
            case "portrait":
                return UIDeviceOrientation.Portrait;
            case "landscaperight":
                return UIDeviceOrientation.LandscapeRight;
            case "landscapeleft":
                return UIDeviceOrientation.LandscapeLeft;
            case "faceup":
                return UIDeviceOrientation.FaceUp;
            case "facedown":
                return UIDeviceOrientation.FaceDown;
            default:
                return UIDeviceOrientation.Unknown;
            }
        }

        public override bool CanConvert (Type objectType)
        {
            return objectType == typeof(UIDeviceOrientation);
        }
    }
}
