using System.Collections.Generic;
using NUnit.Framework;

namespace Wkb2Gltf.Tests
{
    public class BoundingBoxCalculatorTests
    {
        [Test]
        public void BoundingBoxCalculatorTest()
        {
            // arrange
            var bboxes = new List<BoundingBox3D>();
            var bbox1 = new BoundingBox3D(0, 0, 0, 1, 1, 1);
            bboxes.Add(bbox1);

            // act
            var box = BoundingBoxCalculator.GetBoundingBox(bboxes);

            // assert
            Assert.IsTrue(box.XMin == 0);
            Assert.IsTrue(box.YMin == 0);
            Assert.IsTrue(box.XMax == 1);
            Assert.IsTrue(box.YMax == 1);
        }
    }
}
