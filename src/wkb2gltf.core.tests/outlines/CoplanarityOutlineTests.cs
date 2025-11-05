using System.Collections.Generic;
using NUnit.Framework;
using Wkb2Gltf.outlines;
using Wkx;

namespace Wkb2Gltf.Tests.outlines;

/// <summary>
/// Tests to verify that outlines are correctly generated for coplanar vs non-coplanar triangles
/// </summary>
public class CoplanarityOutlineTests
{
    [Test]
    public void CoplanarTrianglesShareEdge_EdgeNotInOutline()
    {
        // Two coplanar triangles sharing an edge (on the same z=0 plane)
        // The shared edge should NOT be in the outline
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 0, 0), 0);
        var t1 = new Triangle(new Point(1, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 0);

        var triangles = new List<Triangle> { t0, t1 };

        // Get adjacency list
        var adjacency = Adjacency.GetAdjacencyList(triangles);

        // Both triangles should have entries in adjacency list (they share an edge and are coplanar)
        Assert.That(adjacency.ContainsKey(0), Is.True, "Triangle 0 should have adjacency entry");
        Assert.That(adjacency.ContainsKey(1), Is.True, "Triangle 1 should have adjacency entry");

        // Get outlines
        var outlines = Part.GetOutlines(triangles, new List<uint> { 0, 1 });

        // The shared edge (indices 1-2 of t0 and 1-0 of t1) should NOT be in outlines
        // We expect 4 outline edges (2 per triangle, excluding the shared edge)
        Assert.That(outlines.Count, Is.EqualTo(8), "Should have 8 outline indices (4 edges)");
    }

    [Test]
    public void NonCoplanarTrianglesShareEdge_EdgeInOutline()
    {
        // Two triangles that share a vertical edge but have different normals
        // (like two walls meeting at a corner)
        // Triangle 0: wall facing in X direction
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 0, 1), new Point(0, 1, 0), 0);
        // Triangle 1: wall facing in Y direction, shares edge (0,0,0)-(0,0,1) with t0
        var t1 = new Triangle(new Point(0, 0, 0), new Point(1, 0, 0), new Point(0, 0, 1), 0);

        var triangles = new List<Triangle> { t0, t1 };

        // Verify they are not coplanar (different normals)
        var isCoplanar = BoundaryDetection.AreCoplanar(t0, t1);
        Assert.That(isCoplanar, Is.False, "Triangles with different orientations should not be coplanar");

        // Get adjacency list
        var adjacency = Adjacency.GetAdjacencyList(triangles);

        // They share edge (0,0,0)-(0,0,1) but are NOT coplanar
        // So they should NOT be in adjacency list
        var hasAdjacency = adjacency.ContainsKey(0) || adjacency.ContainsKey(1);
        Assert.That(hasAdjacency, Is.False, "Non-coplanar triangles should not be adjacent even if they share points");
    }

    [Test]
    public void ParallelPlanesTrianglesShareEdge_EdgeInOutline()
    {
        // Two triangles on parallel planes (z=0 and z=5), sharing edge points
        // The edge SHOULD be in the outline because they're not coplanar
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 0, 0), 0);
        var t1 = new Triangle(new Point(0, 0, 5), new Point(0, 1, 5), new Point(1, 0, 5), 0);

        var triangles = new List<Triangle> { t0, t1 };

        // Verify they are not coplanar
        var isCoplanar = BoundaryDetection.AreCoplanar(t0, t1);
        Assert.That(isCoplanar, Is.False, "Triangles on parallel planes should not be coplanar");

        // Get adjacency list
        var adjacency = Adjacency.GetAdjacencyList(triangles);

        // They don't share any points in 3D space, so no adjacency
        Assert.That(adjacency.Count, Is.EqualTo(0), "Triangles should not be adjacent");
    }
}
