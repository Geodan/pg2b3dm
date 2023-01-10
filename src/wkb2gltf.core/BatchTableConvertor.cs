using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wkb2Gltf;

public class BatchTableJsonConverter : JsonConverter
{
    private readonly Type[] _types;

    public BatchTableJsonConverter(params Type[] types)
    {
        _types = types;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var o = new JObject();
        var  batchTable = (BatchTable)value;
        foreach (var item in batchTable.BatchTableItems) {
            o.Add(new JProperty(item.Name, item.Values));
        }
        o.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
    }

    public override bool CanRead {
        get { return false; }
    }

    public override bool CanConvert(Type objectType)
    {
        return _types.Any(t => t == objectType);
    }
}
