using System.Collections.Generic;
using System.Linq;
using SharpGLTF.Geometry;
using SharpGLTF.Schema2;
using Wkb2Gltf.extensions;

namespace Wkb2Gltf.outlines;
public static class OutlineDetection
{
    /// <summary>
    /// Get outlines of a meshPrimitive
    /// </summary>
    public static List<uint> GetOutlines(MeshPrimitive meshPrimitive, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var indices = meshPrimitive.GetIndices().ToArray();
        var outlines = new List<uint>();

        var tris = Toolkit.EvaluateTriangles(meshPrimitive).ToList();
        var triangles = GetTriangles(tris);
        var outline = GetOutlines2(triangles, normalTolerance: normalTolerance, distanceTolerance);
        outlines.AddRange(outline);

        var res = new List<uint>();
        foreach (var l in outlines) {
            res.Add(indices[l]);
        }

        return res;
    }

    public static List<uint> GetOutlines2(List<Triangle> triangles, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {

        var outlines = new List<uint>();
        var parts = PartFinder.GetParts2(triangles, normalTolerance, distanceTolerance);

        for (uint p = 0; p < parts.Count; p++) {
            var partTriangles = Triangles.SelectByIndex(triangles, parts[(int)p]);
            var outline = Part.GetOutlines(partTriangles, parts[(int)p], 0, distanceTolerance);
            outlines.AddRange(outline);
        }
        return outlines;
    }

    private static List<Triangle> GetTriangles(List<(IVertexBuilder A, IVertexBuilder B, IVertexBuilder C, Material Material)> tris)
    {
        var res = new List<Triangle>();
        foreach (var tri in tris) {
            var p0 = tri.A.GetGeometry().GetPosition().ToPoint();
            var p1 = tri.B.GetGeometry().GetPosition().ToPoint();
            var p2 = tri.C.GetGeometry().GetPosition().ToPoint();

            var t = new Triangle(p0, p1, p2, 0);
            res.Add(t);
        }

        return res;
    }

}