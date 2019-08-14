using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests
{
    public class TriangleTests
    {
        [Test]
        public void TestArea()
        {
            // arrange
            var p0 = new Point(0, 0, 0);
            var p1 = new Point(1, 1, 0);
            var p2 = new Point(1, 0, 0);
            var t = new Triangle(p0, p1, p2);

            // act
            var area = t.Area();

            // assert
            Assert.IsTrue(area > 0.5 - 0.00001);
        }
    }
}
