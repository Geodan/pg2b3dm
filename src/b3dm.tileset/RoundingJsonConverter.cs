using System;
using Newtonsoft.Json;

namespace B3dm.Tileset
{
    public class RoundingJsonConverter : JsonConverter
    {
        int _precision;
        MidpointRounding _rounding;

        public RoundingJsonConverter()
            : this(2)
        {
        }

        public RoundingJsonConverter(int precision)
            : this(precision, MidpointRounding.AwayFromZero)
        {
        }

        public RoundingJsonConverter(int precision, MidpointRounding rounding)
        {
            _precision = precision;
            _rounding = rounding;
        }

        public override bool CanRead {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Math.Round((double)value, _precision, _rounding));
        }
    }
}