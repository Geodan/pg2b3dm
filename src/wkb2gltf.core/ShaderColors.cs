using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wkb2Gltf;

public class ShaderColors
{
    public List<string> EmissiveColors { get; set; }
    [JsonProperty(PropertyName = "PbrSpecularGlossiness")]
    public PbrSpecularGlossinessColors PbrSpecularGlossinessColors { get; set; }

    [JsonProperty(PropertyName = "PbrMetallicRoughness")]
    public PbrMetallicRoughnessColors PbrMetallicRoughnessColors { get; set; }


    public Shader ToShader(int i)
    {
        var shader = new Shader();
        shader.EmissiveColor = (EmissiveColors != null ? EmissiveColors[i] : null);

        shader.PbrSpecularGlossiness = (PbrSpecularGlossinessColors != null ?
            new PbrSpecularGlossiness() {
                DiffuseColor = GetItem(PbrSpecularGlossinessColors.DiffuseColors, i),
                SpecularGlossiness = GetItem(PbrSpecularGlossinessColors.SpecularGlossinessColors, i)
            } :
            null);

        shader.PbrMetallicRoughness = (PbrMetallicRoughnessColors != null ?
            new PbrMetallicRoughness() {
                MetallicRoughness = GetItem(PbrMetallicRoughnessColors.MetallicRoughnessColors, i),
                BaseColor = GetItem(PbrMetallicRoughnessColors.BaseColors, i)
            } :
            null);
        return shader;
    }

    public void Validate(int expectedGeometries)
    {
        var errors = new List<string>();
        Check(EmissiveColors, expectedGeometries, "Emissive", errors);

        if (PbrSpecularGlossinessColors != null) {
            Check(PbrSpecularGlossinessColors.DiffuseColors, expectedGeometries, "Diffuse", errors);
            Check(PbrSpecularGlossinessColors.SpecularGlossinessColors, expectedGeometries, "SpecularGlossines", errors);
        }

        if (PbrMetallicRoughnessColors != null) {
            Check(PbrMetallicRoughnessColors.MetallicRoughnessColors, expectedGeometries, "MetallicRoughness", errors);
            Check(PbrMetallicRoughnessColors.BaseColors, expectedGeometries, "BaseColor", errors);
        }

        if (errors.Count > 0) {
            throw new ArgumentOutOfRangeException($"Shader error for {string.Join(", ", errors)}. Expected amount: {expectedGeometries}");
        }
    }

    private void Check(List<string> colors, int expectedGeometries, string error, List<string> errors)
    {
        // check the amount of colors is not null, one or the same as the geometries or 1
        if (colors != null && colors.Count != expectedGeometries && colors.Count!= 1) {
            errors.Add(error);
        }
    }

    private string GetItem(List<string> items, int i)
    {
        // if there is only one item, always return the first
        // use for having 1 shader per geometry
        if(items!=null && items.Count == 1) {
            i = 0;
        }
        return items != null ? items[i] : null;
    }
}

public class PbrSpecularGlossinessColors
{
    public List<string> DiffuseColors { get; set; }
    [JsonProperty(PropertyName = "SpecularGlossiness")]
    public List<string> SpecularGlossinessColors { get; set; }
}

public class PbrMetallicRoughnessColors
{
    [JsonProperty(PropertyName = "MetallicRoughness")]
    public List<string> MetallicRoughnessColors { get; set; }
    public List<string> BaseColors { get; set; }
}
