using System.Numerics;
using SharpGLTF.Materials;

namespace Wkb2Gltf
{
    public class MaterialCreator
    {
        public static MaterialBuilder CreateMaterial(float r, float g, float b)
        {
            var material = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam(KnownChannel.BaseColor, ColorToVector4(r, g, b));
            return material;
        }

        private static Vector4 ColorToVector4(float r, float g, float b)
        {
            return new Vector4(r / 255, g / 255, b / 255, 1);
        }
    }
}
