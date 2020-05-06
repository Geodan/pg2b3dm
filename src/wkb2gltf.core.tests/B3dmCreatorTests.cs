using System.Collections.Generic;
using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests
{
    public class B3dmCreatorTests
    {
        [Test]
        public void CreateB3dmTest()
        {
            // arrange
            var p0 = new Point(0, 0, 0);
            var p1 = new Point(1, 1, 0);
            var p2 = new Point(1, 0, 0);
            
            var triangle1 = new Triangle(p0, p1, p2,0);
            var triangles = new TriangleCollection() { triangle1 };

            // act
            var b3dm = B3dmCreator.GetB3dm("id", new List<object>() { "1" }, triangles);

            // assert
            Assert.IsTrue(b3dm.B3dmHeader.Version == 1);
            Assert.IsTrue(b3dm.BatchTableJson == "{\"id\":[\"1\"]}");
        }
    }
}
