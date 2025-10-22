using System;

namespace B3dm.Tileset.settings;

public class TilesetSettings
{
    public OutputSettings OutputSettings { get; set; } = new OutputSettings();

    public Version Version { get; set; }

    public string Copyright { get; set; } = string.Empty;

    public string TilesetVersion { get; set; } = "1.1";

    public double[] Translation { get; set; }

    public SubdivisionScheme SubdivisionScheme { get; set;  } = SubdivisionScheme.QUADTREE;

    public double GeometricError { get; set; }

    public double GeometricErrorFactor { get; set; } = 2.0;

    public RefinementType Refinement { get; set; } = RefinementType.ADD;

    public double[] RootBoundingVolumeRegion { get; set; }
    
    public string Crs { get; set; } = string.Empty; 
}
