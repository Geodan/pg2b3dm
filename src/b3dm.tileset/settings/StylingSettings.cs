using SharpGLTF.Materials;

namespace B3dm.Tileset.settings;

public class StylingSettings
{
    public string DefaultColor { get; set; } = "#FFFFFF";

    public string DefaultMetallicRoughness { get; set; } = "#008000";

    public AlphaMode DefaultAlphaMode { get; set; } = AlphaMode.OPAQUE;

    public bool DoubleSided { get; set; } = true;

    public bool AddOutlines { get; set; } = false;
}