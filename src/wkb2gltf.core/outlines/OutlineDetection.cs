using System;
using System.Collections.Generic;
using System.Linq;

namespace Wkb2Gltf.outlines;
public static class OutlineDetection
{
    /// <summary>
    /// Get outlines per geometry (list of triangles)
    /// </summary>
    public static List<uint> GetOutlines(uint[] indices, List<List<Triangle>> triangles, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var outlines = new List<uint>();

        uint offset = 0;

        var i = 0;
        foreach (var geometryTriangles in triangles) {
            var outline = GetOutlines(geometryTriangles, offset: offset, normalTolerance: normalTolerance, distanceTolerance);
            outlines.AddRange(outline);
            offset += (uint)geometryTriangles.Count * 3;
            i++;
        }

        var res = new List<uint>();
        foreach (var outline in outlines) {
            res.Add(indices[outline]);
        }

        return res;
    }

    public static List<uint> GetOutlines(List<Triangle> triangles, uint offset = 0, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var outlines = new List<uint>();

        var parts = PartFinder.GetParts(triangles, normalTolerance, distanceTolerance);

        for (uint p = 0; p < parts.Count; p++) {
            var partTriangles = Triangles.SelectByIndex(triangles, parts[(int)p]);
            var outline = Part.GetOutlines(partTriangles, parts[(int)p], offset, distanceTolerance);
            outlines.AddRange(outline);
        }
        return outlines;
    }
}