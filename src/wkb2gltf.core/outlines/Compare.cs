using System;
using System.Numerics;
using Wkx;

namespace Wkb2Gltf.outlines;
public static class Compare
{
    public static bool IsSimilarPoint(Point p, Point q, double distanceTolerance = 0.01)
    {
        var v0 = new Vector3((float)p.X, (float)p.Y, (float)p.Z);
        var v1 = new Vector3((float)q.X, (float)q.Y, (float)q.Z);

        return Compare.IsAlmostEqual(v0, v1, distanceTolerance);
    }

    public static bool IsAlmostEqual(Vector3 a, Vector3 b, double tolerance)
    {
        if (Math.Abs(a.X - b.X) > tolerance) return false;
        if (Math.Abs(a.Y - b.Y) > tolerance) return false;
        if (Math.Abs(a.Z - b.Z) > tolerance) return false;
        return true;
    }

    public static bool IsAlmostEqual(float a, float b, double tolerance)
    {
        if (Math.Abs(a - b) > tolerance) return false;
        return true;
    }

}
