using System.Collections.Generic;

namespace B3dm.Tileset;

public class TilingSettings
{
    public Wkx.BoundingBox BoundingBox { get; set; } 

    public bool CreateGltf { get; set; } = true;

    public bool KeepProjection { get; set; } = false;

    public bool SkipCreateTiles { get; set; } = false;

    public int MaxFeaturesPerTile { get; set; } = 1000; 

    public SortBy SortBy { get; set; } = SortBy.AREA;

    public bool UseImplicitTiling { get; set; } = true;

    public List<int> Lods { get; set; }
}
