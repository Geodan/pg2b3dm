using System;
using System.Collections.Generic;
using System.IO;
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

    public OctreeTiler(NpgsqlConnection conn, InputTable inputTable, TilingSettings tilingSetttings, StylingSettings stylingSettings, TilesetSettings tilesetSettings)
    {
        this.conn = conn;
        this.inputTable = inputTable;
        this.tilingSettings = tilingSetttings;
        this.stylingSettings = stylingSettings;
        this.tilesetSettings = tilesetSettings;
    }

    public List<Tile3D> GenerateTiles3D(BoundingBox3D bbox, int level, Tile3D tile, List<Tile3D> tiles)
    {
        return GenerateTiles3D(bbox, level, tile, tiles, null);
    }

    public List<Tile3D> GenerateTiles3D(BoundingBox3D bbox, int level, Tile3D tile, List<Tile3D> tiles, Dictionary<string, BoundingBox3D> tileBounds)
    {
        var where = inputTable.GetQueryClause();

        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, inputTable.TableName, inputTable.GeometryColumn, new Point(bbox.XMin, bbox.YMin, bbox.ZMin), new Point(bbox.XMax, bbox.YMax, bbox.ZMax), where, inputTable.EPSGCode, tilingSettings.KeepProjection);
        if (numberOfFeatures == 0) {
            var t2 = new Tile3D(level, tile.Z, tile.X, tile.Y);
            t2.Available = false;
            tiles.Add(t2);
            if (tileBounds != null) {
                var key = $"{level}_{tile.Z}_{tile.X}_{tile.Y}";
                tileBounds[key] = bbox;
            }
        }
        else if (numberOfFeatures > tilingSettings.MaxFeaturesPerTile) {
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

                        var new_tile = new Tile3D(level, tile.X * 2 + x, tile.Y * 2 + y, tile.Z + z);
                        GenerateTiles3D(bbox3d, level, new_tile, tiles, tileBounds);
                    }
                }
            }
        }
        else {
            var boundingBox = new BoundingBox(bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax);

            int target_srs = 4978;

            if (tilingSettings.KeepProjection) {
                target_srs = inputTable.EPSGCode;
            }

            var bbox1 = new double[] { bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax, bbox.ZMin, bbox.ZMax };
            var geometries = GeometryRepository.GetGeometrySubset(conn, inputTable.TableName, inputTable.GeometryColumn, bbox1, inputTable.EPSGCode, target_srs, inputTable.ShadersColumn, inputTable.AttributeColumns, where, inputTable.RadiusColumn, tilingSettings.KeepProjection);

            if (geometries.Count > 0) {

                if (!tilingSettings.SkipCreateTiles) {
                    var bytes = TileWriter.ToTile(geometries, tilesetSettings.Translation, copyright: tilesetSettings.Copyright, addOutlines: stylingSettings.AddOutlines, defaultColor: stylingSettings.DefaultColor, defaultMetallicRoughness: stylingSettings.DefaultMetallicRoughness, doubleSided: stylingSettings.DoubleSided, defaultAlphaMode: stylingSettings.DefaultAlphaMode, createGltf: tilingSettings.CreateGltf);
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

        return tiles;

    }
}
