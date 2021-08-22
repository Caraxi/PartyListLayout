using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace PartyListLayout.Converter {
    public class VectorJsonConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (value is Vector2 v2) {
                writer.WriteValue($"{v2.X}|{v2.Y}".Replace(',', '.'));
            } else if (value is Vector3 v3) {
                writer.WriteValue($"{v3.X}|{v3.Y}|{v3.Z}".Replace(',', '.'));
            } else if (value is Vector4 v4) {
                writer.WriteValue($"{v4.X}|{v4.Y}|{v4.Z}|{v4.W}".Replace(',', '.'));
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            float[] values;
            var str = (string)reader.Value;
            try {
                values = str.Split('|').Select(s => s.Replace(',', '.')).Select((s) => float.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            } catch {
                values = null;
            }
                
            if (objectType == typeof(Vector2)) return values is { Length: >= 2 } ? new Vector2(values[0], values[1]) : new Vector2();
            if (objectType == typeof(Vector3)) return values is { Length: >= 3 } ? new Vector3(values[0], values[1], values[2]) : new Vector3();
            if (objectType == typeof(Vector4)) return values is { Length: >= 4 } ? new Vector4(values[0], values[1], values[2], values[3]) : new Vector4();
            return null;
        }

        public override bool CanConvert(Type objectType) {
            if (objectType == typeof(Vector2)) return true;
            if (objectType == typeof(Vector3)) return true;
            if (objectType == typeof(Vector4)) return true;
            return false;
        }
    }
}
