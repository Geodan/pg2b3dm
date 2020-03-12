using System.Collections.Generic;

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
    }
}
