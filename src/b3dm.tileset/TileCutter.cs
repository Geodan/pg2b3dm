using System;
using System.Collections.Generic;
using Npgsql;
using Wkx;

namespace B3dm.Tileset
{
    public static class TileCutter
    {
        public static Boundingvolume GetBoundingvolume(BoundingBox3D bbox3d)
        {
            var boundingVolume = new Boundingvolume {
                box = bbox3d.GetBox()
            };
            return boundingVolume;
        }

        public static List<Tile> GetTiles(NpgsqlConnection conn, double extentTile, string geometryTable, string geometryColumn, string idcolumn, BoundingBox3D box3d ,int epsg, List<int> lods, double[] geometricErrors, string lodcolumn = "")
        {
            var tiles = new List<Tile>();

            var xrange = (int)Math.Ceiling(box3d.ExtentX() / extentTile);
            var yrange = (int)Math.Ceiling(box3d.ExtentY() / extentTile);
            var tileId = 1;
            for (var x = 0; x < xrange; x++) {
                for (var y = 0; y < yrange; y++) {
                    Tile parent = null;
                    foreach(var lod in lods) {
                        var lodQuery = LodQuery.GetLodQuery(lodcolumn, lod);

                        var from = new Point(box3d.XMin + extentTile * x, box3d.YMin + extentTile * y);
                        var to = new Point(box3d.XMin + extentTile * (x + 1), box3d.YMin + extentTile * (y + 1));
                        var count = BoundingBoxRepository.HasFeaturesInBox(conn, geometryTable, geometryColumn, from, to, epsg, lodQuery);
                        if (count ) {
                            var tile = new Tile(tileId, new BoundingBox((double)from.X, (double)from.Y, (double)to.X, (double)to.Y));
                            tile.Lod = lod;
                            tile.GeometricError = geometricErrors[lod];
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
