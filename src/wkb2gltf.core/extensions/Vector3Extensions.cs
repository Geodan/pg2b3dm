using System.Numerics;

namespace Wkb2Gltf.extensions;
public static class Vector3Extensions
{
    public static Vector3 Normalize(this Vector3 vector3)
    {
        return vector3 / vector3.Length();
    }

    public static Wkx.Point ToPoint(this Vector3 vector)
    {
        return new Wkx.Point(vector.X, vector.Y, vector.Z);
    }
}
