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

    [Test]
    public void TestSharedPointsCoplanar()
    {
        // Two coplanar triangles on the same plane (z=0, normal pointing up)
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 0, 0), 0);
        var t1 = new Triangle(new Wkx.Point(1, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 1, 0), 0);

        var boundary = BoundaryDetection.GetSharedPoints(t0, t1, checkCoplanar: true);

        // Should return shared points because triangles are coplanar
        Assert.That(boundary.first.Count, Is.EqualTo(2));
        Assert.That(boundary.second.Count, Is.EqualTo(2));
    }

    [Test]
    public void TestSharedPointsNonCoplanar()
    {
        // Two non-coplanar triangles with shared edge
        // t0 is on z=0 plane (horizontal), t1 is vertical
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 0, 0), 0);
        var t1 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(0, 0.5, 1), 0);

        var boundary = BoundaryDetection.GetSharedPoints(t0, t1, checkCoplanar: true);

        // Should return empty because triangles are NOT coplanar
        Assert.That(boundary.first.Count, Is.EqualTo(0));
        Assert.That(boundary.second.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestAreCoplanar()
    {
        // Two triangles on the same plane
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 0, 0), 0);
        var t1 = new Triangle(new Wkx.Point(1, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 1, 0), 0);

        Assert.That(t0.AreCoplanar(t1), Is.True);
    }

    [Test]
    public void TestAreNotCoplanar()
    {
        // One horizontal, one vertical triangle
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 0, 0), 0);
        var t1 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(0, 0.5, 1), 0);

        Assert.That(t0.AreCoplanar(t1), Is.False);
    }
}
