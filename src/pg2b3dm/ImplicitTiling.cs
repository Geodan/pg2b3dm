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

        public static List<subtree.Tile> GenerateTiles(string table, NpgsqlConnection conn, int epsg, string geometry_column, string id_column, BoundingBox bbox, int maxFeaturesPerTile, subtree.Tile tile, List<subtree.Tile> tiles, string query, double[] translation, string colorColumn, string attributesColumn, string outputFolder, string copyright="")
        {
            var where = (query != string.Empty ? $" and {query}" : String.Empty);

            var numberOfFeatures = BoundingBoxRepository.CountFeaturesInBox(conn, table, geometry_column, new Point(bbox.XMin, bbox.YMin), new Point(bbox.XMax, bbox.YMax), epsg, where);

            if (numberOfFeatures == 0) {
                var t2 = new subtree.Tile(tile.Z, tile.X, tile.Y);
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

                        var new_tile = new subtree.Tile(tile.Z + 1, tile.X * 2 + x, tile.Y * 2 + y);
                        GenerateTiles(table, conn, epsg, geometry_column, id_column, bboxQuad, maxFeaturesPerTile, new_tile, tiles, query, translation, colorColumn, attributesColumn, outputFolder, copyright);
                    }
                }
            }
            else {
                var file = $"{outputFolder}{Path.DirectorySeparatorChar}{tile.Z}_{tile.X}_{tile.Y}.b3dm";
                Console.Write($"\rCreating tile: {file}");

                var geometries = BoundingBoxRepository.GetGeometrySubsetForImplicitTiling(conn, table, geometry_column, bbox, id_column, translation, epsg, colorColumn, attributesColumn, query);
                var bytes = B3dmWriter.ToB3dm(geometries, copyright);

                File.WriteAllBytes(file, bytes);
                var t1 = new subtree.Tile(tile.Z, tile.X, tile.Y);
                t1.Available = true;
                tiles.Add(t1);
            }

            return tiles;
        }
    }
}
