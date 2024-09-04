using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace B3dm.Tileset.Tests;

public class GeometricErrorCalculatorTests
{
    [Test]
    public void TestGeometricError()
    {
        var geometricError = 200000;
        var geometricErrorFactor = 1.8;

        var level = 11;

        var test = GeometricErrorCalculator.GetGeometricError(geometricError, geometricErrorFactor, level + 2);
        Assert.That(96 == Math.Round(test));
    }


    [Test]
    public void GeometricErrorCalculator2()
    {
        Assert.That(GeometricErrorCalculator.GetGeometricError(2000, 2, 1) == 1000);
        Assert.That(GeometricErrorCalculator.GetGeometricError(2000, 2, 0) == 2000);
        Assert.That(GeometricErrorCalculator.GetGeometricError(2000, 2, 2) == 500);
    }
}
