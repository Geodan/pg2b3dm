using System;
using System.Collections.Generic;

namespace B3dm.Tileset
{
    public static class TreeSerializer
    {
        public static TileSet ToTileset(Node n, double[] transform)
        {
            Counter.Instance.Count = 0;
            double geometricError = 500.0;
            var tileset = new TileSet();
            tileset.asset = new Asset() { version = "1.0" };
            var t = new double[] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, Math.Round(transform[0], 3), Math.Round(transform[1], 3), Math.Round(transform[2], 3), 1.0 };
            tileset.geometricError = geometricError;
            var root = GetRoot(n, geometricError, t);
            tileset.root = root;
            return tileset;
        }

        private static Root GetRoot(Node n, double geometricError, double[] t)
        {
            var root = new Root();
            root.geometricError = geometricError;
            root.refine = "add";
            root.transform = t;
            root.boundingVolume = GetBoundingvolume(n);
            n.Id = Counter.Instance.Count;
            var children = GetChildren(n, geometricError);
            root.children = children;
            return root;
        }

        private static List<Child> GetChildren(Node n, double geometricError)
        {
            var children = new List<Child>();
            foreach (var node in n.Children) {
                Counter.Instance.Count++;
                node.Id = Counter.Instance.Count;
                var child = GetChild(node, geometricError / 2);
                children.Add(child);
            }

            return children;
        }

        public static Child GetChild(Node node, double geometricError)
        {
            var child = new Child();
            child.geometricError=geometricError;
            child.refine = "add";
            child.content = new Content();
            child.content.uri = $"tiles/{node.Id}.b3dm";
            child.boundingVolume = GetBoundingvolume(node);
            child.children = GetChildren(node,geometricError);
            return child;
        }

        private static Boundingvolume GetBoundingvolume(Node n)
        {
            var bbox = n.CalculateBoundingBox3D();
            var boundingVolume = new Boundingvolume();
            boundingVolume.box = bbox.GetBox();
            return boundingVolume;
        }

    }
}
