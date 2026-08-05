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

    // for creating glTF with featureId + texture coordinates
    public static (int, int, int) AddTriangleWithFeatureIdAndTexCoords(this PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexWithFeatureIdTexture, VertexEmpty> prim, (Vector3, Vector3, Vector3) triangle, Vector3 normal, int featureId, (Vector2, Vector2, Vector2) textureCoordinates)
    {
        var vertices = GetVerticesWithFeatureIdAndTexCoords(triangle, normal, featureId, textureCoordinates);
        var res = prim.AddTriangle(vertices[0], vertices[1], vertices[2]);
        return res;
    }

    // for creating b3dm with batchId + texture coordinates
    public static (int, int, int) AddTriangleWithBatchIdAndTexCoords(this PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexWithBatchIdTexture, VertexEmpty> prim, (Vector3, Vector3, Vector3) triangle, Vector3 normal, int batchid, (Vector2, Vector2, Vector2) textureCoordinates)
    {
        var vertices = GetVerticesWithBatchIdAndTexCoords(triangle, normal, batchid, textureCoordinates);
        var res = prim.AddTriangle(vertices[0], vertices[1], vertices[2]);
        return res;
    }

    // for creating glTF with featureId + surfaceId (--surfaces option)
    public static (int, int, int) AddTriangleWithFeatureIdAndSurfaceId(this PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexWithFeatureIdAndSurfaceId, VertexEmpty> prim, (Vector3, Vector3, Vector3) triangle, Vector3 normal, int featureId, int surfaceId)
    {
        var vertices = GetVerticesWithFeatureIdAndSurfaceId(triangle, normal, featureId, surfaceId);
        var res = prim.AddTriangle(vertices[0], vertices[1], vertices[2]);
        return res;
    }

    // for creating glTF with featureId + surfaceId + texture coordinates (--surfaces option + texture pipeline)
    public static (int, int, int) AddTriangleWithFeatureIdAndSurfaceIdAndTexCoords(this PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexWithFeatureIdAndSurfaceIdTexture, VertexEmpty> prim, (Vector3, Vector3, Vector3) triangle, Vector3 normal, int featureId, int surfaceId, (Vector2, Vector2, Vector2) textureCoordinates)
    {
        var vertices = GetVerticesWithFeatureIdAndSurfaceIdAndTexCoords(triangle, normal, featureId, surfaceId, textureCoordinates);
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

    private static List<VertexBuilder<VertexPositionNormal, VertexWithFeatureIdTexture, VertexEmpty>> GetVerticesWithFeatureIdAndTexCoords((Vector3, Vector3, Vector3) triangle, Vector3 normal, int featureId, (Vector2, Vector2, Vector2) textureCoordinates)
    {
        var vb0 = GetVertexBuilderWithFeatureIdAndTexCoords(triangle.Item1, normal, featureId, textureCoordinates.Item1);
        var vb1 = GetVertexBuilderWithFeatureIdAndTexCoords(triangle.Item2, normal, featureId, textureCoordinates.Item2);
        var vb2 = GetVertexBuilderWithFeatureIdAndTexCoords(triangle.Item3, normal, featureId, textureCoordinates.Item3);
        return new List<VertexBuilder<VertexPositionNormal, VertexWithFeatureIdTexture, VertexEmpty>>() { vb0, vb1, vb2 };
    }

    private static List<VertexBuilder<VertexPositionNormal, VertexWithBatchIdTexture, VertexEmpty>> GetVerticesWithBatchIdAndTexCoords((Vector3, Vector3, Vector3) triangle, Vector3 normal, int batchid, (Vector2, Vector2, Vector2) textureCoordinates)
    {
        var vb0 = GetVertexBuilderWithBatchIdAndTexCoords(triangle.Item1, normal, batchid, textureCoordinates.Item1);
        var vb1 = GetVertexBuilderWithBatchIdAndTexCoords(triangle.Item2, normal, batchid, textureCoordinates.Item2);
        var vb2 = GetVertexBuilderWithBatchIdAndTexCoords(triangle.Item3, normal, batchid, textureCoordinates.Item3);
        return new List<VertexBuilder<VertexPositionNormal, VertexWithBatchIdTexture, VertexEmpty>>() { vb0, vb1, vb2 };
    }

    private static VertexBuilder<VertexPositionNormal, VertexWithFeatureIdTexture, VertexEmpty> GetVertexBuilderWithFeatureIdAndTexCoords(Vector3 position, Vector3 normal, int featureId, Vector2 textureCoordinate)
    {
        var vp0 = new VertexPositionNormal(position, normal);
        var vt0 = new VertexWithFeatureIdTexture(featureId, textureCoordinate);
        var vb0 = new VertexBuilder<VertexPositionNormal, VertexWithFeatureIdTexture, VertexEmpty>(vp0, vt0);
        return vb0;
    }

    private static VertexBuilder<VertexPositionNormal, VertexWithBatchIdTexture, VertexEmpty> GetVertexBuilderWithBatchIdAndTexCoords(Vector3 position, Vector3 normal, int batchid, Vector2 textureCoordinate)
    {
        var vp0 = new VertexPositionNormal(position, normal);
        var vt0 = new VertexWithBatchIdTexture(batchid, textureCoordinate);
        var vb0 = new VertexBuilder<VertexPositionNormal, VertexWithBatchIdTexture, VertexEmpty>(vp0, vt0);
        return vb0;
    }

    private static List<VertexBuilder<VertexPositionNormal, VertexWithFeatureIdAndSurfaceId, VertexEmpty>> GetVerticesWithFeatureIdAndSurfaceId((Vector3, Vector3, Vector3) triangle, Vector3 normal, int featureId, int surfaceId)
    {
        var vb0 = GetVertexBuilderWithFeatureIdAndSurfaceId(triangle.Item1, normal, featureId, surfaceId);
        var vb1 = GetVertexBuilderWithFeatureIdAndSurfaceId(triangle.Item2, normal, featureId, surfaceId);
        var vb2 = GetVertexBuilderWithFeatureIdAndSurfaceId(triangle.Item3, normal, featureId, surfaceId);
        return new List<VertexBuilder<VertexPositionNormal, VertexWithFeatureIdAndSurfaceId, VertexEmpty>>() { vb0, vb1, vb2 };
    }

    private static VertexBuilder<VertexPositionNormal, VertexWithFeatureIdAndSurfaceId, VertexEmpty> GetVertexBuilderWithFeatureIdAndSurfaceId(Vector3 position, Vector3 normal, int featureId, int surfaceId)
    {
        var vp0 = new VertexPositionNormal(position, normal);
        var vt0 = new VertexWithFeatureIdAndSurfaceId(featureId, surfaceId);
        var vb0 = new VertexBuilder<VertexPositionNormal, VertexWithFeatureIdAndSurfaceId, VertexEmpty>(vp0, vt0);
        return vb0;
    }

    private static List<VertexBuilder<VertexPositionNormal, VertexWithFeatureIdAndSurfaceIdTexture, VertexEmpty>> GetVerticesWithFeatureIdAndSurfaceIdAndTexCoords((Vector3, Vector3, Vector3) triangle, Vector3 normal, int featureId, int surfaceId, (Vector2, Vector2, Vector2) textureCoordinates)
    {
        var vb0 = GetVertexBuilderWithFeatureIdAndSurfaceIdAndTexCoords(triangle.Item1, normal, featureId, surfaceId, textureCoordinates.Item1);
        var vb1 = GetVertexBuilderWithFeatureIdAndSurfaceIdAndTexCoords(triangle.Item2, normal, featureId, surfaceId, textureCoordinates.Item2);
        var vb2 = GetVertexBuilderWithFeatureIdAndSurfaceIdAndTexCoords(triangle.Item3, normal, featureId, surfaceId, textureCoordinates.Item3);
        return new List<VertexBuilder<VertexPositionNormal, VertexWithFeatureIdAndSurfaceIdTexture, VertexEmpty>>() { vb0, vb1, vb2 };
    }

    private static VertexBuilder<VertexPositionNormal, VertexWithFeatureIdAndSurfaceIdTexture, VertexEmpty> GetVertexBuilderWithFeatureIdAndSurfaceIdAndTexCoords(Vector3 position, Vector3 normal, int featureId, int surfaceId, Vector2 textureCoordinate)
    {
        var vp0 = new VertexPositionNormal(position, normal);
        var vt0 = new VertexWithFeatureIdAndSurfaceIdTexture(featureId, surfaceId, textureCoordinate);
        var vb0 = new VertexBuilder<VertexPositionNormal, VertexWithFeatureIdAndSurfaceIdTexture, VertexEmpty>(vp0, vt0);
        return vb0;
    }
}
