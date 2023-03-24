using System.Collections.Generic;

namespace Wkb2Gltf.outlines;
public static class PartFinder
{
    /// <summary>
    /// Function that get parts based on normals only (and no adjacency check, to speed up processing)
    /// </summary>
    public static Dictionary<int, List<uint>> GetParts(List<Triangle> triangles, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var result = new Dictionary<int, List<uint>>();
        var partId = 0;
        var normal = triangles[0].GetNormal();
        var partIds = new List<uint> {
            0
        };

        if (triangles.Count > 1) {
            for (var i = 1; i < triangles.Count; i++) {
                var newNormal = triangles[i].GetNormal();
                var isNormalEqual = Compare.IsAlmostEqual(normal, newNormal, normalTolerance) || Compare.IsAlmostEqual(normal, newNormal*-1, normalTolerance);
                if (isNormalEqual) {
                    partIds.Add((uint)i);
                }
                else {
                    result.Add(partId, partIds);
                    partId++;
                    normal = newNormal;
                    partIds = new List<uint>() { (uint)i };
                }

                if (i == triangles.Count - 1) {
                    result.Add(partId, partIds);
                }
            }
        }
        else {
            result.Add(partId, partIds);
        }

        return result;
    }
}
