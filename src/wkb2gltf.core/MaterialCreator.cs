using System.Drawing;
using System.Numerics;
using SharpGLTF.Materials;

namespace Wkb2Gltf
{
    public class MaterialCreator
    {
        public static MaterialBuilder GetDefaultMaterial(string color)
        {
            var rgb = ColorTranslator.FromHtml(color);

            var material = new MaterialBuilder().
            WithDoubleSide(true).
            WithMetallicRoughnessShader().
            WithAlpha(AlphaMode.BLEND).
            WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, ColorToVector4(rgb));
            return material;
        }

        public static MaterialBuilder CreateMaterial(Shader shader)
        {
            var material = new MaterialBuilder().
                WithDoubleSide(true).
                WithAlpha(AlphaMode.OPAQUE);

            if (!string.IsNullOrEmpty(shader.EmissiveColor)) {
                material.WithEmissive(ColorToVector3(ColorTranslator.FromHtml(shader.EmissiveColor)));
            }
            if (shader.PbrSpecularGlossiness != null) {
                material.WithSpecularGlossinessShader();

                if (!string.IsNullOrEmpty(shader.PbrSpecularGlossiness.DiffuseColor)) {
                    material.WithDiffuse(ColorToVector4(ColorTranslator.FromHtml(shader.PbrSpecularGlossiness.DiffuseColor)));
                }
                if (!string.IsNullOrEmpty(shader.PbrSpecularGlossiness.SpecularGlossiness)) {
                    var c = ColorToVector4(ColorTranslator.FromHtml(shader.PbrSpecularGlossiness.SpecularGlossiness));
                    var specular = new Vector3(c.X, c.Y, c.Z);
                    var glossiness = c.Z;
                    material.WithSpecularGlossiness(specular, glossiness);
                }
            }
            else if (shader.PbrMetallicRoughness != null) {
                material.WithMetallicRoughnessShader();
                if (!string.IsNullOrEmpty(shader.PbrMetallicRoughness.BaseColor)) {
                    material.WithBaseColor(ColorToVector4(ColorTranslator.FromHtml(shader.PbrMetallicRoughness.BaseColor)));
                }

                if (!string.IsNullOrEmpty(shader.PbrMetallicRoughness.MetallicRoughness)) {
                    var c = ColorToVector4(ColorTranslator.FromHtml(shader.PbrMetallicRoughness.MetallicRoughness));
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
}
