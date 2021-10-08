using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using PartyListLayout.Config;
using PartyListLayout.Helper;

namespace PartyListLayout.Converter {
    public static class LayoutSerializer {
        public static List<byte> SerializeObject(object obj) {
            var bytes = new List<byte>();

            switch (obj) {
                case Vector2 v2:
                    bytes.AddRange(SerializePrimitive(v2.X));
                    bytes.AddRange(SerializePrimitive(v2.Y));
                    break;
                case Vector3 v3:
                    bytes.AddRange(SerializePrimitive(v3.X));
                    bytes.AddRange(SerializePrimitive(v3.Y));
                    bytes.AddRange(SerializePrimitive(v3.Z));
                    break;
                case Vector4 v4:
                    bytes.AddRange(SerializePrimitive(v4.X));
                    bytes.AddRange(SerializePrimitive(v4.Y));
                    bytes.AddRange(SerializePrimitive(v4.Z));
                    bytes.AddRange(SerializePrimitive(v4.W));
                    break;
                case ElementConfig ec:
                    return null;
                default:
                    return null;
            }

            return bytes;
        }

        public static byte[] SerializePrimitive(object obj) {
            return obj switch {
                bool b => new byte[1] { (byte)(b ? 1 : 0) },
                int i => MakeInteger(i),
                float f => BitConverter.GetBytes(f),
                _ => null
            };
        }

        public static List<byte> SerializeValue(object obj) {
            return obj.GetType().IsPrimitive ? new List<byte>(SerializePrimitive(obj)) : SerializeObject(obj);
        }

        public static byte[] SerializeLayout(LayoutConfig layoutConfig) {
            var bytes = new List<byte>();


            bytes.AddRange(Encoding.UTF8.GetBytes("PartyListLayout"));
            bytes.Add(1);

            List<(FieldInfo field, SerializeKeyAttribute serializeKey)> fields = layoutConfig.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => (f, (SerializeKeyAttribute) f.GetCustomAttribute(typeof(SerializeKeyAttribute)))).OrderBy(f => f.Item2.Key).ToList();

            foreach (var (field, serializeKey) in fields) {
                if (serializeKey == null) {
                    SimpleLog.Error($"Missing SerializeKey on Layout Config Field: {field.Name}");
                    continue;
                }

                var fieldBytes = new List<byte>();
                fieldBytes.AddRange(BitConverter.GetBytes((ushort) serializeKey.Key));


                if (field.FieldType.IsPrimitive) {
                    var primitiveBytes = SerializePrimitive(field.GetValue(layoutConfig));
                    if (primitiveBytes == null) {
                        SimpleLog.Fatal($"Unhandled primitive field type encountered during serialization of {field.Name}: {field.FieldType.Name}");
                        continue;
                    }

                    fieldBytes.AddRange(primitiveBytes);
                } else if (field.FieldType == typeof(ElementConfig) || field.FieldType.IsSubclassOf(typeof(ElementConfig))) {

                    var eCfg = (ElementConfig) field.GetValue(layoutConfig);
                    var defaultCfgValue = (ElementConfig) field.GetValue(LayoutConfig.Default);

                    var layoutElement = (LayoutElementAttribute) field.GetCustomAttribute(typeof(LayoutElementAttribute));

                    if (layoutElement == null) {
                        SimpleLog.Fatal($"ElementConfig with not LayoutElementAttribute encountered during serialization of {field.Name}.");
                        continue;
                    }

                    if (eCfg.Equals(defaultCfgValue)) {
                        SimpleLog.Log($"Skip Default Value: {field.Name}");
                        continue;
                    }

                    var eCfgFields = eCfg.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var eField in eCfgFields) {

                        var eSerialize = (SerializeKeyAttribute) eField.GetCustomAttribute(typeof(SerializeKeyAttribute));

                        if (eSerialize == null) {
                            SimpleLog.Fatal($"ElementConfig field with no SerializeKey encourtered during serialization of {field.Name}.{eField.Name}.");
                            continue;
                        }

                        if (eSerialize.ExcludeFlag != LayoutElementFlags.None) {
                            if (layoutElement.Flags.HasFlag(eSerialize.ExcludeFlag)) continue;
                        }

                        if (eSerialize.RequireFlag != LayoutElementFlags.None) {
                            if (!layoutElement.Flags.HasFlag(eSerialize.RequireFlag)) continue;
                        }

                        fieldBytes.AddRange(BitConverter.GetBytes((ushort) eSerialize.Key));
                        fieldBytes.AddRange(SerializeValue(eField.GetValue(eCfg)));
                    }
                    fieldBytes.Add(0);
                    fieldBytes.Add(0);
                } else {
                    var objectBytes = SerializeObject(field.GetValue(layoutConfig));
                    if (objectBytes == null) {
                        SimpleLog.Fatal($"Unhandled object field type encountered during serialization of {field.Name}: {field.FieldType.Name}");
                        continue;
                    }

                    fieldBytes.AddRange(objectBytes);

                    continue;
                }
                bytes.AddRange(fieldBytes);
            }

            return bytes.ToArray();
        }

        public static LayoutConfig DeserializeLayout(byte[] bytes) {
            var layout = new LayoutConfig();
            DeserializeLayout(bytes, layout);
            return layout;
        }


        private static SerializeKey GetNextKey(List<byte> bytes) {
            var keyBytes = TakeBytes(bytes, 2);
            return (SerializeKey) BitConverter.ToUInt16(keyBytes, 0);
        }

        private static byte[] TakeBytes(List<byte> bytes, int count) {
            if (bytes.Count < count) throw new Exception($"Unexpected end of Export String. Expected at least {count} more bytes.");
            var retBytes = bytes.Take(count).ToArray();
            bytes.RemoveRange(0, count);
            return retBytes;
        }

        private static byte TakeByte(List<byte> bytes) {
            return TakeBytes(bytes, 1)[0];
        }

        private static float DeserializeFloat(List<byte> bytes) => BitConverter.ToSingle(TakeBytes(bytes, 4), 0);

        private static void DeserializeField(List<byte> bytes, FieldInfo field, object obj) {
            if (field.FieldType == typeof(bool)) {
                var b = TakeBytes(bytes, 1);
                field.SetValue(obj, b[0] != 0);
            } else if (field.FieldType == typeof(int)) {
                var b = GetInteger(bytes);
                field.SetValue(obj, (int)b);
            } else if (field.FieldType == typeof(float)) {
                var b = TakeBytes(bytes, 4);
                field.SetValue(obj, BitConverter.ToSingle(b, 0));
            } else if (field.FieldType == typeof(Vector2)) {
                field.SetValue(obj, new Vector2(DeserializeFloat(bytes), DeserializeFloat(bytes)));
            } else if (field.FieldType == typeof(Vector3)) {
                field.SetValue(obj, new Vector3(DeserializeFloat(bytes), DeserializeFloat(bytes), DeserializeFloat(bytes)));
            } else if (field.FieldType == typeof(Vector4)) {
                field.SetValue(obj, new Vector4(DeserializeFloat(bytes), DeserializeFloat(bytes), DeserializeFloat(bytes), DeserializeFloat(bytes)));
            }
            else if (field.FieldType == typeof(ElementConfig) || field.FieldType.IsSubclassOf(typeof(ElementConfig))) {
                List<(FieldInfo field, SerializeKeyAttribute serializeKey)> eFields = field.GetValue(obj).GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => (f, (SerializeKeyAttribute) f.GetCustomAttribute(typeof(SerializeKeyAttribute)))).OrderBy(f => f.Item2.Key).ToList();

                while (bytes.Count >= 2) {
                    var key = GetNextKey(bytes);
                    if (key == SerializeKey.None) break;
                    try {
                        var eField = eFields.First(f => f.serializeKey?.Key == key);
                        DeserializeField(bytes, eField.field, field.GetValue(obj));
                    } catch {
                        SimpleLog.Log($"No field with key in {field.Name}: {key}");
                    }
                }
            }
            else {
                SimpleLog.Error($"Unhandled Import for {field.FieldType.Name} in {field.Name}");
            }
        }

        public static bool DeserializeLayout(byte[] bytes, LayoutConfig layoutConfig) {
            var byteList = new List<byte>(bytes);

            if (Encoding.UTF8.GetString(bytes, 0, 15) != "PartyListLayout") {
                throw new Exception("Incorrect Header");
            }

            var exportVersion = byteList[15];

            if (exportVersion != 1) {
                throw new Exception("Unsupported export version. Please update the plugin.");
            }

            byteList.RemoveRange(0, 16);

            List<(FieldInfo field, SerializeKeyAttribute serializeKey)> fields = layoutConfig.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => (f, (SerializeKeyAttribute) f.GetCustomAttribute(typeof(SerializeKeyAttribute)))).OrderBy(f => f.Item2.Key).ToList();

            while (byteList.Count >= 2) {
                var key = GetNextKey(byteList);

                try {
                    var (field, serializeKey) = fields.First(f => f.serializeKey?.Key == key);
                    DeserializeField(byteList, field, layoutConfig);
                } catch {
                    SimpleLog.Log($"No field with Key: {key}");
                    return false;
                }
            }

            return true;
        }

        private static byte[] MakeInteger(int value) => MakeInteger((uint)value);
        private static byte[] MakeInteger(uint value)
        {
            if (value < 0xCF)
            {
                return new byte[] { (byte)(value + 1) };
            }

            var bytes = BitConverter.GetBytes(value);

            var ret = new List<byte>() { 0xF0 };
            for (var i = 3; i >= 0; i--)
            {
                if (bytes[i] != 0)
                {
                    ret.Add(bytes[i]);
                    ret[0] |= (byte)(1 << i);
                }
            }

            ret[0] -= 1;

            return ret.ToArray();
        }

        private static uint GetInteger(List<byte> bytes)
        {
            uint marker = TakeByte(bytes);
            if (marker < 0xD0)
                return marker - 1;

            // the game adds 0xF0 marker for values >= 0xCF
            // uasge of 0xD0-0xEF is unknown, should we throw here?
            // if (marker < 0xF0) throw new NotSupportedException();

            marker = (marker + 1) & 0b1111;

            var ret = new byte[4];
            for (var i = 3; i >= 0; i--)
            {
                ret[i] = (marker & (1 << i)) == 0 ? (byte)0 : TakeByte(bytes);
            }

            return BitConverter.ToUInt32(ret, 0);
        }




    }
}
