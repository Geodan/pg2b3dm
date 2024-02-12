using System;
using System.Collections.Generic;
using Gavaghan.Geodesy;
using Triangulate;
using Wkx;

namespace Wkb2Gltf;

public static class GeometryProcessor
{
    public static List<Triangle> GetTriangles(Geometry geometry, int batchId, double[] translation, ShaderColors shadercolors = null)
    {
        if (geometry is not Polygon && geometry is not MultiPolygon && geometry is not PolyhedralSurface) {
            throw new NotSupportedException($"Geometry type {geometry.GeometryType} is not supported");
        }
        var geometries = GetGeometries(geometry);

        var isTriangulated = IsTriangulated(geometries);

        var relativePolygons = GetRelativePolygons(geometries, translation);
        var m1 = new MultiPolygon(relativePolygons);

        if (!isTriangulated) {
            var triangles = Triangulator.Triangulate(m1);
            geometries = ((MultiPolygon)triangles).Geometries;
        }
        else {
            geometries = m1.Geometries;
        }


        return GetTriangles(batchId, shadercolors, geometries);

    }

    private static List<Polygon> GetGeometries(Geometry geometry)
    {
        // return the Geometries properties of the geometry, for polygon, multipolygon and polyhedral surface
        if (geometry is Polygon) {
            return new List<Polygon>() { (Polygon)geometry };
        }
        else if (geometry is MultiPolygon) {
            return ((MultiPolygon)geometry).Geometries;
        }
        else if (geometry is PolyhedralSurface) {
            return ((PolyhedralSurface)geometry).Geometries;
        }
        else {
            throw new NotSupportedException($"Geometry type {geometry.GeometryType} is not supported");
        }
    }

    private static List<Polygon> GetRelativePolygons(List<Polygon> geometries, double[] translation)
    {
        var relativePolygons = new List<Polygon>();

        foreach (var geometry in geometries) {
            var exteriorRing = new LinearRing();
            var interiorRings = new List<LinearRing>();
            foreach (var point in geometry.ExteriorRing.Points) {
                var relativePoint = ToRelativePoint(point, translation);
                exteriorRing.Points.Add(relativePoint);
            }

            foreach(var interiorRing in geometry.InteriorRings) {
                var relativeInteriorRing = new LinearRing();
                foreach (var point in interiorRing.Points) {
                    var relativePoint = ToRelativePoint(point, translation);
                    relativeInteriorRing.Points.Add(relativePoint);
                }
                interiorRings.Add(relativeInteriorRing);
            }
            var relativePolygon = new Polygon(exteriorRing, interiorRings);
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

    private static Point ToRelativePoint(Point pnt, double[] translation)
    {
        var reference = Ellipsoid.WGS84;
        var geoCalc = new GeodeticCalculator();

        var trans1 = new GlobalCoordinates(Angle.FromDegrees(translation[1]), Angle.FromDegrees(translation[0]));
        var pnt1 = new GlobalCoordinates(Angle.FromDegrees(translation[1]), Angle.FromDegrees((double)pnt.X));
        var distance_x = geoCalc.CalculateGeodeticCurve(reference, pnt1, trans1).EllipsoidalDistanceMeters;

        var trans2 = new GlobalCoordinates(Angle.FromDegrees(translation[1]), Angle.FromDegrees(translation[0]));
        var pnt2 = new GlobalCoordinates(Angle.FromDegrees((double)pnt.Y), Angle.FromDegrees(translation[0]));
        var distance_y = geoCalc.CalculateGeodeticCurve(reference, pnt2, trans2).EllipsoidalDistanceMeters;

        if (pnt.X < translation[0]) {
            distance_x = -distance_x;
        }
        if (pnt.Y < translation[1]) {
            distance_y = -distance_y;
        }

        return new Point(distance_x, distance_y, pnt.Z);
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
