using System;
using System.Collections.Generic;
using System.IO;
using B3dm.Tileset;
using Npgsql;
using subtree;
using Wkx;

namespace pg2b3dm
{
    public static class ImplicitTiling
    {
        public static byte[] GetSubtreeBytes(string contentAvailability, string subtreeAvailability = null)
        {
            var subtree_root = new Subtree();
            // todo: use other constant for tile availability
            subtree_root.TileAvailabiltyConstant = 1;

            var s0_root = BitArrayCreator.FromString(contentAvailability);
            subtree_root.ContentAvailability = s0_root;

            if (subtreeAvailability != null) {
                var c0_root = BitArrayCreator.FromString(subtreeAvailability);
                subtree_root.ChildSubtreeAvailability = c0_root;
            }

            var subtreebytes = SubtreeWriter.ToBytes(subtree_root);
            return subtreebytes;
        }

        public static List<B3dm.Tileset.Tile> GenerateTiles(string table, NpgsqlConnection conn, int epsg, string geometry_column, string id_column, BoundingBox bbox, int maxFeaturesPerTile, B3dm.Tileset.Tile tile, List<B3dm.Tileset.Tile> tiles, string query, double[] translation, string colorColumn, string attributesColumn, string outputFolder, string copyright="", bool skipCreateTiles = false)
        {
            var where = (query != string.Empty ? $" and {query}" : String.Empty);

            var numberOfFeatures = BoundingBoxRepository.CountFeaturesInBox(conn, table, geometry_column, new Point(bbox.XMin, bbox.YMin), new Point(bbox.XMax, bbox.YMax), epsg, where);

            if (numberOfFeatures == 0) {
                var t2 = new B3dm.Tileset.Tile(tile.Z, tile.X, tile.Y);
                t2.Available = false;
                tiles.Add(t2);
            }
            else if (numberOfFeatures > maxFeaturesPerTile) {
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

                        var new_tile = new B3dm.Tileset.Tile(tile.Z + 1, tile.X * 2 + x, tile.Y * 2 + y);
                        new_tile.BoundingBox = bboxQuad;
                        GenerateTiles(table, conn, epsg, geometry_column, id_column, bboxQuad, maxFeaturesPerTile, new_tile, tiles, query, translation, colorColumn, attributesColumn, outputFolder, copyright, skipCreateTiles);
                    }
                }
            }
            else {
                if (!skipCreateTiles) {
                    var file = $"{outputFolder}{Path.AltDirectorySeparatorChar}{tile.Z}_{tile.X}_{tile.Y}.b3dm";
                    Console.Write($"\rCreating tile: {file}  ");

                    var geometries = BoundingBoxRepository.GetGeometrySubset(conn, table, geometry_column, id_column, translation, tile,  epsg, colorColumn, attributesColumn, query);
                    var bytes = B3dmWriter.ToB3dm(geometries, copyright);

                    File.WriteAllBytes(file, bytes);
                }
                var t1 = new B3dm.Tileset.Tile(tile.Z, tile.X, tile.Y);
                t1.BoundingBox = tile.BoundingBox;
                t1.Available = true;
                tiles.Add(t1);
            }

            return tiles;
        }
    }
}
