using System;

namespace Wkb2Gltf;

public class GeometryTexture
{
    public string TextureMapping { get; set; } = string.Empty;

    public byte[] TextureImageData { get; set; } = Array.Empty<byte>();

    public string TextureMimeType { get; set; } = string.Empty;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(TextureMapping) && TextureImageData.Length > 0;
    }
}
