using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using subtree;

namespace B3dm.Tileset.Tests;
public class TileSelectorTests
{
    [Test]
    public void FirstTileSelectorTest()
    {
        // arrange
        var t0 = new Tile(0, 0, 0);
        t0.Available = true;
        var t1 = new Tile(1, 1, 1);
        t1.Available = true;
        var tiles = new List<Tile> { t0, t1 };

        // act
        var result = TileSelector.Select(tiles, t0, 0, 1);

        // assert
        Assert.That(result.Count == 1);
        Assert.That(result.First().Z== 1 && result.First().X == 1 && result.First().Y == 1);
    }

    [Test]
    public void TestSelectWith1Tile()
    {
        var t0 = new Tile(0, 0, 0);
        t0.Available = true;
        var tiles = new List<Tile> { t0 };

        // act
        var result = TileSelector.Select(tiles, t0, 0, 1);

        // assert
        Assert.That(result.Count == 1);

    }
}
