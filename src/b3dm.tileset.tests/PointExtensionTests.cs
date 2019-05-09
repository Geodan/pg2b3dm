using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests
{
    public class PointExtensionTests
    {
        [Test]
        public void PointExtensionToVectorTest()
        {
            // arrange
            var p = new Point(5,6,7) { };

            // act
            var v = p.ToVector();

            // assert
            Assert.IsTrue(v[0] == 5);
            Assert.IsTrue(v[1] == 6);
            Assert.IsTrue(v[2] == 7);
        }
    }
}
