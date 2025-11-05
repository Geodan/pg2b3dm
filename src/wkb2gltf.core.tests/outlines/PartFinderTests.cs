using System.Collections.Generic;
using NUnit.Framework;
using Wkb2Gltf.outlines;
using Wkx;

namespace Wkb2Gltf.Tests.outlines;
public class PartFinderTests
{
    [Test]
    public void PartFinderTest2()
    {
        var t0 = new Triangle(new Point(223785.734375, -389497.78125, -221034.46875), new Point(223779.40625, -389497.75, -221027.78125), new Point(223786.34375, -389490.84375, -221034.890625), 0);
        var t1 = new Triangle(new Point(223785.515625, -389490.8125, -221034.015625), new Point(223786.34375, -389490.84375, -221034.890625), new Point(223779.46875, -389490.65625, -221027.609375), 1);

        var triangles = new List<Triangle>() { t0, t1};
        var parts = PartFinder.GetParts(triangles, 0.1);
        // These two triangles only share a single vertex, not an edge, so they should be 2 separate parts
        Assert.That(parts.Count, Is.EqualTo(2));
        Assert.That(parts[0].Count, Is.EqualTo(1));
        Assert.That(parts[1].Count, Is.EqualTo(1));
    }

    [Test]
    public void PartFinderFirstTest()
    {
        // arrange
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 0, 0), 0);
        var t1 = new Triangle(new Point(1, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 1);
        var t2 = new Triangle(new Point(5, 0, 0), new Point(5, 1, 0), new Point(5, 5, 0), 2);

        var triangles = new List<Triangle>() { t0, t1, t2 };

        // Now we check both normal AND connectivity, so we expect 2 parts:
        // - Part 0: t0 and t1 (connected, same normal)
        // - Part 1: t2 (separate, same normal)
        var parts = PartFinder.GetParts(triangles);

        Assert.That(parts.Count, Is.EqualTo(2));
        Assert.That(parts[0].Count, Is.EqualTo(2));
        Assert.That(parts[1].Count, Is.EqualTo(1));
    }

    [Test]
    public void PartFinderDisconnectedFacesWithSameNormal()
    {
        // This test simulates a building with two separate vertical faces
        // Both faces have the same normal (pointing in the same direction)
        // but they are physically disconnected
        
        // Face 1: Two triangles forming a square (0,0,0) to (2,0,2)
        var face1_t0 = new Triangle(new Point(0, 0, 0), new Point(2, 0, 0), new Point(0, 0, 2), 0);
        var face1_t1 = new Triangle(new Point(2, 0, 0), new Point(2, 0, 2), new Point(0, 0, 2), 1);
        
        // Face 2: Two triangles forming a square (5,0,0) to (7,0,2) - same normal, different location
        var face2_t0 = new Triangle(new Point(5, 0, 0), new Point(7, 0, 0), new Point(5, 0, 2), 2);
        var face2_t1 = new Triangle(new Point(7, 0, 0), new Point(7, 0, 2), new Point(5, 0, 2), 3);
        
        var triangles = new List<Triangle>() { face1_t0, face1_t1, face2_t0, face2_t1 };
        
        var parts = PartFinder.GetParts(triangles);
        
        // Should create 2 parts: one for each connected face
        Assert.That(parts.Count, Is.EqualTo(2));
        
        // Each part should contain 2 triangles
        Assert.That(parts[0].Count, Is.EqualTo(2));
        Assert.That(parts[1].Count, Is.EqualTo(2));
    }

    [Test]
    public void PartFinderComplexBuilding()
    {
        // Simulates a building with multiple faces of different orientations
        // Front face (2 triangles, normal pointing in +Y direction)
        var front_t0 = new Triangle(new Point(0, 0, 0), new Point(1, 0, 0), new Point(0, 0, 1), 0);
        var front_t1 = new Triangle(new Point(1, 0, 0), new Point(1, 0, 1), new Point(0, 0, 1), 1);
        
        // Side face (2 triangles, normal pointing in +X direction)
        var side_t0 = new Triangle(new Point(1, 0, 0), new Point(1, 1, 0), new Point(1, 0, 1), 2);
        var side_t1 = new Triangle(new Point(1, 1, 0), new Point(1, 1, 1), new Point(1, 0, 1), 3);
        
        // Another front-facing face, disconnected from first (same normal as front)
        var front2_t0 = new Triangle(new Point(5, 0, 0), new Point(6, 0, 0), new Point(5, 0, 1), 4);
        var front2_t1 = new Triangle(new Point(6, 0, 0), new Point(6, 0, 1), new Point(5, 0, 1), 5);
        
        var triangles = new List<Triangle>() { front_t0, front_t1, side_t0, side_t1, front2_t0, front2_t1 };
        
        var parts = PartFinder.GetParts(triangles);
        
        // Should create 3 parts:
        // - Part 0: front face (2 triangles)
        // - Part 1: side face (2 triangles, different normal)
        // - Part 2: second front face (2 triangles, same normal as part 0 but disconnected)
        Assert.That(parts.Count, Is.EqualTo(3));
    }
}
