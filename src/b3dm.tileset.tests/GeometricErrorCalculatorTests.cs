using System.Collections.Generic;
using NUnit.Framework;

namespace B3dm.Tileset.Tests;

public class GeometricErrorCalculatorTests
{
    [Test]
    public void CalculateGeometricErrorFirstTest()
    {
        var lods = new List<int> { 0, 1 };
        var geometricErrors = GeometricErrorCalculator.GetGeometricErrors(500, lods);

        Assert.That(geometricErrors.Length == 3, Is.True);
        Assert.That(geometricErrors[0] == 500, Is.True);
        Assert.That(geometricErrors[1] == 250, Is.True);
        Assert.That(geometricErrors[2] == 0, Is.True);
    }

    [Test]
    public void CalculateGeometricErrorForOnly1Level()
    {
        var lods = new List<int> { 0 };
        var geometricErrors = GeometricErrorCalculator.GetGeometricErrors(500, lods);

        Assert.That(geometricErrors.Length == 2, Is.True);
        Assert.That(geometricErrors[0] == 500, Is.True);
        Assert.That(geometricErrors[1] == 0, Is.True);
    }

    [Test]
    public void CalculateGeometricErrorRoundingTest(){
        var lods = new List<int> { 0,1,2 };
        var geometricErrors = GeometricErrorCalculator.GetGeometricErrors(100, lods);
        Assert.That(geometricErrors.Length == 4, Is.True);
        Assert.That(geometricErrors[0] == 100, Is.True);
        Assert.That(geometricErrors[1] == 67, Is.True);
        Assert.That(geometricErrors[2] == 33, Is.True);
        Assert.That(geometricErrors[3] == 0, Is.True);
    }
}
