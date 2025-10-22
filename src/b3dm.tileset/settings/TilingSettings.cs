using System.Collections.Generic;

namespace B3dm.Tileset.settings;

public class TilingSettings
{
    public bool CreateGltf { get; set; } = true;

    public bool KeepProjection { get; set; } = false;

    public bool SkipCreateTiles { get; set; } = false;

    public int MaxFeaturesPerTile { get; set; } = 1000; 

    public bool UseImplicitTiling { get; set; } = true;

    public List<int> Lods { get; set; }
}
