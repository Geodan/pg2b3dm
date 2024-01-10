using System;
using System.Collections.Generic;
using Triangulate;
using Wkx;

namespace Wkb2Gltf;

public static class GeometryProcessor
{
    public static List<Triangle> GetTriangles(Geometry geometry, int batchId, ShaderColors shadercolors = null, Point center = null)
    {
        if (geometry is not MultiPolygon && geometry is not PolyhedralSurface) {
            throw new NotSupportedException($"Geometry type {geometry.GeometryType} is not supported");
        }

        var isMultiPolygon = geometry is MultiPolygon;

        var geometries = isMultiPolygon ?
            ((MultiPolygon)geometry).Geometries :
            ((PolyhedralSurface)geometry).Geometries;

        var isTriangulated = IsTriangulated(geometries);

        var relativePolygons = GetRelativePolygons(geometries, center);
        var m1 = new MultiPolygon(relativePolygons);

        if (!isTriangulated) {

            if (HasInteriorRings(geometries)) {
                throw new NotSupportedException("Geometries with interior rings are not supported. ");
            }

            var triangles = Triangulator.Triangulate(m1);
            geometries = ((MultiPolygon)triangles).Geometries;
        }
        else {
            geometries = m1.Geometries;
        }


        return GetTriangles(batchId, shadercolors, geometries);

    }

    private static List<Polygon> GetRelativePolygons(List<Polygon> geometries, Point center)
    {
        if (center == null) {
            return geometries;
        }
        var relativePolygons = new List<Polygon>();
        foreach (var geometry in geometries) {
            var linearRing = new LinearRing();
            foreach (var point in geometry.ExteriorRing.Points) {
                var relativePoint = ToRelativePoint(point, center);
                linearRing.Points.Add(relativePoint);
            }

            // todo: add support for interrior rings

            var relativePolygon = new Polygon(linearRing);
            relativePolygons.Add(relativePolygon);
        }

        return relativePolygons;
    }

    private static List<Triangle> GetTriangles(int batchId, ShaderColors shadercolors, List<Polygon> geometries)
    {
        var degenerated_triangles = 0;
        var allTriangles = new List<Triangle>();
        for (var i = 0; i < geometries.Count; i++) {
            var geometry = geometries[i];
            var triangle = GetTriangle(geometry, batchId);

            if (triangle != null && shadercolors != null) {
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

    public static Triangle GetTriangle(Polygon geometry, int batchId)
    {
        var triangle = ToTriangle(geometry, batchId);
        return triangle;
    }

    private static Triangle ToTriangle(Polygon geometry, int batchId)
    {
        var pnts = geometry.ExteriorRing.Points;
        if (pnts.Count != 4) {
            throw new ArgumentOutOfRangeException($"Expected number of vertices in triangles: 4, actual: {pnts.Count}");
        }

        var triangle = new Triangle(pnts[0], pnts[1], pnts[2], batchId);

        return triangle;
    }

    private static Point ToRelativePoint(Point pnt, Point center)
    {
        var distances = Haversine.GetDistances((double)pnt.X, (double)pnt.Y, (double)center.X, (double)center.Y);
        var res = new Point(distances.dx, distances.dy, pnt.Z);
        return res;
    }

    private static bool HasInteriorRings(List<Polygon> polygons)
    {
        foreach (var polygon in polygons) {
            if (polygon.InteriorRings.Count > 0) {
                return true;
            }
        }
        return false;
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
