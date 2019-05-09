using System;
using System.Collections.Generic;
using System.Linq;
using B3dm.Tile;
using Wkb2Gltf;
using Wkx;

namespace B3dm.Tileset
{
    public static class TileCutter
    {
        public static Node ConstructTree(List<BoundingBox3D> zupboxes)
        {
            // select min and max from zupboxes for x and y
            var bbox3d = BoundingBoxCalculator.GetBoundingBox(zupboxes);
            var bbox = bbox3d.ToBoundingBox();

            double maxTileSize = 2000.0;
            var featuresPerTile = 20;

            var xrange = (int)Math.Ceiling(bbox3d.ExtentX() / maxTileSize);
            var yrange = (int)Math.Ceiling(bbox3d.ExtentY() / maxTileSize);

            var tree = new Node();

            for (var x = 0; x < xrange; x++) {
                for (var y = 0; y < yrange; y++) {
                    var tileextent = B3dmTile.GetExtent(bbox, maxTileSize, x, y);
                    var features = new List<Feature>();

                    // loop through all zupboxes
                    for (var t = 0; t < zupboxes.Count; t++) {
                        var isinside = tileextent.Inside(zupboxes[t]);
                        if (isinside) {
                            var f = new Feature() { Id = t, BoundingBox3D = zupboxes[t] };
                            features.Add(f);
                        }
                    }

                    if (features.Count == 0) {
                        continue;
                    }
                    var node = new Node();
                    if (features.Count > featuresPerTile) {
                        node.Features = features.Take(featuresPerTile).ToList();
                        var new_features = features.GetRange(featuresPerTile, features.Count - featuresPerTile).ToList();
                        var new_x = x * 2;
                        var new_y = y * 2;
                        var new_maxTileSize = maxTileSize / 2;
                        Divide(bbox, new_features, new_x, new_y, new_maxTileSize, featuresPerTile, node);
                    }
                    else {
                        node.Features = features;
                    }
                    tree.Children.Add(node);
                }

            }
            return tree;
        }

        private static void Divide(BoundingBox extent, List<Feature> features, int XOffset, int YOffset, double TileSize, int FeaturesPerTile, Node parent)
        {
            for (var i = 0; i < 2; i++) {
                for (var j = 0; j < 2; j++) {
                    var tileextent = B3dmTile.GetExtent(extent, TileSize, i, j);
                    var insideFeatures = new List<Feature>();

                    foreach (var f in features) {
                        var center = f.BoundingBox3D.GetCenter();
                        var isinside = tileextent.Inside(center);
                        if (isinside) {
                            var new_f = new Feature() { Id = f.Id, BoundingBox3D = f.BoundingBox3D };
                            insideFeatures.Add(new_f);
                        }
                    }

                    if (insideFeatures.Count == 0) {
                        continue;
                    }

                    var node = new Node();
                    if (insideFeatures.Count > FeaturesPerTile) {
                        node.Features = insideFeatures.Take(FeaturesPerTile).ToList();
                        var new_features = insideFeatures.GetRange(FeaturesPerTile, insideFeatures.Count - FeaturesPerTile).ToList();
                        var new_x = (XOffset + i) * 2;
                        var new_y = (YOffset + j) * 2;
                        var new_maxTileSize = TileSize / 2;
                        Divide(tileextent, new_features, new_x, new_y, new_maxTileSize, FeaturesPerTile, node);
                    }
                    else {
                        node.Features = insideFeatures;
                    }
                    parent.Children.Add(node);
                }
            }
        }
    }
}
