using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using Wkb2Gltf.extensions;
using Wkb2Gltf.outlines;
using Wkx;
using Wkb2Gltf.Extensions;

namespace Wkb2Gltf.Tests.outlines;
public class OutlineDetectionTests
{
    static bool DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty> mesh)
    {
        var normal = triangle.GetNormal();
        var prim = mesh.UsePrimitive(material);
        var vectors = triangle.ToVectors();
        var indices = prim.AddTriangleWithBatchId(vectors, normal, 0);
        return indices.Item1 > 0;
    }

    [Test]
    public void OutlineDetectionTest()
    {
        // arrange
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 0, 0), 0);
        var t1 = new Triangle(new Point(1, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 1);
        var t2 = new Triangle(new Point(10, 0, 0), new Point(10, 1, 0), new Point(11, 1, 0), 2);

        // put them in wrong order
        var triangles = new List<Triangle> { t0, t2, t1 };

        var parts = PartFinder.GetParts(triangles);

        Assert.That(parts.Count == 1, Is.True);
        Assert.That(parts[0].Count == 3, Is.True);
        Assert.That(parts[0][0] == 0, Is.True);
        Assert.That(parts[0][1] == 1, Is.True);
        Assert.That(parts[0][0] == 0, Is.True);

        var outlines = OutlineDetection.GetOutlines2(triangles);
        Assert.That(outlines.Count == 8, Is.True);
    }

    [Test]
    public void FindConnectedTrianglesWithSameNormal()
    {
        // arrange
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 0, 0), 0);
        var t1 = new Triangle(new Point(1, 0, 0), new Point(1, 1, 0), new Point(2, 0, 0), 0);
        var t2 = new Triangle(new Point(1, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 0);

        var triangles = new List<Triangle> { t0, t1, t2 };

        var a = Adjacency.GetAdjacencyList(triangles);
        Assert.That(a.ContainsKey(0), Is.True);
        Assert.That(a[0].Count == 1, Is.True);
        Assert.That(a[1].Count == 1, Is.True);
        Assert.That(a[2].Count == 2, Is.True);


        var outlines = Part.GetOutlines(triangles, new List<uint>() { 0, 1, 2 });
        Assert.That(outlines[0] == 0, Is.True);
        Assert.That(outlines[1] == 1, Is.True);
        Assert.That(outlines[2] == 2, Is.True);
        Assert.That(outlines[3] == 0, Is.True);
        Assert.That(outlines[4] == 4, Is.True);
        Assert.That(outlines[5] == 5, Is.True);
        Assert.That(outlines[6] == 5, Is.True);
        Assert.That(outlines[7] == 3, Is.True);
        Assert.That(outlines[8] == 7, Is.True);
        Assert.That(outlines[9] == 8, Is.True);
    }

}
