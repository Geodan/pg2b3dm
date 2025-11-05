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
    public void TestCoplanarTriangles()
    {
        // Two triangles on the same plane (z=0)
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 0, 0), 0);
        var t1 = new Triangle(new Wkx.Point(1, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 1, 0), 0);

        var isCoplanar = BoundaryDetection.AreCoplanar(t0, t1);

        Assert.That(isCoplanar, Is.True);
    }

    [Test]
    public void TestNonCoplanarTrianglesParallelNormals()
    {
        // Two triangles with same normal but on different planes
        // First triangle on z=0 plane
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 0, 0), 0);
        // Second triangle on z=5 plane (parallel but different plane)
        var t1 = new Triangle(new Wkx.Point(0, 0, 5), new Wkx.Point(0, 1, 5), new Wkx.Point(1, 0, 5), 0);

        var isCoplanar = BoundaryDetection.AreCoplanar(t0, t1);

        Assert.That(isCoplanar, Is.False);
    }

    [Test]
    public void TestNonCoplanarTrianglesDifferentNormals()
    {
        // Two triangles with different normals and different planes
        // First triangle on XY plane (normal pointing up in Z)
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(0, 1, 0), new Wkx.Point(1, 0, 0), 0);
        // Second triangle on XZ plane (normal pointing up in Y)
        var t1 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(1, 0, 0), new Wkx.Point(0, 0, 1), 0);

        var isCoplanar = BoundaryDetection.AreCoplanar(t0, t1);

        Assert.That(isCoplanar, Is.False);
    }

    [Test]
    public void TestDegenerateTriangle()
    {
        // Degenerate triangle (all points on a line, zero normal)
        var t0 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(1, 0, 0), new Wkx.Point(2, 0, 0), 0);
        // Normal triangle
        var t1 = new Triangle(new Wkx.Point(0, 0, 0), new Wkx.Point(1, 0, 0), new Wkx.Point(0, 1, 0), 0);

        // Degenerate triangle should return false for coplanarity
        var isCoplanar = BoundaryDetection.AreCoplanar(t0, t1);

        Assert.That(isCoplanar, Is.False, "Degenerate triangle should not be considered coplanar");
    }
}
