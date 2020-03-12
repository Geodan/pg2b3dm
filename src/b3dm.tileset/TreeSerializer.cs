using System.Collections.Generic;

namespace B3dm.Tileset
{
    public static class TreeSerializer
    {
        public static TileSet ToTileset(List<List<Feature>> tiles, double[] transform)
        {
            Counter.Instance.Count = 0;
            double geometricError = 500.0;
            var tileset = new TileSet();
            tileset.asset = new Asset() { version = "1.0", generator="pg2b3dm" };
            var t = new double[] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, transform[0], transform[1], transform[2], 1.0 };
            tileset.geometricError = geometricError;
            var root = GetRoot(tiles, geometricError, t);
            tileset.root = root;
            return tileset;
        }

        private static Root GetRoot(List<List<Feature>> tiles, double geometricError, double[] t)
        {
            var root = new Root();
            root.geometricError = geometricError;
            root.refine = "REPLACE";
            root.transform = t;
            root.boundingVolume = GetBoundingvolume(tiles);
            var children = GetChildren(tiles);
            root.children = children;
            return root;
        }

        private static List<Child> GetChildren(List<List<Feature>> tiles)
        {
            var counter = 0;
            var children = new List<Child>();
            foreach (var tile in tiles) {
                counter++;
                var child = GetChild(tile, counter, 0);
                children.Add(child);
            }

            return children;
        }

        public static Child GetChild(List<Feature> tile, int id, double geometricError)
        {
            var child = new Child();
            child.geometricError=geometricError;
            // child.refine = "REPLACE";
            child.content = new Content();
            child.content.uri = $"tiles/{id}.b3dm";
            child.boundingVolume = GetBoundingvolume(tile);
            // child.children = GetChildren(node,geometricError);
            return child;
        }

        private static Boundingvolume GetBoundingvolume(List<Feature> features)
        {
            var bboxes = new List<BoundingBox3D>();
            foreach (var f in features) {
                bboxes.Add(f.BoundingBox3D);
            }
            var bbox = BoundingBoxCalculator.GetBoundingBox(bboxes);
            var boundingVolume = new Boundingvolume();
            boundingVolume.box = bbox.GetBox();
            return boundingVolume;
        }


        private static Boundingvolume GetBoundingvolume(List<List<Feature>> tiles)
        {
            var bboxes = new List<BoundingBox3D>();
            foreach(var t in tiles) {
                foreach (var f in t) {
                    bboxes.Add(f.BoundingBox3D);
                }
            }
            var bbox = BoundingBoxCalculator.GetBoundingBox(bboxes);
            var boundingVolume = new Boundingvolume();
            boundingVolume.box = bbox.GetBox();
            return boundingVolume;
        }

    }
}
