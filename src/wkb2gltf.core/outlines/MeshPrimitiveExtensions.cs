using System.Collections.Generic;
using System.Linq;
using SharpGLTF.Schema2;

namespace Wkb2Gltf.outlines;
public static class MeshPrimitiveExtensions
{
    // todo: refactor so triangles parameter is no longer needed here
    public static void AddOutlines(this MeshPrimitive meshPrimitive, List<Triangle> triangles)
    {
        //double normalTolerance = 0.01;
        //double distanceTolerance = 0.01;

        var originalIndices = meshPrimitive.IndexAccessor.AsIndicesArray().ToArray();
        var outlines = new uint[originalIndices.Length];
        // var outlines = OutlineDetection.GetOutlines(originalIndices, triangles, normalTolerance: normalTolerance, distanceTolerance).ToArray();
        meshPrimitive.SetCesiumOutline(outlines);
    }
}
