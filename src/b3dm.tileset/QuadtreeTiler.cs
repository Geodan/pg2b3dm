using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using B3dm.Tileset.settings;
using Npgsql;
using subtree;
using Wkx;

namespace pg2b3dm;

public class QuadtreeTiler
{
    private readonly NpgsqlConnection conn;
    private readonly int source_epsg;
    private readonly int maxFeaturesPerTile;
    private readonly double[] translation;
    private readonly string outputFolder;
    private readonly List<int> lods;
    private readonly string copyright;
    private readonly bool skipCreateTiles;
    private readonly StylingSettings stylingSettings;
    private InputTable inputTable;

    public QuadtreeTiler(string connectionString, InputTable inputTable, StylingSettings stylingSettings, int maxFeaturesPerTile, double[] translation, string outputFolder, List<int> lods, string copyright = "", bool skipCreateTiles = false)
    {
        this.conn = new NpgsqlConnection(connectionString);
        this.inputTable = inputTable;
        this.source_epsg = inputTable.EPSGCode;
        this.maxFeaturesPerTile = maxFeaturesPerTile;
        this.translation = translation;
        this.outputFolder = outputFolder;
        this.lods = lods;
        this.copyright = copyright;
        this.skipCreateTiles = skipCreateTiles;
        this.stylingSettings = stylingSettings;
    }

    public List<Tile> GenerateTiles(BoundingBox bbox, Tile tile, List<Tile> tiles, int lod = 0, bool createGltf = false, bool keepProjection = false)
    {
        var where = inputTable.GetQueryClause();

        var lodquery = LodQuery.GetLodQuery(inputTable.LodColumn, lod);

        if (inputTable.LodColumn != String.Empty) {
            where += $" and {lodquery}";
        }

        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, inputTable.TableName, inputTable.GeometryColumn, new Point(bbox.XMin, bbox.YMin), new Point(bbox.XMax, bbox.YMax), where, source_epsg, keepProjection);

        if (numberOfFeatures == 0) {
            tile.Available = false;
            tiles.Add(tile);
        }
        else if (numberOfFeatures > maxFeaturesPerTile) {
            tile.Available = false;
            tiles.Add(tile);

            var z = tile.Z + 1;

            // split in quadtree
            for (var x = 0; x < 2; x++) {
                for (var y = 0; y < 2; y++) {
                    var dx = (bbox.XMax - bbox.XMin) / 2;
                    var dy = (bbox.YMax - bbox.YMin) / 2;

                    var xstart = bbox.XMin + dx * x;
                    var ystart = bbox.YMin + dy * y;
                    var xend = xstart + dx;
                    var yend = ystart + dy;

                    var bboxQuad = new BoundingBox(xstart, ystart, xend, yend);
                    var new_tile = new Tile(z, tile.X * 2 + x, tile.Y * 2 + y);
                    new_tile.BoundingBox = bboxQuad.ToArray();
                    GenerateTiles(bboxQuad, new_tile, tiles, lod, createGltf, keepProjection);
                }
            }
        }
        else {

            var file = $"{tile.Z}_{tile.X}_{tile.Y}";
            if (inputTable.LodColumn != String.Empty) {
                file += $"_{lod}";
            }

            var ext = createGltf ? ".glb" : ".b3dm";
            file += ext;
            Console.Write($"\rCreating tile: {file}  ");
            tile.ContentUri = file;

            int target_srs = 4978;

            if(keepProjection) {
                target_srs = source_epsg;
            }

            byte[] bytes = null;

            var geometries = GeometryRepository.GetGeometrySubset(conn, inputTable.TableName, inputTable.GeometryColumn, tile.BoundingBox, source_epsg, target_srs, inputTable.ShadersColumn, inputTable.AttributeColumns, where, inputTable.RadiusColumn, keepProjection);
            // var scale = new double[] { 1, 1, 1 };
            if (geometries.Count > 0) {

                tile.Lod = lod;

                if (!skipCreateTiles) {
                    bytes = TileWriter.ToTile(geometries, translation, copyright: copyright, addOutlines: stylingSettings.AddOutlines, defaultColor: stylingSettings.DefaultColor, defaultMetallicRoughness: stylingSettings.DefaultMetallicRoughness, doubleSided: stylingSettings.DoubleSided, defaultAlphaMode: stylingSettings.DefaultAlphaMode, defaultAlphaCutoff: stylingSettings.DefaultAlphaCutoff, createGltf: createGltf);
                    File.WriteAllBytes($"{outputFolder}{Path.AltDirectorySeparatorChar}{file}", bytes);
                }
                if (inputTable.LodColumn != String.Empty) {
                    if (lod < lods.Max()) {
                        // take the next lod
                        var currentIndex = lods.FindIndex(p => p == lod);
                        var nextIndex = currentIndex + 1;
                        var nextLod = lods[nextIndex];
                        // make a copy of the tile 
                        var t2 = new Tile(tile.Z, tile.X, tile.Y);
                        t2.BoundingBox = tile.BoundingBox;
                        var lodNextTiles = GenerateTiles(bbox, t2, new List<Tile>(), nextLod, createGltf, keepProjection);
                        tile.Children = lodNextTiles;
                    };
                }

                // next code is used to fix geometries that have centroid in the tile, but some parts outside...
                var bbox_geometries = GeometryRepository.GetGeometriesBoundingBox(conn, inputTable.TableName, inputTable.GeometryColumn, source_epsg, tile, where, keepProjection);
                var bbox_tile = new double[] { bbox_geometries[0], bbox_geometries[1], bbox_geometries[2], bbox_geometries[3] };
                tile.BoundingBox = bbox_tile;
                tile.ZMin = bbox_geometries[4];
                tile.ZMax = bbox_geometries[5];

                tile.Available = true;

                if (skipCreateTiles) { tile.Available = true; }
            }
            else {
                tile.Available = false;
            }
            tiles.Add(tile);
        }

        return tiles;
    }
}
