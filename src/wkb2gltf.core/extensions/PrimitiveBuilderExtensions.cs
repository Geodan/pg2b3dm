using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;

namespace Wkb2Gltf.Extensions
{
    public static class PrimitiveBuilderExtentionMethods
    {
        public static (int, int, int) AddTriangleWithBatchId(this PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexWithBatchId, VertexEmpty> prim, (Vector3, Vector3, Vector3) triangle, Vector3 normal, int batchid)
        {
            var vertices = GetVertices(triangle, normal, batchid);
            var res = prim.AddTriangle(vertices[0], vertices[1], vertices[2]);
            return res;
        }

        private static List<VertexBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>> GetVertices((Vector3, Vector3, Vector3) triangle, Vector3 normal, int batchid)
        {
            var vb0 = GetVertexBuilder(triangle.Item1, normal, batchid);
            var vb1 = GetVertexBuilder(triangle.Item2, normal, batchid);
            var vb2 = GetVertexBuilder(triangle.Item3, normal, batchid);
            return new List<VertexBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>>() { vb0, vb1, vb2 };
        }

        private static VertexBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty> GetVertexBuilder(Vector3 position, Vector3 normal, int batchid)
        {
            var vp0 = new VertexPositionNormal(position, normal);
            var vb0 = new VertexBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>(vp0, batchid);
            return vb0;
        }
    }
}
