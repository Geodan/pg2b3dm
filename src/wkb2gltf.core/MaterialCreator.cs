using System.Drawing;
using System.Numerics;
using SharpGLTF.Materials;

namespace Wkb2Gltf
{
    public class MaterialCreator
    {
        public static MaterialBuilder CreateMaterial(Color c)
        {
            var material = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithAlpha(AlphaMode.BLEND).
                WithChannelParam(KnownChannel.BaseColor, ColorToVector4(c));
            return material;
        }

        private static Vector4 ColorToVector4(Color c)
        {
            var v = new Vector4((float)c.R / 255, (float)c.G / 255, (float)c.B / 255, (float)c.A/255);
            return v;
        }
    }
}
