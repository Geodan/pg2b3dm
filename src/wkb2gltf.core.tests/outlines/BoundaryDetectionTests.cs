using NUnit.Framework;
using Wkb2Gltf.outlines;

namespace Wkb2Gltf.Tests.outlines;
public class BoundaryDetectionTests
{
    [Test]
    public void TestSharedPoints()
    {
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 0, 0), 0);
        var t1 = new Triangle(new Wkx.Point(1, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 1, 0), 0);

        var boundary = BoundaryDetection.GetSharedPoints(t0, t1);

        Assert.That(boundary.first.Count == 2, Is.True);
        Assert.That(boundary.first[0] == 1, Is.True);
        Assert.That(boundary.first[1] == 2, Is.True);

        Assert.That(boundary.second.Count == 2, Is.True);
        Assert.That(boundary.second[0] == 1, Is.True);
        Assert.That(boundary.second[1] == 0, Is.True);
    }
}
