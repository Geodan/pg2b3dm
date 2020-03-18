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

        public static List<Tile> GetTilesNew(NpgsqlConnection conn, double extentTile, string geometryTable, string geometryColumn, string idcolumn, BoundingBox3D box3d)
        {
            var epsg = 4978; // todo: make dynamic
            var tiles = new List<Tile>();
            var translation = box3d.GetCenter().ToVector();

            var xrange = (int)Math.Ceiling(box3d.ExtentX() / extentTile);
            var yrange = (int)Math.Ceiling(box3d.ExtentY() / extentTile);
            var tileId = 1;
            for (var x = 0; x < xrange; x++) {
                for (var y = 0; y < yrange; y++) {

                    // check if there are features in this tile...
                    var from = new Point(box3d.XMin + extentTile*x, box3d.YMin + extentTile*y);
                    var to = new Point(box3d.XMin + extentTile * (x+1), box3d.YMin + extentTile * (y+1));
                    var featuresInBox = BoundingBoxRepository.GetFeaturesInBox(conn, geometryTable, geometryColumn,from, to, epsg);
                    if(featuresInBox > 0) {
                        var tile = new Tile(tileId, new BoundingBox((double)from.X, (double)from.Y, (double)to.X, (double)to.Y), featuresInBox);
                        tiles.Add(tile);
                        tileId++;
                    }
                }
            }

            return tiles;
        }


        public static List<List<Feature>> GetTiles(NpgsqlConnection conn, double extentTile, string geometryTable, string geometryColumn, string idcolumn, double[] translation)
        {
            var zupBoxes = GetZupBoxes(conn, geometryTable, geometryColumn, idcolumn, translation);
            var tiles = TileCutter.GetTiles(zupBoxes, extentTile);
            return tiles;
        }

        public static List<List<Feature>> GetTiles(List<BoundingBox3D> zupboxes, double MaxTileSize)
        {
            // select min and max from zupboxes for x and y
            var bbox3d = BoundingBoxCalculator.GetBoundingBox(zupboxes);
            var bbox = bbox3d.ToBoundingBox();

            var xrange = (int)Math.Ceiling(bbox3d.ExtentX() / MaxTileSize);
            var yrange = (int)Math.Ceiling(bbox3d.ExtentY() / MaxTileSize);

            var tiles = new List<List<Feature>>();

            for (var x = 0; x < xrange; x++) {
                for (var y = 0; y < yrange; y++) {
                    var tileextent = B3dmTile.GetExtent(bbox, MaxTileSize, x, y);
                    var features = new List<Feature>();

                    // loop through all zupboxes
                    foreach (var zUpBox in zupboxes) {
                        var isinside = tileextent.Inside(zUpBox.GetCenter());
                        if (isinside) {
                            var f = new Feature() { Id = zUpBox.Id, BoundingBox3D = zUpBox };
                            features.Add(f);
                        }
                    }

                    if (features.Count > 0) {
                        tiles.Add(features);
                    }
                }

            }
            return tiles;
        }
    }
}
