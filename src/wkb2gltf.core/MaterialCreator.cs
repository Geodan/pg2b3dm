using System.Drawing;
using System.Numerics;
using SharpGLTF.Materials;

namespace Wkb2Gltf;

public class MaterialCreator
{
    public static MaterialBuilder CreateMaterial(Shader shader, bool defaultDoubleSided = true, AlphaMode defaultAlphaMode = AlphaMode.OPAQUE, float defaultAlphaCutoff = 0.5f)
    {
        var material = new MaterialBuilder().
            WithDoubleSide(defaultDoubleSided).
            WithAlpha(defaultAlphaMode, defaultAlphaCutoff);

        if (!string.IsNullOrEmpty(shader.EmissiveColor)) {
            material.WithEmissive(ColorToVector3(RgbaColor.FromHex(shader.EmissiveColor)));
        }
        if (shader.PbrSpecularGlossiness != null) {
#pragma warning disable CS0618 // Type or member is obsolete
            material.WithSpecularGlossinessShader();
#pragma warning restore CS0618 // Type or member is obsolete

            if (!string.IsNullOrEmpty(shader.PbrSpecularGlossiness.DiffuseColor)) {
#pragma warning disable CS0618 // Type or member is obsolete
                material.WithDiffuse(ColorToVector4(RgbaColor.FromHex(shader.PbrSpecularGlossiness.DiffuseColor)));
#pragma warning restore CS0618 // Type or member is obsolete
            }
            if (!string.IsNullOrEmpty(shader.PbrSpecularGlossiness.SpecularGlossiness)) {
                var c = ColorToVector4(RgbaColor.FromHex(shader.PbrSpecularGlossiness.SpecularGlossiness));
                var specular = new Vector3(c.X, c.Y, c.Z);
                var glossiness = c.Z;
#pragma warning disable CS0618 // Type or member is obsolete
                material.WithSpecularGlossiness(specular, glossiness);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
        else if (shader.PbrMetallicRoughness != null) {
            material.WithMetallicRoughnessShader();
            if (!string.IsNullOrEmpty(shader.PbrMetallicRoughness.BaseColor)) {
                material.WithBaseColor(ColorToVector4(RgbaColor.FromHex(shader.PbrMetallicRoughness.BaseColor)));
            }

            if (!string.IsNullOrEmpty(shader.PbrMetallicRoughness.MetallicRoughness)) {
                var html = RgbaColor.FromHex(shader.PbrMetallicRoughness.MetallicRoughness);
                var c = ColorToVector4(html);
                material.WithMetallicRoughness(c.X, c.Y);
            }
        }

        // todo: implement 'fallback' method (when both PbrSpecularGlossiness and PbrMetallicRoughness are implemented

        return material;
    }

    private static Vector4 ColorToVector4(Color c)
    {
        var v = new Vector4((float)c.R / 255, (float)c.G / 255, (float)c.B / 255, (float)c.A / 255);
        return v;
    }
    private static Vector3 ColorToVector3(Color c)
    {
        var v = new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
        return v;
    }
}
