using System.Numerics;
using Wkx;

namespace Wkb2Gltf.Extensions;

public static class PointExtensions
{
    public static Vector3 ToVector(this Point p)
    {
        return new Vector3((float)p.X, (float)p.Y, (float)p.Z);
    }

    public static Vector3 Minus(this Point p, Point other)
    {
        var x = p.X - other.X;
        var y = p.Y - other.Y;
        var z = p.Z - other.Z;
        if (z == null) {
            z = 0;
        }
        return new Vector3((float)x,(float)y,(float)z);
    }
}
