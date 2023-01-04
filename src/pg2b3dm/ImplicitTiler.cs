using System;
using System.Collections.Generic;
using System.IO;
using B3dm.Tileset;
using Npgsql;
using Wkx;

namespace pg2b3dm;

public class ImplicitTiler
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

    public ImplicitTiler(string table, NpgsqlConnection conn, int epsg, string geometryColumn, int maxFeaturesPerTile, string query, double[] translation, string colorColumn, string attributesColumn, string lodColumn, string outputFolder, List<int> lods, string copyright = "", bool skipCreateTiles = false)
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

    public List<Tile> GenerateTiles(BoundingBox bbox, Tile tile, List<Tile> tiles)
    {
        var where = (query != string.Empty ? $" and {query}" : String.Empty);

        if (lods.Count > 1) {
            where += $" and {lodColumn}={lods[0]}";
        }
        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, table, geometryColumn, new Point(bbox.XMin, bbox.YMin), new Point(bbox.XMax, bbox.YMax), epsg, where);

        var t2 = new Tile(tile.Z, tile.X, tile.Y);
        if (numberOfFeatures == 0) {
            t2.Available = false;
            tiles.Add(t2);
        }
        else if (numberOfFeatures > maxFeaturesPerTile) {
            t2.Available = false;
            tiles.Add(t2);

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
                    new_tile.BoundingBox = bboxQuad;
                    GenerateTiles(bboxQuad, new_tile, tiles);
                }
            }
        }
        else {

            if (!skipCreateTiles) {

                var file = $"{outputFolder}{Path.AltDirectorySeparatorChar}{tile.Z}_{tile.X}_{tile.Y}.b3dm";
                Console.Write($"\rCreating tile: {file}  ");

                var geometries = GeometryRepository.GetGeometrySubset(conn, table, geometryColumn, translation, tile, epsg, colorColumn, attributesColumn, lodColumn, query);
                var bytes = B3dmWriter.ToB3dm(geometries, copyright);

                File.WriteAllBytes(file, bytes);
            }

            t2.BoundingBox = tile.BoundingBox;
            t2.Available = true;
            tiles.Add(t2);
        }

        return tiles;
    }
}
