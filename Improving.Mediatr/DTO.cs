using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Improving.MediatR
{
    using Newtonsoft.Json;

    public abstract class DTO
    {
        public static readonly JsonSerializerSettings DefaultJsonSettings =
            new JsonSerializerSettings
            {
                Formatting            = Formatting.Indented,
                DateFormatString      = "MM-dd-yyyy hh:mm:ss",
                NullValueHandling     = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters            =  {
                    new StringEnumConverter(),
                    new ByteArrayFormatter()
                }
            };

        public override string ToString()
        {
            return ToString(this);
        }

        public static string ToString(object dto)
        {
            if (dto == null) return "";
            return new StringBuilder(PrettyName(dto.GetType())).Append(" ")
                .Append(JsonConvert.SerializeObject(dto, DefaultJsonSettings))
                .Replace("\"", "")
                .ToString();
        }

        public static string PrettyName(Type type)
        {
            if (type.IsGenericType)
            {
                return (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                     ? $"{PrettyName(Nullable.GetUnderlyingType(type))}?"
                     : $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GenericTypeArguments.Select(a => PrettyName(a)).ToArray())}>";
            }
            if (type.IsArray)
                return $"{PrettyName(type.GetElementType())}[]";

            string name;
            return SimpleNames.TryGetValue(type, out name) ? name : type.Name;
        }

        private static readonly Dictionary<Type, string> SimpleNames 
            = new Dictionary<Type, string>
              {
                  { typeof (Boolean), "bool" },
                  { typeof (Byte),    "byte" },
                  { typeof (Char),    "char" },
                  { typeof (Decimal), "decimal" },
                  { typeof (Double),  "double" },
                  { typeof (Single),  "float" },
                  { typeof (Int32),   "int" },
                  { typeof (Int64),   "long" },
                  { typeof (SByte),   "sbyte" },
                  { typeof (Int16),   "short" },
                  { typeof (String),  "string "},
                  { typeof (UInt32),  "uint" },
                  { typeof (UInt64),  "ulong" },
                  { typeof (UInt16),  "ushort" }
              };

        class ByteArrayFormatter : JsonConverter
        {
            public override bool CanRead => false;

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof (byte[]);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var bytes = (byte[]) value;
                writer.WriteValue($"({bytes.Length} bytes)");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false.");
            }
        }
    }
}
