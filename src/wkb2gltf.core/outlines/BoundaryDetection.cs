﻿using System.Collections.Generic;

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
}
