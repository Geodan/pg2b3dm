using System;
using System.Collections.Generic;
using Wkx;

namespace Wkb2Gltf
{
    public static class Triangulator
    {
        public static TriangleCollection GetTriangles(List<GeometryRecord> geomrecords)
        {
            var triangleCollection = new TriangleCollection();
            foreach (var g in geomrecords) {
                var surface = (PolyhedralSurface)g.Geometry;
                var color = g.HexColor;
                var triangles = Triangulator.GetTriangles(surface, color);
                triangleCollection.AddRange(triangles);
            }

            return triangleCollection;
        }

        public static TriangleCollection GetTriangles(PolyhedralSurface polyhedralsurface, string hexColor="")
        {
            var geometries = polyhedralsurface.Geometries;
            var allTriangles = GetTriangles(geometries, hexColor);
            return allTriangles;
        }

        public static TriangleCollection GetTriangles(List<Polygon> geometries, string hexColor = "")
        {
            var degenerated_triangles = 0;
            var allTriangles = new TriangleCollection();
            foreach (var geometry in geometries) {
                var triangle = GetTriangle(geometry, hexColor);
                if (triangle != null) {
                    allTriangles.Add(triangle);
                }
                else {
                    degenerated_triangles++;
                }
            }

            return allTriangles;
        }

        public static Triangle GetTriangle(Polygon geometry, string hexColor = "")
        {
            var pnts = geometry.ExteriorRing.Points;
            if (pnts.Count != 4) {
                throw new ArgumentOutOfRangeException($"Expected number of vertices in triangles: 4, actual: {pnts.Count}");
            }

            var triangle = new Triangle(pnts[0], pnts[1], pnts[2]);
            if (hexColor != string.Empty) {
                triangle.Color = hexColor;
            }

            if (!triangle.IsDegenerated()) {
                return triangle;
            }
            return null;
        }
    }
}
