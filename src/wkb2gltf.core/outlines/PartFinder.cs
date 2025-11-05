using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Wkb2Gltf.outlines;
public static class PartFinder
{
    /// <summary>
    /// Function that gets parts based on normals and connectivity
    /// Uses a two-phase approach: first group by normal (with tolerance), then split by connectivity
    /// </summary>
    public static Dictionary<int, List<uint>> GetParts(List<Triangle> triangles, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        if (triangles.Count == 0) {
            return new Dictionary<int, List<uint>>();
        }

        if (triangles.Count == 1) {
            return new Dictionary<int, List<uint>> { { 0, new List<uint> { 0 } } };
        }

        // Use a larger normal tolerance for grouping to handle sloped surfaces
        // A dot product of 0.707 corresponds to ~45 degree angle difference
        // This should separate walls (horizontal normal) from floors/roofs (vertical normal)
        // while allowing triangulation of sloped roofs
        var groupingTolerance = 0.3;  // dot product > 0.7 means angle < ~45 degrees
        
        // First, group triangles by normal (with permissive tolerance)
        var normalGroups = GroupByNormal(triangles, groupingTolerance);

        // Then, split each normal group into connected components
        var result = new Dictionary<int, List<uint>>();
        var partId = 0;

        foreach (var normalGroup in normalGroups) {
            var connectedParts = FindConnectedComponents(triangles, normalGroup, distanceTolerance);
            foreach (var part in connectedParts) {
                result.Add(partId, part);
                partId++;
            }
        }

        return result;
    }

    private static List<List<uint>> GroupByNormal(List<Triangle> triangles, double normalTolerance)
    {
        var groups = new List<List<uint>>();
        var assigned = new bool[triangles.Count];

        for (var i = 0; i < triangles.Count; i++) {
            if (assigned[i]) continue;

            var normal = triangles[i].GetNormal();
            var group = new List<uint> { (uint)i };
            assigned[i] = true;

            for (var j = i + 1; j < triangles.Count; j++) {
                if (assigned[j]) continue;

                var otherNormal = triangles[j].GetNormal();
                var dotProduct = Vector3.Dot(normal, otherNormal);
                // Use > instead of IsAlmostEqual to allow a range of angles
                if (dotProduct > 1.0f - normalTolerance) {
                    group.Add((uint)j);
                    assigned[j] = true;
                }
            }

            groups.Add(group);
        }

        return groups;
    }

    private static List<List<uint>> FindConnectedComponents(List<Triangle> allTriangles, List<uint> indices, double distanceTolerance)
    {
        if (indices.Count == 0) {
            return new List<List<uint>>();
        }

        if (indices.Count == 1) {
            return new List<List<uint>> { new List<uint> { indices[0] } };
        }

        // Build adjacency map for triangles in this group
        var adjacencyMap = new Dictionary<uint, List<uint>>();
        foreach (var idx in indices) {
            adjacencyMap[idx] = new List<uint>();
        }

        for (var i = 0; i < indices.Count; i++) {
            for (var j = i + 1; j < indices.Count; j++) {
                var idx1 = indices[i];
                var idx2 = indices[j];
                var t1 = allTriangles[(int)idx1];
                var t2 = allTriangles[(int)idx2];

                // Check if triangles share an edge
                var sharedPoints = BoundaryDetection.GetSharedPoints(t1, t2, distanceTolerance);
                if (sharedPoints.first.Count >= 2) {
                    adjacencyMap[idx1].Add(idx2);
                    adjacencyMap[idx2].Add(idx1);
                }
            }
        }

        // Find connected components using flood fill
        var visited = new HashSet<uint>();
        var components = new List<List<uint>>();

        foreach (var startIdx in indices) {
            if (visited.Contains(startIdx)) continue;

            var component = new List<uint>();
            var queue = new Queue<uint>();
            queue.Enqueue(startIdx);
            visited.Add(startIdx);

            while (queue.Count > 0) {
                var current = queue.Dequeue();
                component.Add(current);

                foreach (var neighbor in adjacencyMap[current]) {
                    if (!visited.Contains(neighbor)) {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            components.Add(component);
        }

        return components;
    }
}
