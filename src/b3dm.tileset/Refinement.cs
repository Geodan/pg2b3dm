using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace B3dm.Tileset;

[JsonConverter(typeof(StringEnumConverter))]

public enum RefinementType
{
    ADD, REPLACE
}
