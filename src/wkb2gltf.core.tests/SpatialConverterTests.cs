using NUnit.Framework;

namespace Wkb2Gltf.Tests;
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
}
