using System;
using System.Collections.Generic;
using Triangulate;
using Wkx;

namespace Wkb2Gltf;

public static class GeometryProcessor
{
    public static List<Triangle> GetTriangles(Geometry geometry, int batchId, double[] translation, ShaderColors shadercolors = null)
    {
        if (geometry is not Polygon && geometry is not MultiPolygon && geometry is not PolyhedralSurface && geometry is not LineString) {
            throw new NotSupportedException($"Geometry type {geometry.GeometryType} is not supported");
        }

        var geometries = geometry is LineString ? 
            GetTrianglesFromLineString((LineString)geometry, translation) :
            GetTrianglesFromPolygons(geometry, translation);

        var result = GetTriangles(batchId, shadercolors, geometries);

        return result;
    }


    private static List<Polygon> GetTrianglesFromLineString(LineString line, double[] translation)
    {
        var result = new List<Polygon>();
        var relativeLine = GetRelativeLine(line, translation);
        var wkt = relativeLine.AsText();
        // todo: triangulate the line
        // var triangles = Triangulator.Triangulate(relativeLine, radius);
        return result;
    }
    private static List<Polygon> GetTrianglesFromPolygons(Geometry geometry, double[] translation)
    {
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

        return geometries;
    }

    private static List<Polygon> GetGeometries(Geometry geometry)
    {
        var wkt = geometry.AsText();
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

    private static LineString GetRelativeLine(LineString line, double[] translation)
    {
        var result = new LineString();
        foreach(var pnt in line.Points) {
            var relativePoint = ToRelativePoint(pnt, translation);
            result.Points.Add(relativePoint);
        }
        return result;
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
        // var trans = SpatialConverter.GeodeticToEcef((double)pnt.X, (double)pnt.Y, (double)pnt.Z);
        // var res = new Point(translation[0] - trans.X, translation[1] - trans.Y, translation[2] - trans.Z );
        var res = new Point((double)pnt.X - translation[0] , (double)pnt.Y - translation[1], pnt.Z - translation[2]);
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
