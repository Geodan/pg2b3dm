using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests
{
    public class B3dmTileTests
    {
        [Test]
        public void B3dmTIleExtentTests()
        {
            // arrange
            var maxTileSize = 2000;
            var extent = new BoundingBox(-183.8725, -134.17865, 183.8725, 138.588);

            // act
            var b3dmextent = B3dmTile.GetExtent(extent, maxTileSize, 0, 0);

            // assert
            Assert.IsTrue(b3dmextent.XMin == -183.8725);
            Assert.IsTrue(b3dmextent.YMin == -134.17865);
            Assert.IsTrue(b3dmextent.XMax == 1816.1275);
            Assert.IsTrue(b3dmextent.YMax == 1865.82135);
        }
    }
}
