using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests;

public class EcefConverterTest
{
    [Test]
    public void Lla2EcefTest()
    {
        var p = new Point(-75.614, 38.908);
        var ecef1 = SpatialConverter.GeodeticToEcef((double)p.X,(double)p.Y, 0);
        Assert.That(ecef1.X > 0, Is.True);
    }
}
