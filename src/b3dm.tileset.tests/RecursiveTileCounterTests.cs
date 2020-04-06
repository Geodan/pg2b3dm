using System.Collections.Generic;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class RecursiveTileCounterTests
    {
        [Test]
        public void RecursiveTileCountFirstTest()
        {
            // arrange

            var tiles = new List<Tile>();
            var t0 = new Tile(0, null);
            var t0_child = new Tile(100, null);
            t0.Child = t0_child;
            tiles.Add(t0);
            tiles.Add(new Tile(1, null));


            // act
            var tileCount = RecursiveTileCounter.CountTiles(tiles, 0);

            // assert
            Assert.IsTrue(tileCount == 3);
        }
    }
}
