using System.Collections.Generic;
using Wkx;

namespace Wkb2Gltf
{
    public class GeometryRecord
    {
        public GeometryRecord(int batchId)
        {
            BatchId = batchId;
            Attributes = new string[0];
        }
        public string Id { get; set; }
        public Geometry Geometry { get; set; }

        public int BatchId { get; set; }

        public object[] Attributes { get; set; }

        public ShaderColors Shader { get; set; }

        public List<Triangle> GetTriangles()
        {
            var surface = (PolyhedralSurface)Geometry;
            var triangles = Triangulator.GetTriangles(surface, BatchId, Shader);
            return triangles;
        }

    }
}
