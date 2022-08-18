using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests
{
    public class TreeSerializerTests
    {
        [Test]
        public void SerializeToImplicitTilingTest()
        {
            // arrange
            var t0 = new Tile(0, new BoundingBox());
            var t1 = new Tile(1, new BoundingBox());
            var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
            var bbox = new double[] { 0, 0, 1, 1 };

            // act
            var json = TreeSerializer.ToImplicitTileset(translation, bbox, 500, 5, "ADD");
            File.WriteAllText(@"d:\aaa\subtree\test.json", json);
            var jsonobject = JObject.Parse(json);

            // assert
            Assert.IsTrue(jsonobject != null);
        }

        [Test]
        public void SerializeToJsonTest()
        {
            // arrange
            var t0 = new Tile(0, new BoundingBox());
            var t1 = new Tile(1, new BoundingBox());
            var tiles = new List<Tile> { t0, t1 };
            var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
            var bbox = new double[] { 0, 0, 1, 1 };

            // act
            var json = TreeSerializer.ToJson(tiles, translation, bbox, 500, "replace");
            var jsonobject = JObject.Parse(json);
            
            // assert
            Assert.IsTrue(jsonobject != null);


        }

        [Test]
        public void SerializeTree()
        {
            // arrange
            var t0=new Tile(0, new BoundingBox());
            var t1 = new Tile(1, new BoundingBox());
            var tiles = new List<Tile> { t0, t1 };

            // act
            var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
            var bbox = new double[] { 0, 0, 1, 1 };

            // assert
            var tileset = TreeSerializer.ToTileset(tiles, translation, bbox, 500, "replace");
            Assert.IsTrue(tileset.root.children.Count == 2);
        }


        [Test]
        public void SerializeTreeWithLods()
        {
            // arrange
            var t0 = new Tile(0, new BoundingBox());
            var t0_1 = new Tile(2, new BoundingBox());
            t0.Children = new List<Tile> { t0_1 };

            var t1 = new Tile(1, new BoundingBox());
            var tiles = new List<Tile> { t0, t1 };

            // act
            var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
            var bbox = new double[] { 0, 0, 1, 1 };

            // assert
            var tileset = TreeSerializer.ToTileset(tiles, translation, bbox, 500, "replace");
            Assert.IsTrue(tileset.root.children[0].children.Count == 1);
        }
    }
}
