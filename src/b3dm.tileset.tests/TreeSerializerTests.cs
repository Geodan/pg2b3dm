using System.Collections.Generic;
using B3dm.Tileset.Extensions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using subtree;
using Wkx;

namespace B3dm.Tileset.Tests;

public class TreeSerializerTests
{
    [Test]
    public void SerializeToImplicitTilingTest()
    {
        // arrange
        var t0 = new Tile(0, 0, 0 );
        var t1 = new Tile(1, 1,1);
        var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
        var bbox = new double[] { 0, 0, 1, 1 };

        // act
        var json = TreeSerializer.ToImplicitTileset(translation, bbox, 500, 5, 1);

        // assert
        Assert.That(json != null, Is.True);
    }

    [Test]
    public void SerializeTree()
    {
        // arrange
        var t0=new Tile(0, 0,0);
        t0.Available = true;
        t0.BoundingBox = new BoundingBox(0, 0, 10, 10).ToArray();
        var t1 = new Tile(1, 1, 1);
        t1.Available = true;
        t1.BoundingBox = new BoundingBox(0,0,10,10).ToArray();
        var tiles = new List<Tile> { t0, t1 };

        // act
        var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
        var bbox = new double[] { 0, 0, 1, 1 };

        // assert
        var tileset = TreeSerializer.ToTileset(tiles, translation, bbox, new double[] { 500, 0 }, 0, 10);
        Assert.That(tileset.root.children.Count == 2, Is.True);
    }


    [Test]
    public void SerializeTreeWithLods()
    {
        // arrange
        var t0 = new Tile(0,0,0);
        t0.Lod = 0;
        t0.Available = true;
        t0.BoundingBox = new BoundingBox(0, 0, 10, 10).ToArray();
        var t0_1 = new Tile(2,0,1);
        t0_1.Lod = 1;
        t0_1.Available = true;
        t0_1.BoundingBox = new BoundingBox(0, 0, 10, 10).ToArray();
        t0.Children = new List<Tile> { t0_1 };

        var tiles = new List<Tile> { t0 };

        // act
        var translation = new double[] { -8406745.0078531764, 4744614.2577285888, 38.29 };
        var bbox = new double[] { 0, 0, 1, 1 };

        // assert
        var tileset = TreeSerializer.ToTileset(tiles, translation, bbox, new double[] {500,100,0}, 0, 10);
        Assert.That(tileset.root.children[0].children.Count == 1, Is.True);
        Assert.That(tileset.root.geometricError==500, Is.True);
        Assert.That(tileset.root.children[0].geometricError == 100, Is.True);
        Assert.That(tileset.root.children[0].children[0].geometricError == 0, Is.True);

    }
}
