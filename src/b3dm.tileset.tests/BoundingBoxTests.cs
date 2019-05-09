using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset
{
    public class BoundingBoxTests
    {
        [Test]
        public void BoundingBoxInsideTests()
        {
            // arrange
            var bb = new BoundingBox(0, 0, 10, 10);
            var insidepoint = new Point(5, 5);
            var outsidepoint = new Point(11, 11);

            // act
            var result_inside = bb.Inside(insidepoint);
            var result_outside = bb.Inside(outsidepoint);

            // assert
            Assert.True(result_inside);
            Assert.False(result_outside);
        }
    }
}
