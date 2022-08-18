using System;
using System.Collections.Generic;
using B3dm.Tileset.extensions;
using Newtonsoft.Json;

namespace B3dm.Tileset
{
    public static class TreeSerializer
    {
        public static string ToJson(List<Tile2> tiles, double[] transform, double[] region, double[] geometricErrors, string refinement, double minheight, double maxheight, Version version = null)
        {
            var tileset = ToTileset(tiles, transform, region, geometricErrors, refinement, minheight, maxheight, version);
            var json = JsonConvert.SerializeObject(tileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            return json; ;
        }

        public static string ToImplicitTileset(double[] transform, double[] box, double maxGeometricError, int subtreeLevels, string refinementMethod, Version version=null)
        {
            var geometricError = maxGeometricError;
            var tileset = new TileSet {
                asset = new Asset() { version = "1.1", generator = $"pg2b3dm {version}" }
            };
            var t = new double[] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, transform[0], transform[1], transform[2], 1.0 };
            var root = GetRoot(geometricError, t, box, refinementMethod);
            var content = new Content() { uri = "content/{level}_{x}_{y}.b3dm" };
            root.content = content;
            var subtrees = new Subtrees() { uri = "subtrees/{level}_{x}_{y}.subtree" };
            root.implicitTiling = new Implicittiling() { subdivisionScheme = "QUADTREE", subtreeLevels = subtreeLevels, subtrees = subtrees };
            tileset.root = root;
            var json = JsonConvert.SerializeObject(tileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            return json;
        }

        public static TileSet ToTileset(List<Tile2> tiles, double[] transform, double[] region, double[] geometricErrors, string refinement, double minheight, double maxheight, Version version = null)
        {
            var tileset = new TileSet {
                asset = new Asset() { version = "1.0", generator = $"pg2b3dm {version}" }
            };
            var t = new double[] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, transform[0], transform[1], transform[2], 1.0 };
            var root = GetRoot(geometricErrors[0], t, region, refinement);
            var children = GetChildren(tiles, geometricErrors[1], minheight, maxheight);
            root.children = children;
            tileset.root = root;
            return tileset;
        }

        private static Root GetRoot(double geometricError, double[] translation, double[] region, string refinement)
        {
            var boundingVolume = new Boundingvolume {
                region = region
            };

            var root = new Root {
                geometricError = geometricError,
                refine = refinement,
                transform = translation,
                boundingVolume = boundingVolume
            };

            return root;
        }

        private static List<Child> GetChildren(List<Tile2> tiles, double geometricError, double minheight, double maxheight)
        {
            var children = new List<Child>();
            foreach (var tile in tiles) {
                if (tile.Available) {
                    var child = GetChild(tile, geometricError, minheight, maxheight);

                    if (tile.Children != null) {
                        child.children = GetChildren(tile.Children, geometricError, minheight, maxheight);
                    }
                    children.Add(child);
                }
            }

            return children;
        }

        public static Child GetChild(Tile2 tile, double geometricError, double minheight, double maxheight)
        {
            var child = new Child {
                geometricError = geometricError,
                content = new Content()
            };
            child.content.uri = $"content/{tile.Z}_{tile.X}_{tile.Y}.b3dm";

            var region = tile.BoundingBox.ToLatLon().ToRadians().ToRegion(minheight,maxheight);
            child.boundingVolume = new Boundingvolume {
                region = region
            };

            return child;
        }
    }
}
