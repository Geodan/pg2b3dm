using System;
using System.Collections.Generic;
using System.IO;
using B3dm.Tileset;
using Npgsql;
using Wkx;

namespace pg2b3dm;

public static class ImplicitTiling
{
    public static List<Tile> GenerateTiles(string table, NpgsqlConnection conn, int epsg, string geometry_column, BoundingBox bbox, int maxFeaturesPerTile, B3dm.Tileset.Tile tile, List<B3dm.Tileset.Tile> tiles, string query, double[] translation, string colorColumn, string attributesColumn, string lodColumn, string outputFolder, List<int> lods, string copyright="", bool skipCreateTiles = false)
    {
        var where = (query != string.Empty ? $" and {query}" : String.Empty);

        if (lods.Count > 1) {
            where += $" and {lodColumn}={lods[0]}";
        }
        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, table, geometry_column, new Point(bbox.XMin, bbox.YMin), new Point(bbox.XMax, bbox.YMax), epsg, where);

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
                    GenerateTiles(table, conn, epsg, geometry_column, bboxQuad, maxFeaturesPerTile, new_tile, tiles, query, translation, colorColumn, attributesColumn, lodColumn, outputFolder, lods, copyright, skipCreateTiles);
                }
            }
        }
        else {

            if (!skipCreateTiles) {

                var file = $"{outputFolder}{Path.AltDirectorySeparatorChar}{tile.Z}_{tile.X}_{tile.Y}.b3dm";
                Console.Write($"\rCreating tile: {file}  ");

                var geometries = BoundingBoxRepository.GetGeometrySubset(conn, table, geometry_column, translation, tile, epsg, colorColumn, attributesColumn, lodColumn, query);
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
