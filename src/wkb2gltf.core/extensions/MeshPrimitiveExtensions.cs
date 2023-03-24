using SharpGLTF.Schema2;
using Wkb2Gltf.outlines;

namespace Wkb2Gltf.extensions;
public static class MeshPrimitiveExtensions
{
    public static void AddOutlines(this MeshPrimitive meshPrimitive)
    {
        var normalTolerance = 0.1;
        var distanceTolerance = 0.01;
        var outlines = OutlineDetection.GetOutlines(meshPrimitive, normalTolerance: normalTolerance, distanceTolerance).ToArray();
        if (outlines.Length > 0) {
            meshPrimitive.SetCesiumOutline(outlines);
        }
    }
}
