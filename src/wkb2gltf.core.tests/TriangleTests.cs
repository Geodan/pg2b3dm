using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests
{
    public class TriangleTests
    {
        [Test]
        public void TestIsDegenerated()
        {
            // arrange
            var p0 = new Point(4403.12831325084, 5497.3228336684406, -451.62756590108586);
            var p1 = new Point(4403.1283132596873, 5497.3228336591274, -451.62756593199413);
            var p2 = new Point(4392.54991199635, 5483.549242743291, -450.72132376581396);

            var triangle = new Triangle(p0, p1, p2);

            // act
            var isDegenerated = triangle.IsDegenerated();

            // assert
            Assert.IsTrue(isDegenerated);
        }

        [Test]
        public void ToVectorsTest()
        {
            // arrange
            var p0 = new Point(4403.12831325084, 5497.3228336684406, -451.62756590108586);
            var p1 = new Point(4403.1283132596873, 5497.3228336591274, -451.62756593199413);
            var p2 = new Point(4392.54991199635, 5483.549242743291, -450.72132376581396);

            var triangle = new Triangle(p0, p1, p2);

            // act
            var res = triangle.ToVectors();

            // assert
            Assert.IsTrue(res.Item1.X == 4403.12842f);

        }
    }
}
