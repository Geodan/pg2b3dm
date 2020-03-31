using System;
using System.Collections.Generic;
using Npgsql;
using Wkx;

namespace B3dm.Tileset
{
    public static class TileCutter
    {

        public static Boundingvolume GetTileBoundingBoxNew(NpgsqlConnection conn, string GeometryTable, string GeometryColumn, string idcolumn, double[] translation, string[] ids)
        {
            var bboxes = BoundingBoxRepository.GetAllBoundingBoxesForTile(conn, GeometryTable, GeometryColumn, idcolumn, translation, ids);
            var zupBoxes = new List<BoundingBox3D>();
            foreach (var bbox in bboxes) {
                var zupBox = bbox.TransformYToZ();
                zupBoxes.Add(zupBox);
            }

            var bvol = GetBoundingvolume(zupBoxes);
            return bvol;

        }

        public static Boundingvolume GetBoundingvolume(List<BoundingBox3D> bbs)
        {
            var bbox = BoundingBoxCalculator.GetBoundingBox(bbs);
            var boundingVolume = new Boundingvolume {
                box = bbox.GetBox()
            };
            return boundingVolume;
        }

        public static List<Tile> GetTiles(NpgsqlConnection conn, double extentTile, string geometryTable, string geometryColumn, string idcolumn, BoundingBox3D box3d ,int epsg, List<int> lods, string lodcolumn = "")
        {
            var tiles = new List<Tile>();

            var xrange = (int)Math.Ceiling(box3d.ExtentX() / extentTile);
            var yrange = (int)Math.Ceiling(box3d.ExtentY() / extentTile);
            var tileId = 1;
            for (var x = 0; x < xrange; x++) {
                for (var y = 0; y < yrange; y++) {
                    Tile parent = null;
                    foreach(var lod in lods) {

                        var lodQuery = "";
                        if (lodcolumn != String.Empty) {
                            lodQuery = $"and {lodcolumn}={lod}";
                        }

                        var from = new Point(box3d.XMin + extentTile * x, box3d.YMin + extentTile * y);
                        var to = new Point(box3d.XMin + extentTile * (x + 1), box3d.YMin + extentTile * (y + 1));
                        var ids = BoundingBoxRepository.GetFeaturesInBox(conn, geometryTable, geometryColumn, idcolumn, from, to, epsg, lodQuery);
                        if (ids.Count > 0) {
                            var tile = new Tile(tileId, new BoundingBox((double)from.X, (double)from.Y, (double)to.X, (double)to.Y), ids);
                            tile.Lod = lod;
                            if (parent != null) {
                                parent.Child = tile;
                            }
                            else {
                                tiles.Add(tile);
                            }
                            parent = tile;
                            tileId++;
                        }
                    }
                    parent = null;
                }
            }

            return tiles;
        }
    }
}
