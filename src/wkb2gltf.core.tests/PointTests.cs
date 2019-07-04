using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests
{
    public class PointTests
    {
        [Test]
        public void PointMinusTest()
        {
            // arrange
            var p = new Point(5, 10, 0);
            var p1 = new Point(1, 1, 0);

            // act
            var difference = p.Minus(p1);

            // assert
            Assert.IsTrue(difference.X == 4);
            Assert.IsTrue(difference.Y == 9);

        }

    }
}
