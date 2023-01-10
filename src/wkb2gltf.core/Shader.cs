namespace Wkb2Gltf;

public class Shader
{
    public string EmissiveColor { get; set; }

    public PbrSpecularGlossiness PbrSpecularGlossiness { get; set; }

    public PbrMetallicRoughness PbrMetallicRoughness { get; set; }

    public override bool Equals(object other)
    {
        var otherShader = (Shader)other;
        var emissive = (EmissiveColor == otherShader.EmissiveColor);
        var spec = false;
        var mr = false;
        if(PbrSpecularGlossiness != null && otherShader.PbrSpecularGlossiness != null) {
            spec = PbrSpecularGlossiness.Equals(otherShader.PbrSpecularGlossiness);
        }
        else if(PbrSpecularGlossiness==null && otherShader.PbrSpecularGlossiness == null) {
            spec = true;
        }

        if (PbrMetallicRoughness != null && otherShader.PbrMetallicRoughness != null) {
            mr = PbrMetallicRoughness.Equals(otherShader.PbrMetallicRoughness);
        }
        else if (PbrMetallicRoughness == null && otherShader.PbrMetallicRoughness == null) {
            mr = true;
        }

        if (spec && emissive && mr) {
            return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
