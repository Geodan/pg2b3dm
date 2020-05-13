using System.Collections.Generic;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class GeometricErrorCalculatorTests
    {
        [Test]
        public void CalculateGeometricErrorFirstTest()
        {
            var lods = new List<int> { 0, 1 };
            var geometricErrors = GeometricErrorCalculator.GetGeometricErrors(500, lods);

            Assert.IsTrue(geometricErrors.Length == 3);
            Assert.IsTrue(geometricErrors[0] == 500);
            Assert.IsTrue(geometricErrors[1] == 250);
            Assert.IsTrue(geometricErrors[2] == 0);
        }

        [Test]
        public void CalculateGeometricErrorForOnly1Level()
        {
            var lods = new List<int> { 0 };
            var geometricErrors = GeometricErrorCalculator.GetGeometricErrors(500, lods);

            Assert.IsTrue(geometricErrors.Length == 2);
            Assert.IsTrue(geometricErrors[0] == 500);
            Assert.IsTrue(geometricErrors[1] == 0);
        }

        [Test]
        public void CalculateGeometricErrorRoundingTest(){
            var lods = new List<int> { 0,1,2 };
            var geometricErrors = GeometricErrorCalculator.GetGeometricErrors(100, lods);
            Assert.IsTrue(geometricErrors.Length == 4);
            Assert.IsTrue(geometricErrors[0] == 100);
            Assert.IsTrue(geometricErrors[1] == 67);
            Assert.IsTrue(geometricErrors[2] == 33);
            Assert.IsTrue(geometricErrors[3] == 0);
        }
    }
}
