using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wkb2Gltf.outlines;
using Wkx;

namespace Wkb2Gltf.Tests.outlines;
public class PrimitiveOutlineTests
{
    [Test]
    public void TwoBlocksGeometryTest()
    {
        // arrange
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 0);
        var t1 = new Triangle(new Point(0, 0, 0), new Point(1, 1, 0), new Point(2, 0, 0), 1);
        var t2 = new Triangle(new Point(2, 0, 0), new Point(1, 1, 0), new Point(2, 2, 0), 2);
        var t3 = new Triangle(new Point(1, 1, 0), new Point(1, 2, 0), new Point(2, 2, 0), 3);

        var triangles = new List<Triangle> { t0, t1, t2, t3 };

        // act
        var outlines = Part.GetOutlines(triangles, new List<uint>() { 0, 1, 2, 3 });

        // assert
        Assert.IsTrue(outlines.Count == 6 * 2);
        Assert.IsTrue(outlines.SequenceEqual(new List<uint>() { 0, 1, 1, 2, 5, 3, 8, 6, 9, 10, 10, 11 }));
    }

    [Test]
    public void SingleTriangleAreaTest()
    {
        // arrange
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 0);
        var triangles = new List<Triangle> { t0 };

        // act
        var outlines = Part.GetOutlines(triangles, new List<uint>() { 0 });

        // asert
        Assert.IsTrue(outlines.Count == 6);
        Assert.IsTrue(outlines.SequenceEqual(new List<uint>() { 0, 1, 1, 2, 2, 0 }));
    }

    [Test]
    public void RectangleAreaTest()
    {
        // arrange
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 0);
        var t1 = new Triangle(new Point(1, 1, 0), new Point(1, 0, 0), new Point(0, 0, 0), 1);
        var triangles = new List<Triangle>() { t0, t1 };

        // act
        var outlines = Part.GetOutlines(triangles, new List<uint>() { 0, 1 });

        // assert
        Assert.IsTrue(outlines.Count == 8);
        Assert.IsTrue(outlines.SequenceEqual(new List<uint>() { 0, 1, 1, 2, 3, 4, 4, 5 }));
    }

    [Test]
    public void TripleTriangleTest()
    {
        // arrange
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 0);
        var t1 = new Triangle(new Point(1, 1, 0), new Point(1, 0, 0), new Point(0, 0, 0), 1);
        var t2 = new Triangle(new Point(1, 0, 0), new Point(1, 1, 0), new Point(2, 0, 0), 2);
        var triangles = new List<Triangle>() { t0, t1, t2 };

        // act
        var outlines = Part.GetOutlines(triangles, new List<uint> { 0, 1, 2 });

        // assert
        Assert.IsTrue(outlines.Count == 10);
    }

    [Test]
    public void CubeTriangleTest()
    {
        // arrange
        var t0 = new Triangle(new Point(-1, -1, 1), new Point(1, -1, 1), new Point(1, 1, 1), 0);
        var t1 = new Triangle(new Point(-1, -1, 1), new Point(1, 1, 1), new Point(-1, 1, 1), 1);
        var t2 = new Triangle(new Point(1, -1, 1), new Point(1, -1, -1), new Point(1, 1, -1), 2);
        var t3 = new Triangle(new Point(1, -1, 1), new Point(1, 1, -1), new Point(1, 1, 1), 3);
        var t4 = new Triangle(new Point(1, -1, -1), new Point(-1, -1, -1), new Point(-1, 1, -1), 4);
        var t5 = new Triangle(new Point(1, -1, -1), new Point(-1, 1, -1), new Point(1, 1, -1), 5);
        var t6 = new Triangle(new Point(-1, -1, -1), new Point(-1, -1, 1), new Point(-1, 1, 1), 6);
        var t7 = new Triangle(new Point(-1, -1, -1), new Point(-1, 1, 1), new Point(-1, 1, -1), 7);
        var t8 = new Triangle(new Point(-1, 1, 1), new Point(1, 1, 1), new Point(1, 1, -1), 8);
        var t9 = new Triangle(new Point(-1, 1, 1), new Point(1, 1, -1), new Point(-1, 1, -1), 9);
        var t10 = new Triangle(new Point(1, -1, 1), new Point(-1, -1, -1), new Point(1, -1, -1), 10);
        var t11 = new Triangle(new Point(1, -1, 1), new Point(-1, -1, 1), new Point(-1, -1, -1), 11);
        var triangles = new List<Triangle>() { t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11 };

        // act
        var partsOrig = PartFinder.GetParts(triangles);
        var parts2 = PartFinder.GetParts(triangles);

        var parts = OutlineDetection.GetOutlines2(triangles);

        // assert
        Assert.IsTrue(parts.Count == 48);
    }
}
