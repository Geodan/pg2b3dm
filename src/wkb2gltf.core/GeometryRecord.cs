using System;
using System.Collections.Generic;
using System.Linq;
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

    public float? Radius { get; set; }

    public long? SourceId { get; set; }

    public string TextureMapping { get; set; } = string.Empty;

    public string GeometryProperties { get; set; } = string.Empty;

    public byte[] TextureImageData { get; set; } = Array.Empty<byte>();

    public string TextureMimeType { get; set; } = string.Empty;

    public List<GeometryTexture> Textures { get; set; } = [];

    public bool HasTextureData()
    {
        return Textures.Any(texture => texture.IsValid()) || (!string.IsNullOrWhiteSpace(TextureMapping) && TextureImageData.Length > 0);
    }

    public List<Triangle> GetTriangles(double[] translation = null, double[] scale = null)
    {
        var textures = new List<GeometryTexture>();
        if (Textures.Count > 0) {
            textures.AddRange(Textures);
        }
        else if (!string.IsNullOrWhiteSpace(TextureMapping) && TextureImageData.Length > 0) {
            textures.Add(new GeometryTexture() {
                TextureMapping = TextureMapping,
                TextureImageData = TextureImageData,
                TextureMimeType = TextureMimeType
            });
        }

        var triangles = GeometryProcessor.GetTriangles(Geometry, BatchId, translation, scale, Shader, Radius, TextureMapping, GeometryProperties, TextureImageData, TextureMimeType, textures);

        return triangles;
    }


}
