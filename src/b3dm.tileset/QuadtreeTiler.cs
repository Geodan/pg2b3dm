using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using Npgsql;
using subtree;
using Wkx;

namespace pg2b3dm;

public class QuadtreeTiler
{
    private readonly string table;
    private readonly NpgsqlConnection conn;
    private readonly int epsg;
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

    public QuadtreeTiler(NpgsqlConnection conn, string table, int epsg, string geometryColumn, int maxFeaturesPerTile, string query, double[] translation, string colorColumn, string attributesColumn, string lodColumn, string outputFolder, List<int> lods, string copyright = "", bool skipCreateTiles = false)
    {
        this.table = table;
        this.conn = conn;
        this.epsg = epsg;
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
    }

    public List<Tile> GenerateTiles(BoundingBox bbox, Tile tile, List<Tile> tiles, int lod = 0, bool addOutlines = false, double areaTolerance = 0.01, string defaultColor = "#FFFFFF", string defaultMetallicRoughness = "#008000", bool doubleSided = true, bool createGltf = false)
    {
        var where = (query != string.Empty ? $" and {query}" : String.Empty);

        var lodquery = LodQuery.GetLodQuery(lodColumn, lod);

        if (lodColumn != String.Empty) {
            where += $" and {lodquery}";
        }

        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, table, geometryColumn, new Point(bbox.XMin, bbox.YMin), new Point(bbox.XMax, bbox.YMax), epsg, where);

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
                    GenerateTiles(bboxQuad, new_tile, tiles, lod, addOutlines, areaTolerance, defaultColor, defaultMetallicRoughness, doubleSided, createGltf);
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

            if (!skipCreateTiles) {

                var geometries = GeometryRepository.GetGeometrySubset(conn, table, geometryColumn, translation, tile.BoundingBox, epsg, colorColumn, attributesColumn, where);
                var bytes = TileWriter.ToTile(geometries, copyright, addOutlines, areaTolerance, defaultColor, defaultMetallicRoughness, doubleSided, createGltf);
                tile.Lod = lod;

                File.WriteAllBytes($"{outputFolder}{Path.AltDirectorySeparatorChar}{file}", bytes);

                if (lodColumn != String.Empty) {
                    if (lod < lods.Max()) {
                        // take the next lod
                        var currentIndex = lods.FindIndex(p => p == lod);
                        var nextIndex = currentIndex + 1;
                        var nextLod = lods[nextIndex];
                        // make a copy of the tile 
                        var t2 = new Tile(tile.X, tile.Y, tile.Z);
                        t2.BoundingBox = tile.BoundingBox;
                        var lodNextTiles = GenerateTiles(bbox, t2, new List<Tile>(), nextLod, addOutlines, areaTolerance, defaultColor, defaultMetallicRoughness, doubleSided, createGltf);
                        tile.Children = lodNextTiles;
                    };
                }

                // next code is used to fix geometries that have centroid in the tile, but some parts outside...
                var bbox_geometries = GeometryRepository.GetGeometriesBoundingBox(conn, table, geometryColumn, tile, where);
                tile.BoundingBox = bbox_geometries;

            }

            tile.Available = true;
            tiles.Add(tile);
        }

        return tiles;
    }
}
