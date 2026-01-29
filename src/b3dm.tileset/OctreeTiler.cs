using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B3dm.Tileset.settings;
using Npgsql;
using pg2b3dm;
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

    public OctreeTiler(string connectionString, InputTable inputTable, TilingSettings tilingSetttings, StylingSettings stylingSettings, TilesetSettings tilesetSettings)
    {
        this.conn = new NpgsqlConnection(connectionString);
        this.inputTable = inputTable;
        this.tilingSettings = tilingSetttings;
        this.stylingSettings = stylingSettings;
        this.tilesetSettings = tilesetSettings;
    }

    public List<Tile3D> GenerateTiles3D(BoundingBox3D bbox, int level, Tile3D tile, List<Tile3D> tiles)
    {
        return GenerateTiles3D(bbox, level, tile, tiles, null, null);
    }

    public List<Tile3D> GenerateTiles3D(BoundingBox3D bbox, int level, Tile3D tile, List<Tile3D> tiles, Dictionary<string, BoundingBox3D> tileBounds)
    {
        return GenerateTiles3D(bbox, level, tile, tiles, tileBounds, null);
    }

    public List<Tile3D> GenerateTiles3D(BoundingBox3D bbox, int level, Tile3D tile, List<Tile3D> tiles, Dictionary<string, BoundingBox3D> tileBounds, HashSet<string> processedGeometries)
    {
        if (processedGeometries == null) {
            processedGeometries = new HashSet<string>();
        }

        var where = inputTable.GetQueryClause();

        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, inputTable.TableName, inputTable.GeometryColumn, new Point(bbox.XMin, bbox.YMin, bbox.ZMin), new Point(bbox.XMax, bbox.YMax, bbox.ZMax), where, inputTable.EPSGCode, tilingSettings.KeepProjection, processedGeometries);
        if (numberOfFeatures == 0) {
            var t2 = new Tile3D(level, tile.X, tile.Y, tile.Z);
            t2.Available = false;
            tiles.Add(t2);
            if (tileBounds != null) {
                var key = $"{level}_{tile.Z}_{tile.X}_{tile.Y}";
                tileBounds[key] = bbox;
            }
        }
        else if (numberOfFeatures > tilingSettings.MaxFeaturesPerTile) {
            // First, create a tile with the largest geometries up to MaxFeaturesPerTile for this level
            CreateTileForLargestGeometries3D(bbox, level, tile, tiles, tileBounds, where, processedGeometries);

            level++;
            for (var x = 0; x < 2; x++) {
                for (var y = 0; y < 2; y++) {
                    var dx = (bbox.XMax - bbox.XMin) / 2;
                    var dy = (bbox.YMax - bbox.YMin) / 2;

                    var xstart = bbox.XMin + dx * x;
                    var ystart = bbox.YMin + dy * y;
                    var xend = xstart + dx;
                    var yend = ystart + dy;


                    for (var z = 0; z < 2; z++) {
                        var dz = (bbox.ZMax - bbox.ZMin) / 2;
                        var z_start = bbox.ZMin + dz * z;
                        var zend = z_start + dz;
                        var bbox3d = new BoundingBox3D(xstart, ystart, z_start, xend, yend, zend);

                        var new_tile = new Tile3D(level, tile.X * 2 + x, tile.Y * 2 + y, tile.Z * 2 + z);
                        GenerateTiles3D(bbox3d, level, new_tile, tiles, tileBounds, processedGeometries);
                    }
                }
            }
        }
        else {
            CreateTile3D(bbox, level, tile, tiles, tileBounds, where, processedGeometries);
        }

        return tiles;

    }

    private void CreateTileForLargestGeometries3D(BoundingBox3D bbox, int level, Tile3D tile, List<Tile3D> tiles, Dictionary<string, BoundingBox3D> tileBounds, string where, HashSet<string> processedGeometries)
    {
        // Get the largest geometries (up to MaxFeaturesPerTile) for this tile at this level
        int target_srs = tilingSettings.KeepProjection ? inputTable.EPSGCode : 4978;

        var bbox1 = new double[] { bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax, bbox.ZMin, bbox.ZMax };
        var geometriesToProcess = GeometryRepository.GetGeometrySubset(conn, inputTable.TableName, inputTable.GeometryColumn, bbox1, inputTable.EPSGCode, target_srs, inputTable.ShadersColumn, inputTable.AttributeColumns, where, inputTable.RadiusColumn, tilingSettings.KeepProjection, processedGeometries, tilingSettings.MaxFeaturesPerTile);

        if (geometriesToProcess.Count > 0) {
            // Collect hashes of processed geometries
            foreach (var geom in geometriesToProcess) {
                if (!string.IsNullOrEmpty(geom.Hash)) {
                    processedGeometries.Add(geom.Hash);
                }
            }

            if (!tilingSettings.SkipCreateTiles) {
                var bytes = TileWriter.ToTile(geometriesToProcess, tilesetSettings.Translation, copyright: tilesetSettings.Copyright, addOutlines: stylingSettings.AddOutlines, defaultColor: stylingSettings.DefaultColor, defaultMetallicRoughness: stylingSettings.DefaultMetallicRoughness, doubleSided: stylingSettings.DoubleSided, defaultAlphaMode: stylingSettings.DefaultAlphaMode, alphaCutoff: stylingSettings.AlphaCutoff, createGltf: tilingSettings.CreateGltf);
                var file = $"{tilesetSettings.OutputSettings.ContentFolder}{Path.AltDirectorySeparatorChar}{tile.Level}_{tile.Z}_{tile.X}_{tile.Y}.glb";
                Console.Write($"\rCreating tile: {file}  ");
                File.WriteAllBytes($"{file}", bytes);
            }
            tile.Available = true;
        }

        tiles.Add(tile);
        if (tileBounds != null) {
            var key = $"{tile.Level}_{tile.Z}_{tile.X}_{tile.Y}";
            tileBounds[key] = bbox;
        }
    }

    private void CreateTile3D(BoundingBox3D bbox, int level, Tile3D tile, List<Tile3D> tiles, Dictionary<string, BoundingBox3D> tileBounds, string where, HashSet<string> processedGeometries)
    {
        int target_srs = tilingSettings.KeepProjection ? inputTable.EPSGCode : 4978;

        var bbox1 = new double[] { bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax, bbox.ZMin, bbox.ZMax };
        var geometries = GeometryRepository.GetGeometrySubset(conn, inputTable.TableName, inputTable.GeometryColumn, bbox1, inputTable.EPSGCode, target_srs, inputTable.ShadersColumn, inputTable.AttributeColumns, where, inputTable.RadiusColumn, tilingSettings.KeepProjection, processedGeometries);

        if (geometries.Count > 0) {
            // Collect hashes of processed geometries
            foreach (var geom in geometries) {
                if (!string.IsNullOrEmpty(geom.Hash)) {
                    processedGeometries.Add(geom.Hash);
                }
            }

            if (!tilingSettings.SkipCreateTiles) {
                var bytes = TileWriter.ToTile(geometries, tilesetSettings.Translation, copyright: tilesetSettings.Copyright, addOutlines: stylingSettings.AddOutlines, defaultColor: stylingSettings.DefaultColor, defaultMetallicRoughness: stylingSettings.DefaultMetallicRoughness, doubleSided: stylingSettings.DoubleSided, defaultAlphaMode: stylingSettings.DefaultAlphaMode, alphaCutoff: stylingSettings.AlphaCutoff, createGltf: tilingSettings.CreateGltf);
                var file = $"{tilesetSettings.OutputSettings.ContentFolder}{Path.AltDirectorySeparatorChar}{tile.Level}_{tile.Z}_{tile.X}_{tile.Y}.glb";
                Console.Write($"\rCreating tile: {file}  ");
                File.WriteAllBytes($"{file}", bytes);
            }
            tile.Available = true;
        }
        else {
            tile.Available = false;
        }

        tiles.Add(tile);
        if (tileBounds != null) {
            var key = $"{tile.Level}_{tile.Z}_{tile.X}_{tile.Y}";
            tileBounds[key] = bbox;
        }
    }
}
