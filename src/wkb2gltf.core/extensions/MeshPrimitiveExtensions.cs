using System.Collections.Generic;
using System.Linq;
using SharpGLTF.Geometry;
using SharpGLTF.Schema2;
using Wkb2Gltf.outlines;

namespace Wkb2Gltf.extensions;
public static class MeshPrimitiveExtensions
{
    // todo: refactor so triangles parameter is no longer needed here
    public static void AddOutlines(this MeshPrimitive meshPrimitive)
    {
        var tris = Toolkit.EvaluateTriangles(meshPrimitive).ToList();
        var triangles = GetTriangles(tris);
        var trianglesWrapped = new List<List<Triangle>>() { triangles }; 

        var normalTolerance = 0.01;
        var distanceTolerance = 0.01;

        var originalIndices = meshPrimitive.GetIndices().ToArray();

        var outlines = OutlineDetection.GetOutlines(originalIndices, trianglesWrapped, normalTolerance: normalTolerance, distanceTolerance).ToArray();
        meshPrimitive.SetCesiumOutline(outlines);
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
