using System;
using System.Collections.Generic;

namespace B3dm.Tileset
{
    public static class TileCutter
    {
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
