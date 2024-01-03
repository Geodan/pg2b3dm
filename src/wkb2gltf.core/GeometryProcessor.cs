using System;
using System.Collections.Generic;
using Triangulate;
using Wkx;

namespace Wkb2Gltf;

public static class GeometryProcessor
{
    public static List<Triangle> GetTriangles(Geometry geometry, int batchId, ShaderColors shadercolors = null, double areaTolerence = 0.01, Point center = null)
    {
        if (geometry is not MultiPolygon && geometry is not PolyhedralSurface) {
            throw new NotSupportedException($"Geometry type {geometry.GeometryType} is not supported");
        }

        var isMultiPolygon = geometry is MultiPolygon;

        var geometries = isMultiPolygon ?
            ((MultiPolygon)geometry).Geometries :
            ((PolyhedralSurface)geometry).Geometries;


        var isTriangulated = IsTriangulated(geometries);

        if (!isTriangulated) {
            // try to triangulate
            var geometryTriangles = Triangulator.Triangulate(geometry);

            geometries = isMultiPolygon ?
                ((MultiPolygon)geometryTriangles).Geometries :
                ((PolyhedralSurface)geometryTriangles).Geometries;
        }


        return GetTriangles(batchId, shadercolors, areaTolerence, geometries, center);

    }

    private static List<Triangle> GetTriangles(int batchId, ShaderColors shadercolors, double areaTolerence, List<Polygon> geometries, Point center)
    {
        var degenerated_triangles = 0;
        var allTriangles = new List<Triangle>();
        for (var i = 0; i < geometries.Count; i++) {
            var geometry = geometries[i];
            var triangle = GetTriangle(geometry, batchId, center);

            if (triangle != null && shadercolors != null && triangle.Area() > areaTolerence) {
                shadercolors.Validate(geometries.Count);
                triangle.Shader = shadercolors.ToShader(i);
            }

            if (triangle != null) {
                allTriangles.Add(triangle);
            }
            else {
                degenerated_triangles++;
            }
        }

        return allTriangles;
    }

    public static Triangle GetTriangle(Polygon geometry, int batchId, Point center = null)
    {
        var triangle = ToTriangle(geometry, batchId, center);

        if (!triangle.IsDegenerated()) {
            return triangle;
        }
        return null;
    }

    private static Triangle ToTriangle(Polygon geometry, int batchId, Point center = null)
    {
        var pnts = geometry.ExteriorRing.Points;
        if (pnts.Count != 4) {
            throw new ArgumentOutOfRangeException($"Expected number of vertices in triangles: 4, actual: {pnts.Count}");
        }

        Triangle triangle;
        if (center != null) {

            var p0 = ToRelativePoint(pnts[0], center);
            var p1 = ToRelativePoint(pnts[1], center);
            var p2 = ToRelativePoint(pnts[2], center);
            triangle = new Triangle(p0, p1, p2, batchId);
        }
        else {
            triangle = new Triangle(pnts[0], pnts[1], pnts[2], batchId);
        }

        return triangle;
    }

    private static Point ToRelativePoint(Point pnt, Point center)
    {
        var distances = Haversine.GetDistances((double)pnt.X, (double)pnt.Y, (double)center.X, (double)center.Y);
        var res = new Point(distances.dx, distances.dy, pnt.Z);
        return res;
    }

    private static bool IsTriangulated(List<Polygon> polygons)
    {
        foreach (var polygon in polygons) {
            if (polygon.ExteriorRing.Points.Count != 4) {
                return false;
            }
        }
        return true;
    }

}
