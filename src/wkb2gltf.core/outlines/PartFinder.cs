using System;
using System.Collections.Generic;
using System.Linq;
using Wkb2Gltf.extensions;

namespace Wkb2Gltf.outlines;
public static class PartFinder
{
    /// <summary>
    /// This method get a list of parts from a set of triangles. A part consists of set of triangles that are connected and have the same normal direction.
    /// </summary>
    public static Dictionary<int, List<uint>> GetParts(List<Triangle> triangles, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var result = new Dictionary<int, List<uint>>();

        var connectedTriangles = GetAllConnectedTriangles(triangles, normalTolerance, distanceTolerance);
        var partId = 0;
        var remaining = new List<uint>();
        for (var i = 0; i < triangles.Count; i++) {
            remaining.Add((uint)i);
        }

        while (remaining.Count > 0) {
            var partItems = GetPartTriangles(connectedTriangles, remaining.First(), new List<uint>());
            partItems.Sort();

            result.Add(partId, partItems);

            partId++;
            foreach (var item in partItems) {
                remaining.Remove(item);
            }
        }

        return result;
    }

    /// <summary>
    /// This recursive method gets all triangles that are connected to the triangle with given ID. It searches in the 
    /// dictionary and stores in the items
    /// </summary>
    public static List<uint> GetPartTriangles(Dictionary<int, List<uint>> dict, uint triangleId, List<uint> items)
    {
        if (!items.Contains(triangleId)) {
            items.Add(triangleId);

            var children = dict[(int)triangleId];

            foreach (var child in children) {
                GetPartTriangles(dict, child, items);
            }
        }

        return items;
    }


    /// <summary>
    /// This method creates a dictionary where connections between triangles are stored.
    /// </summary>
    private static Dictionary<int, List<uint>> GetAllConnectedTriangles(List<Triangle> triangles, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var result = new Dictionary<int, List<uint>>();

        for (var i = 0; i < triangles.Count; i++) {
            var connectedTriangles = GetConnectedTrianglesForTriangle(triangles, (uint)i, normalTolerance, distanceTolerance);
            result[i] = connectedTriangles;
        }

        return result;
    }

    /// <summary>
    /// This method searches in list of triangles the triangles that are connected to triangle with given id
    /// </summary>
    private static List<uint> GetConnectedTrianglesForTriangle(List<Triangle> triangles, uint id, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var res = new List<uint>();
        var t0 = triangles[(int)id];
        var normal0 = t0.GetNormal().Normalize();
        for (uint i = 0; i < triangles.Count; i++) {
            if (i != id) {
                var triangle = triangles[(int)i];
                var n1 = triangle.GetNormal().Normalize();
                var isNormalEqual = Compare.IsAlmostEqual(normal0, n1, normalTolerance);
                var isNormalEqualFlipped = Compare.IsAlmostEqual(normal0, n1 * -1, normalTolerance);

                var normalOk = isNormalEqual || isNormalEqualFlipped;

                var sharedPoints = BoundaryDetection.GetSharedPoints(t0, triangle, distanceTolerance);

                if (normalOk && sharedPoints.first.Count == 2 && sharedPoints.second.Count == 2) {
                    res.Add(i);
                }
            }
        }
        return res;
    }
}
