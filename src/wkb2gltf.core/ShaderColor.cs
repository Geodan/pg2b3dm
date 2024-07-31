using Newtonsoft.Json;

namespace Wkb2Gltf;

public class ShaderColor
{
    public string EmissiveColor { get; set; }
    [JsonProperty(PropertyName = "PbrSpecularGlossiness")]
    public PbrSpecularGlossinessColor PbrSpecularGlossinessColor { get; set; }

    [JsonProperty(PropertyName = "PbrMetallicRoughness")]
    public PbrMetallicRoughnessColor PbrMetallicRoughnessColor { get; set; }


    public Shader ToShader(int i)
    {
        var shader = new Shader();
        shader.EmissiveColor = EmissiveColor;

        shader.PbrSpecularGlossiness = (PbrSpecularGlossinessColor != null ?
            new PbrSpecularGlossiness() {
                DiffuseColor = PbrSpecularGlossinessColor.DiffuseColor,
                SpecularGlossiness = PbrSpecularGlossinessColor.SpecularGlossinessColor
            } :
            null);

        shader.PbrMetallicRoughness = (PbrMetallicRoughnessColor != null ?
            new PbrMetallicRoughness() {
                MetallicRoughness = PbrMetallicRoughnessColor.MetallicRoughnessColor,
                BaseColor = PbrMetallicRoughnessColor.BaseColor
            } :
            null);
        return shader;
    }
}

public class PbrSpecularGlossinessColor
{
    public string DiffuseColor { get; set; }
    [JsonProperty(PropertyName = "SpecularGlossiness")]
    public string SpecularGlossinessColor { get; set; }
}

public class PbrMetallicRoughnessColor
{
    [JsonProperty(PropertyName = "MetallicRoughness")]
    public string MetallicRoughnessColor { get; set; }
    public string BaseColor { get; set; }
}
