using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using Npgsql;
using SharpGLTF.Materials;
using subtree;
using Wkx;

namespace pg2b3dm;

public class QuadtreeTiler
{
    private readonly string table;
    private readonly NpgsqlConnection conn;
    private readonly int source_epsg;
    private readonly string geometryColumn;
    private readonly int maxFeaturesPerTile;
    private readonly string query;
    private readonly double[] translation;
    private readonly string colorColumn;
    private readonly string attributesColumn;
    private readonly string lodColumn;
    private readonly string outputFolder;
    private readonly List<int> lods;
    private readonly string copyright;
    private readonly bool skipCreateTiles;
    private readonly string radiusColumn;

    public QuadtreeTiler(NpgsqlConnection conn, string table, int source_epsg, string geometryColumn, int maxFeaturesPerTile, string query, double[] translation, string colorColumn, string attributesColumn, string lodColumn, string outputFolder, List<int> lods, string copyright = "", bool skipCreateTiles = false, string radiusColumn = "")
    {
        this.table = table;
        this.conn = conn;
        this.source_epsg = source_epsg;
        this.geometryColumn = geometryColumn;
        this.maxFeaturesPerTile = maxFeaturesPerTile;
        this.query = query;
        this.translation = translation;
        this.colorColumn = colorColumn;
        this.attributesColumn = attributesColumn;
        this.lodColumn = lodColumn;
        this.outputFolder = outputFolder;
        this.lods = lods;
        this.copyright = copyright;
        this.skipCreateTiles = skipCreateTiles;
        this.radiusColumn = radiusColumn;
    }

    public List<Tile> GenerateTiles(BoundingBox bbox, Tile tile, List<Tile> tiles, int lod = 0, bool addOutlines = false, string defaultColor = "#FFFFFF", string defaultMetallicRoughness = "#008000", bool doubleSided = true, AlphaMode defaultAlphaMode = AlphaMode.OPAQUE, bool createGltf = false, bool keepProjection = false)
    {
        var where = (query != string.Empty ? $" and {query}" : String.Empty);

        var lodquery = LodQuery.GetLodQuery(lodColumn, lod);

        if (lodColumn != String.Empty) {
            where += $" and {lodquery}";
        }

        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, table, geometryColumn, new Point(bbox.XMin, bbox.YMin), new Point(bbox.XMax, bbox.YMax), where, source_epsg);

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
                    GenerateTiles(bboxQuad, new_tile, tiles, lod, addOutlines, defaultColor, defaultMetallicRoughness, doubleSided, defaultAlphaMode, createGltf, keepProjection);
                }
            }
        }
        else {

            var file = $"{tile.Z}_{tile.X}_{tile.Y}";
            if (lodColumn != String.Empty) {
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

            var geometries = GeometryRepository.GetGeometrySubset(conn, table, geometryColumn, tile.BoundingBox, source_epsg, target_srs, colorColumn, attributesColumn, where, radiusColumn);
            // var scale = new double[] { 1, 1, 1 };
            if (geometries.Count > 0) {

                tile.Lod = lod;

                if (!skipCreateTiles) {
                    bytes = TileWriter.ToTile(geometries, translation, copyright: copyright, addOutlines: addOutlines, defaultColor: defaultColor, defaultMetallicRoughness: defaultMetallicRoughness, doubleSided: doubleSided, defaultAlphaMode: defaultAlphaMode, createGltf: createGltf);
                    File.WriteAllBytes($"{outputFolder}{Path.AltDirectorySeparatorChar}{file}", bytes);
                }
                if (lodColumn != String.Empty) {
                    if (lod < lods.Max()) {
                        // take the next lod
                        var currentIndex = lods.FindIndex(p => p == lod);
                        var nextIndex = currentIndex + 1;
                        var nextLod = lods[nextIndex];
                        // make a copy of the tile 
                        var t2 = new Tile(tile.Z, tile.X, tile.Y);
                        t2.BoundingBox = tile.BoundingBox;
                        var lodNextTiles = GenerateTiles(bbox, t2, new List<Tile>(), nextLod, addOutlines, defaultColor, defaultMetallicRoughness, doubleSided, defaultAlphaMode, createGltf, keepProjection);
                        tile.Children = lodNextTiles;
                    };
                }

                // next code is used to fix geometries that have centroid in the tile, but some parts outside...
                var bbox_geometries = GeometryRepository.GetGeometriesBoundingBox(conn, table, geometryColumn, source_epsg, tile, where);
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
