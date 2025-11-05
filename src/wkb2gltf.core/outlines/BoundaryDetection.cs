using System;
using System.Collections.Generic;
using System.Numerics;

namespace Wkb2Gltf.outlines;
public static class BoundaryDetection
{
    public static int? GetPoint(Triangle triangle, Wkx.Point p, double distanceTolerance = 0.01)
    {
        var points = triangle.GetPoints();
        for (var i = 0; i < points.Count; i++) {
            if (Compare.IsSimilarPoint(p, points[i], distanceTolerance)) {
                return i;
            }
        }
        return null;
    }

    public static (List<int> first, List<int> second) GetSharedPoints(Triangle triangle0, Triangle triangle1, double distanceTolerance = 0.01)
    {
        var t0 = new List<int>();
        var t1 = new List<int>();

        var pnts = triangle0.GetPoints();
        for (var i = 0; i < pnts.Count; i++) {
            var res = GetPoint(triangle1, pnts[i], distanceTolerance);
            if (res != null) {
                t0.Add(i);
                t1.Add((int)res);
            }
        }

        return (t0, t1);
    }

    /// <summary>
    /// Check if two triangles are coplanar (lie on the same plane)
    /// </summary>
    /// <param name="triangle0">First triangle</param>
    /// <param name="triangle1">Second triangle</param>
    /// <param name="distanceTolerance">Tolerance for perpendicular distance from points to plane</param>
    /// <returns>True if triangles are coplanar, false otherwise</returns>
    public static bool AreCoplanar(Triangle triangle0, Triangle triangle1, double distanceTolerance = 0.01)
    {
        // Get the normal of the first triangle
        var normal = triangle0.GetNormal();
        
        // Validate normal is not degenerate (zero-length or NaN)
        if (float.IsNaN(normal.X) || float.IsNaN(normal.Y) || float.IsNaN(normal.Z) || 
            normal.LengthSquared() < float.Epsilon)
        {
            return false;
        }
        
        // Get a point from the first triangle to define the plane
        var p0 = triangle0.GetP0();
        var planePoint = PointToVector3(p0);
        
        // Check if all points of the second triangle lie on the plane defined by triangle0
        var points = triangle1.GetPoints();
        foreach (var point in points)
        {
            var p = PointToVector3(point);
            var diff = p - planePoint;
            var distance = Math.Abs(Vector3.Dot(normal, diff));
            
            if (distance > distanceTolerance)
            {
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Convert Wkx.Point to Vector3
    /// </summary>
    private static Vector3 PointToVector3(Wkx.Point point)
    {
        return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
    }
}
