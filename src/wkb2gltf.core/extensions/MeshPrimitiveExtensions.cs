using System.Collections.Generic;
using System.Linq;
using SharpGLTF.Schema2;
using Wkb2Gltf.outlines;

namespace Wkb2Gltf.extensions;
public static class MeshPrimitiveExtensions
{
    // todo: refactor so triangles parameter is no longer needed here
    public static void AddOutlines(this MeshPrimitive meshPrimitive, List<List<Triangle>> triangles)
    {
        var normalTolerance = 0.01;
        var distanceTolerance = 0.01;

        var originalIndices = meshPrimitive.IndexAccessor.AsIndicesArray().ToArray();
        var outlines = OutlineDetection.GetOutlines(originalIndices, triangles, normalTolerance: normalTolerance, distanceTolerance).ToArray();
        meshPrimitive.SetCesiumOutline(outlines);
    }
}
