using System.Numerics;

namespace Wkb2Gltf.extensions;
public static class Vector3Extensions
{
    public static Vector3 Normalize(this Vector3 vector3)
    {
        return vector3 / vector3.Length();
    }
}
