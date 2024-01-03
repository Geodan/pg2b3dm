using System.Collections.Generic;
using Wkx;

namespace Wkb2Gltf;

public class GeometryRecord
{
    public GeometryRecord(int batchId)
    {
        BatchId = batchId;
        Attributes = new Dictionary<string, object>();
    }
    public Geometry Geometry { get; set; }

    public int BatchId { get; set; }

    public Dictionary<string,object> Attributes { get; set; }

    public ShaderColors Shader { get; set; }

    public List<Triangle> GetTriangles(double areaTolerance=0.01, Point center = null)
    {
        var triangles = GeometryProcessor.GetTriangles(Geometry, BatchId, Shader, areaTolerance, center);

        return triangles;
    }

}
