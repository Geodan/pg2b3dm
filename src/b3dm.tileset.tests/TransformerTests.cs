using System;
using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests
{
    public class TransformerTests
    {
        [Test]
        public void TestTranslate()
        {
            // arrange
            var point = new Point(1, 2, 3);

            // act
            var pointTranslated = point.Translate(10, 20, 30);

            // assert
            Assert.IsTrue(pointTranslated.Equals(new Point(11, 22, 33)));
        }

        [Test]
        public void TestRotateX()
        {
            // arrange
            var point = new Point(1, 2.5, 3);
            var pointRotatedXExpected = new Point(1, -2.5, -3);

            // act
            var pointRotatedXActual = point.RotateX(Math.PI);

            // assert
            Assert.IsTrue(pointRotatedXActual.Equals(pointRotatedXExpected));
        }
    }
}
