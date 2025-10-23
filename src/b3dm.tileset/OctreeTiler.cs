using System.Collections.Generic;
using B3dm.Tileset.settings;
using Npgsql;
using subtree;
using Wkx;

namespace B3dm.Tileset;

public class OctreeTiler
{
    private readonly NpgsqlConnection conn;
    private readonly TilingSettings tilingSettings;
    private readonly StylingSettings stylingSettings;
    private readonly TilesetSettings tilesetSettings;
    private readonly InputTable inputTable;

    public OctreeTiler(NpgsqlConnection conn, InputTable inputTable, TilingSettings tilingSetttings, StylingSettings stylingSettings, TilesetSettings tilesetSettings)
    {
        this.conn = conn;
        this.inputTable = inputTable;
        this.tilingSettings = tilingSetttings;
        this.stylingSettings = stylingSettings;
        this.tilesetSettings = tilesetSettings;
    }

    public List<Tile3D> GenerateTiles3D(BoundingBox3D bbox, int Level, Tile3D tile, List<Tile3D> tiles)
    {
        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, inputTable.TableName, inputTable.GeometryColumn, new Point(bbox.XMin, bbox.YMin, bbox.ZMin), new Point(bbox.XMax, bbox.YMax, bbox.ZMax), inputTable.Query, inputTable.EPSGCode, tilingSettings.KeepProjection);
        return null;

    }
}
