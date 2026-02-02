using System.Collections.Generic;
using System.Linq;

namespace Wkb2Gltf.outlines;
public static class Part
{
    public static List<uint> GetOutlines(List<Triangle> triangles, List<uint> indices, uint offset = 0, double distanceTolerance = 0.01, double normalTolerance = 0.01)
    {
        var result = new List<uint>();
        if (triangles.Count == 1) {
            result = GetOutlines(triangles[0], offset: offset + indices[0] * 3);
        }
        else if (triangles.Count > 1) {
            var adjacency = Adjacency.GetAdjacencyList(triangles, distanceTolerance, normalTolerance);
            var i = 0;
            foreach (var triangle in triangles) {
                List<(int from, int to)> val;
                if (adjacency.TryGetValue(i, out val)) {
                    var outlines = GetOutlines(triangle, adjacency[i], offset + indices[i] * 3);
                    result.AddRange(outlines);
                }
                i++;
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the outlines for 1 triangle, except list contains the outlines (from, to) to exclude
    /// </summary>
    public static List<uint> GetOutlines(Triangle triangle, List<(int from, int to)> except = null, uint offset = 0)
    {
        List<uint> result = new List<uint>();
        if (except != null) {
            if (!Contains(except, 0, 1)) {
                result.Add(0 + offset);
                result.Add(1 + offset);
            };

            if (!Contains(except, 1, 2)) {
                result.Add(1 + offset);
                result.Add(2 + offset);
            };

            if (!Contains(except, 2, 0)) {
                result.Add(2 + offset);
                result.Add(0 + offset);
            };
        }
        else {
            result = new List<uint>() { offset, offset + 1, offset + 1, offset + 2, offset + 2, offset };
        }
        return result;
    }

    /// <summary>
    /// Checks if an outline (or the reverse) already exists or not
    /// </summary>
    public static bool Contains(List<(int from, int to)> list, int from, int to)
    {
        var contains = list.Any(m => m.from == from && m.to == to);
        var contains1 = list.Any(m => m.from == to && m.to == from);

        return contains || contains1;
    }
}
