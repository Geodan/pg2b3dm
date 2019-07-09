using System;
using NUnit.Framework;

namespace Wkb2Gltf.Tests
{
    public class BoundingBox3DTests
    {
        [Test]
        public void BoudingBox3DCenterTest()
        {
            // Arrange
            var bb = new BoundingBox3D() { XMin = 0, YMin = 0, ZMin = 0, XMax = 2, YMax = 2, ZMax = 2 };

            // act
            var center = bb.GetCenter();

            // assert
            Assert.IsTrue(center.X == 1 && center.Y==1 && center.Z == 1);
        }

        [Test]
        public void BoundingBox3DTransformYToZTest()
        {
            // Arrange
            var bb = new BoundingBox3D() { XMin = -105.4645, YMin = -11.3846445, ZMin = 72.374, XMax = -58.6345, YMax = -7.228368, ZMax = 127.598 };

            // act 
            var zUpBox = bb.TransformYToZ();

            // assert
            Assert.IsTrue(zUpBox.XMin == -105.4645);
            Assert.IsTrue(zUpBox.YMin == -72.374);
            Assert.IsTrue(zUpBox.ZMin == -11.3846445);

            Assert.IsTrue(zUpBox.XMax == -58.6345);
            Assert.IsTrue(zUpBox.YMax == -72.374);
            Assert.IsTrue(zUpBox.ZMax == -7.228368);
        }

        [Test]
        public void BoundingBoxToStringTest()
        {
            // Arrange
            var bb = new BoundingBox3D() { XMin = -105.4645, YMin = -11.3846445, ZMin = 72.374, XMax = -58.6345, YMax = -7.228368, ZMax = 127.598 };

            // act 
            var zUpBox = bb.ToString();

            // assert
            Assert.IsTrue(zUpBox == "-105.4645,-11.3846445,72.374,-58.6345,-7.228368,127.598");
        }

        [Test]
        public void BoundBoxToBoxTest()
        {
            // arrange
            var bb = new BoundingBox3D(-183.87249755859375, -134.17864990234375, -11.730524063110352, 183.87249755859375, 138.58799743652344, 11.730524063110352);

            // act
            var actual_result = bb.GetBox();

            // assert
            var expected_result = new double[] { 0.0, 2.205, 0.0, 183.872, 0, 0, 0, 136.383, 0, 0, 0, 11.731 };
        }

        [Test]
        public void BoundBoxEqualsTest()
        {
            // arrange
            var bb = new BoundingBox3D(-183.87249755859375, -134.17864990234375, -11.730524063110352, 183.87249755859375, 138.58799743652344, 11.730524063110352);
            var bb1 = new BoundingBox3D(-183.88249755859375, -134.18864990234375, -11.740524063110352, 183.86249755859375, 138.57799743652344, 11.720524063110352);

            // act
            var isequal = bb.Equals(bb1);

            // assert
            Assert.IsTrue(isequal);
        }
    }
}
