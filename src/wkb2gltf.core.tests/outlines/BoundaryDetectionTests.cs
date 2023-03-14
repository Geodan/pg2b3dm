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

        Assert.IsTrue(boundary.first.Count == 2);
        Assert.IsTrue(boundary.first[0] == 1);
        Assert.IsTrue(boundary.first[1] == 2);

        Assert.IsTrue(boundary.second.Count == 2);
        Assert.IsTrue(boundary.second[0] == 1);
        Assert.IsTrue(boundary.second[1] == 0);
    }
}
