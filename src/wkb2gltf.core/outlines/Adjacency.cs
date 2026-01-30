using System.Collections.Generic;
using System.Linq;

namespace Wkb2Gltf.outlines;
public static class Adjacency
{
    /// <summary>
    /// 
    /// </summary>
    public static Dictionary<int, List<(int from, int to)>> GetAdjacencyList(List<Triangle> triangles, double distanceTolerance = 0.01, double normalTolerance = 0.01)
    {
        var res = new Dictionary<int, List<(int from, int to)>>();

        for (var i = 0; i < triangles.Count; i++) {
            var t0 = triangles[i];

            for (var j = 0; j < triangles.Count; j++) {
                if (i != j) {
                    var boundaries = BoundaryDetection.GetSharedPoints(t0, triangles[j], distanceTolerance, checkCoplanar: true, normalTolerance);
                    if (boundaries.first.Count == 2 && boundaries.second.Count == 2) {
                        Upsert(res, i, boundaries.first[0], boundaries.first[1]);
                        Upsert(res, j, boundaries.second[0], boundaries.second[1]);
                    }
                }
            }
        }

        return res;
    }

    private static void Upsert(Dictionary<int, List<(int from, int to)>> res,
        int i,
        int from,
        int to)
    {
        if (!res.ContainsKey(i)) {
            res.Add(i, new List<(int, int)> { (from, to) });
        }
        else {
            var contains = res[i].Any(m => m.from == from && m.to == to);
            var contains1 = res[i].Any(m => m.from == to && m.to == from);
            if (!contains && !contains1) {
                res[i].Add((from, to));
            }
        }
    }
}
