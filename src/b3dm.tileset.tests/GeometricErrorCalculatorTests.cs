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

            Assert.IsTrue(geometricErrors.Length== 2);
            Assert.IsTrue(geometricErrors[0] == 250);
        }


        [Test]
        public void CalculateGeometricErrorForOnly1Level()
        {
            var lods = new List<int> { 0 };
            var geometricErrors = GeometricErrorCalculator.GetGeometricErrors(500, lods);

            Assert.IsTrue(geometricErrors.Length == 1);
            Assert.IsTrue(geometricErrors[0] == 0);
        }

    }
}
