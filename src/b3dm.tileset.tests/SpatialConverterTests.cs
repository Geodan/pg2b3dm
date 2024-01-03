using System.Numerics;
using NUnit.Framework;

namespace B3dm.Tileset.Tests;
public class SpatialConverterTests
{
    [Test]
    public void TestGeodeticToEcef()
    {
        var result = SpatialConverter.GeodeticToEcef(0, 0, 0);
        Assert.That(result.X, Is.EqualTo(6378137.0));
        Assert.That(result.Y, Is.EqualTo(0.0));
        Assert.That(result.Z, Is.EqualTo(0.0));
    }

    [Test]
    // test function EcefToEnu
    public void TestEcefToEnu()
    {
        var p = new Vector3(3887940.499793678f, 332858.0186060839f, 5028256.028364045f);
        var result = SpatialConverter.EcefToEnu(p);
        Assert.That(result.M11, Is.EqualTo(-0.08530091f));
        Assert.That(result.M12, Is.EqualTo(0.996355236f));
    }

}
