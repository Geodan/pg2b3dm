using System.Numerics;

namespace Wkb2Gltf
{
    public static class Matrix4x4Extensions
    {
        public static float[] Flatten(this Matrix4x4 m) {
            var floats = new float[] {
                m.M11, m.M21, m.M31, m.M41,
                m.M12, m.M22, m.M32, m.M42,
                m.M13, m.M23, m.M33, m.M43,
                m.M14, m.M24, m.M34, m.M44,

            };
            return floats;
        }
    }
}
