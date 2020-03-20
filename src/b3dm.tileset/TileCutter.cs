using System;
using System.Collections.Generic;
using Npgsql;
using Wkx;

namespace B3dm.Tileset
{
    public static class TileCutter
    {
        public static List<BoundingBox3D> GetZupBoxes(NpgsqlConnection conn, string GeometryTable, string GeometryColumn, string idcolumn, double[] translation)
        {
            var bboxes = BoundingBoxRepository.GetAllBoundingBoxes(conn, GeometryTable, GeometryColumn, idcolumn, translation);
            var zupBoxes = new List<BoundingBox3D>();
            foreach (var bbox in bboxes) {
                var zupBox = bbox.TransformYToZ();
                zupBoxes.Add(zupBox);
            }

            return zupBoxes;
        }

        public static List<Tile> GetTiles(NpgsqlConnection conn, double extentTile, string geometryTable, string geometryColumn, string idcolumn, BoundingBox3D box3d ,int epsg)
        {
            var tiles = new List<Tile>();

            var xrange = (int)Math.Ceiling(box3d.ExtentX() / extentTile);
            var yrange = (int)Math.Ceiling(box3d.ExtentY() / extentTile);
            var tileId = 1;
            for (var x = 0; x < xrange; x++) {
                for (var y = 0; y < yrange; y++) {

                    // check if there are features in this tile...
                    var from = new Point(box3d.XMin + extentTile*x, box3d.YMin + extentTile*y);
                    var to = new Point(box3d.XMin + extentTile * (x+1), box3d.YMin + extentTile * (y+1));
                    var ids = BoundingBoxRepository.GetFeaturesInBox(conn, geometryTable, geometryColumn,idcolumn, from, to, epsg);
                    if(ids.Count > 0) {
                        var tile = new Tile(tileId, new BoundingBox((double)from.X, (double)from.Y, (double)to.X, (double)to.Y), ids);
                        tiles.Add(tile);
                        tileId++;
                    }
                }
            }

            return tiles;
        }
    }
}
