using System.Collections.Generic;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
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

        [Test]
        public void RotateAndTranslateTest()
        {
            // arrange
            var bb = new BoundingBox3D(-8413063.545175588, 4746417.570850967, 0, -8413036.978556471, 4746442.517736644, 11.06);
            var translation = new double[] { -8406745.007853176, 4744614.257728589, 38.29 };
            var expectedBoundingVolume = new BoundingBox3D(-6318.537322411314, -38.289999999999885, -1828.2600080547854, -6291.970703294501, -27.229999999999883, -1803.3131223786622);

            // act
            var actualRotatedAndTranslatedBoundingVolumne = BoundingBoxCalculator.RotateAndTranslate(bb, translation);

            // assert
            // todo: fix this...
            Assert.IsTrue(expectedBoundingVolume.Equals(actualRotatedAndTranslatedBoundingVolumne));

        }
    }
}
