using System.Collections.Generic;
using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests;

public class SurfaceTypesTests
{
    [Test]
    public void ParseSurfaceTypes_ValidInput_ReturnsValues()
    {
        var result = GeometryProcessor.ParseSurfaceTypes("4:1,2,2,0", 4);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(new[] { 1, 2, 2, 0 }));
    }

    [Test]
    public void ParseSurfaceTypes_ValidInputWithParentheses_ReturnsValues()
    {
        // PostgreSQL often renders this as a composite/record-like text value, e.g. "(13:0,2,2,...)"
        var result = GeometryProcessor.ParseSurfaceTypes("(4:1,2,2,0)", 4);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(new[] { 1, 2, 2, 0 }));
    }

    [Test]
    public void ParseSurfaceTypes_EmptyString_ReturnsNull()
    {
        Assert.That(GeometryProcessor.ParseSurfaceTypes("", 4), Is.Null);
        Assert.That(GeometryProcessor.ParseSurfaceTypes(null, 4), Is.Null);
    }

    [Test]
    public void ParseSurfaceTypes_MismatchedDeclaredCount_ReturnsNull()
    {
        // declared count (3) does not match number of values (4)
        Assert.That(GeometryProcessor.ParseSurfaceTypes("3:1,2,2,0", 4), Is.Null);
    }

    [Test]
    public void ParseSurfaceTypes_MismatchedExpectedCount_ReturnsNull()
    {
        // declared/value count (4) does not match the actual number of polygons (5)
        Assert.That(GeometryProcessor.ParseSurfaceTypes("4:1,2,2,0", 5), Is.Null);
    }

    [Test]
    public void ParseSurfaceTypes_MalformedValue_ReturnsNull()
    {
        Assert.That(GeometryProcessor.ParseSurfaceTypes("4:1,x,2,0", 4), Is.Null);
    }

    [Test]
    public void ParseSurfaceTypes_MissingColon_ReturnsNull()
    {
        Assert.That(GeometryProcessor.ParseSurfaceTypes("1,2,2,0", 4), Is.Null);
    }

    [Test]
    public void GetTriangles_WithSurfaces_AssignsSurfaceIdPerPolygon()
    {
        // Two unit-triangle polygons (as a MultiPolygon), each producing exactly 1 triangle.
        var wkt = "MULTIPOLYGON Z (((0 0 0,0 0 1,0 1 0,0 0 0)),((0 0 0,0 1 0,1 1 0,0 0 0)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        var triangles = GeometryProcessor.GetTriangles(g, batchId: 0, surfaces: "2:1,2");

        Assert.That(triangles.Count, Is.EqualTo(2));
        Assert.That(triangles[0].SurfaceId, Is.EqualTo(1));
        Assert.That(triangles[1].SurfaceId, Is.EqualTo(2));
    }

    [Test]
    public void GetTriangles_WithoutSurfaces_SurfaceIdStaysNull()
    {
        var wkt = "MULTIPOLYGON Z (((0 0 0,0 0 1,0 1 0,0 0 0)),((0 0 0,0 1 0,1 1 0,0 0 0)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        var triangles = GeometryProcessor.GetTriangles(g, batchId: 0);

        Assert.That(triangles.Count, Is.EqualTo(2));
        foreach (var triangle in triangles) {
            Assert.That(triangle.SurfaceId, Is.Null);
        }
    }

    [Test]
    public void GetTriangles_WithMalformedSurfaces_FallsBackToNullSurfaceId()
    {
        var wkt = "MULTIPOLYGON Z (((0 0 0,0 0 1,0 1 0,0 0 0)),((0 0 0,0 1 0,1 1 0,0 0 0)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        // declared count (3) does not match the actual number of polygons (2) -> fail-safe null
        var triangles = GeometryProcessor.GetTriangles(g, batchId: 0, surfaces: "3:1,2,0");

        Assert.That(triangles.Count, Is.EqualTo(2));
        foreach (var triangle in triangles) {
            Assert.That(triangle.SurfaceId, Is.Null);
        }
    }
}
