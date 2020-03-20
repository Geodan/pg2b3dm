using System.Collections.Generic;

namespace B3dm.Tileset
{
    public static class TreeSerializer
    {

        public static TileSet ToTileset(List<Tile> tiles, double[] transform, double[] box)
        {
            var geometricError = 500.0;
            var tileset = new TileSet {
                asset = new Asset() { version = "1.0", generator = "pg2b3dm" }
            };
            var t = new double[] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, transform[0], transform[1], transform[2], 1.0 };
            tileset.geometricError = geometricError;
            var root = GetRoot(tiles, geometricError, t, box);
            tileset.root = root;
            return tileset;
        }

        private static Root GetRoot(List<Tile> tiles, double geometricError, double[] translation, double[] box)
        {
            var boundingVolume = new Boundingvolume {
                box = box
            };

            var root = new Root {
                geometricError = geometricError,
                refine = "REPLACE",
                transform = translation,
                boundingVolume = boundingVolume
            };
            var children = GetChildren(tiles, translation);
            root.children = children;
            return root;
        }

        private static List<Child> GetChildren(List<Tile> tiles, double[] translation)
        {
            var counter = 0;
            var children = new List<Child>();
            foreach (var tile in tiles) {
                counter++;
                var child = GetChild(tile, counter, 0, translation);
                children.Add(child);
            }

            return children;
        }

        public static Child GetChild(Tile tile, int id, double geometricError, double[] translation)
        {
            var child = new Child {
                geometricError = geometricError,
                // child.refine = "REPLACE";
                content = new Content()
            };
            child.content.uri = $"tiles/{id}.b3dm";
            child.boundingVolume = tile.Boundingvolume;
            // child.children = GetChildren(node,geometricError);
            return child;
        }
    }
}
