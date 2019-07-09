using System.Collections.Generic;
using NUnit.Framework;
using Wkb2Gltf;

namespace B3dm.Tileset.Tests
{
    public class TileCutterTests
    {
        private bool FindInList(List<BoundingBox3D> bbs, BoundingBox3D bb)
        {
            foreach(var b in bbs) {
                if (b.Equals(bb)) return true;
            }
            return false;
        }

        private List<BoundingBox3D> GetBoundingBoxes(Node node)
        {
            var res = new List<BoundingBox3D>();
            var bb = node.CalculateBoundingBox3D();
            res.Add(bb);

            foreach (var c in node.Children) {
                var newbb = GetBoundingBoxes(c);
                res.AddRange(newbb);
            }
            return res;
        }
    }
}
