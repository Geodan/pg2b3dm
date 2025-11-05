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
    /// <param name="distanceTolerance">Tolerance for distance comparison</param>
    /// <returns>True if triangles are coplanar, false otherwise</returns>
    public static bool AreCoplanar(Triangle triangle0, Triangle triangle1, double distanceTolerance = 0.01)
    {
        // Get the normal of the first triangle
        var normal = triangle0.GetNormal();
        
        // Get a point from the first triangle to define the plane
        var p0 = triangle0.GetP0();
        var planePoint = new Vector3((float)p0.X, (float)p0.Y, (float)p0.Z);
        
        // Check if all points of the second triangle lie on the plane defined by triangle0
        var points = triangle1.GetPoints();
        foreach (var point in points)
        {
            var p = new Vector3((float)point.X, (float)point.Y, (float)point.Z);
            var diff = p - planePoint;
            var distance = Math.Abs(Vector3.Dot(normal, diff));
            
            if (distance > distanceTolerance)
            {
                return false;
            }
        }
        
        return true;
    }
}
