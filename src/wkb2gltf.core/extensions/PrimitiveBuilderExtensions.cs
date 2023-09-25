using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;

namespace Wkb2Gltf.Extensions;

public static class PrimitiveBuilderExtentionMethods
{

    // for creating glTF with featureId
    public static (int, int, int) AddTriangleWithFeatureId(this PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexWithFeatureId, VertexEmpty> prim, (Vector3, Vector3, Vector3) triangle, Vector3 normal, int featureId)
    {
        var vertices = GetVerticesWithFeatureId(triangle, normal, featureId);
        var res = prim.AddTriangle(vertices[0], vertices[1], vertices[2]);
        return res;
    }

    // for creating b3dm with batchId
    public static (int, int, int) AddTriangleWithBatchId(this PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexWithBatchId, VertexEmpty> prim, (Vector3, Vector3, Vector3) triangle, Vector3 normal, int batchid)
    {
        var vertices = GetVerticesWithBatchId(triangle, normal, batchid);
        var res = prim.AddTriangle(vertices[0], vertices[1], vertices[2]);
        return res;
    }

    private static List<VertexBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>> GetVerticesWithBatchId((Vector3, Vector3, Vector3) triangle, Vector3 normal, int batchid)
    {
        var vb0 = GetVertexBuilderWithBatchId(triangle.Item1, normal, batchid);
        var vb1 = GetVertexBuilderWithBatchId(triangle.Item2, normal, batchid);
        var vb2 = GetVertexBuilderWithBatchId(triangle.Item3, normal, batchid);
        return new List<VertexBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>>() { vb0, vb1, vb2 };
    }

    private static VertexBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty> GetVertexBuilderWithBatchId(Vector3 position, Vector3 normal, int batchid)
    {
        var vp0 = new VertexPositionNormal(position, normal);
        var vb0 = new VertexBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>(vp0, batchid);
        return vb0;
    }

    private static List<VertexBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty>> GetVerticesWithFeatureId((Vector3, Vector3, Vector3) triangle, Vector3 normal, int featureid)
    {
        var vb0 = GetVertexBuilderWithFeatureId(triangle.Item1, normal, featureid);
        var vb1 = GetVertexBuilderWithFeatureId(triangle.Item2, normal, featureid);
        var vb2 = GetVertexBuilderWithFeatureId(triangle.Item3, normal, featureid);
        return new List<VertexBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty>>() { vb0, vb1, vb2 };
    }

    private static VertexBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty> GetVertexBuilderWithFeatureId(Vector3 position, Vector3 normal, int featureid)
    {
        var vp0 = new VertexPositionNormal(position, normal);
        var vb0 = new VertexBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty>(vp0, featureid);
        return vb0;
    }
}
