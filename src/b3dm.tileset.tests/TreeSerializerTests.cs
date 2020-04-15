using System.Collections.Generic;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class TreeSerializerTests
    {
        [Test]
        public void SerializeTree()
        {
            // arrange
            var t0=new Tile(0, new Wkx.BoundingBox());
            var t1 = new Tile(1, new Wkx.BoundingBox());
            var tiles = new List<Tile> { t0, t1 };

            // act
            var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
            var bbox = new double[] { 0, 0, 1, 1 };

            // assert
            var tileset = TreeSerializer.ToTileset(tiles, translation, bbox, 500);
            Assert.IsTrue(tileset.root.children.Count == 2);
        }


        [Test]
        public void SerializeTreeWithLods()
        {
            // arrange
            var t0 = new Tile(0, new Wkx.BoundingBox());
            var t0_1 = new Tile(2, new Wkx.BoundingBox());
            t0.Child = t0_1;

            var t1 = new Tile(1, new Wkx.BoundingBox());
            var tiles = new List<Tile> { t0, t1 };

            // act
            var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
            var bbox = new double[] { 0, 0, 1, 1 };

            // assert
            var tileset = TreeSerializer.ToTileset(tiles, translation, bbox, 500);
            Assert.IsTrue(tileset.root.children[0].children.Count == 1);
        }
    }
}
