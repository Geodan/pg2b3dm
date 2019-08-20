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
                var colors = g.HexColors;
                var triangles = Triangulator.GetTriangles(surface, colors);
                triangleCollection.AddRange(triangles);
            }

            return triangleCollection;
        }

        public static TriangleCollection GetTriangles(PolyhedralSurface polyhedralsurface, string[] hexColors)
        {
            var degenerated_triangles = 0;
            var allTriangles = new TriangleCollection();
            for(var i=0;i<polyhedralsurface.Geometries.Count;i++) {
                var geometry = polyhedralsurface.Geometries[i];
                Triangle triangle;
                if (hexColors.Length > 0) {
                    if (hexColors.Length == 1) {
                        triangle = GetTriangle(geometry, hexColors[0]);
                    }
                    else {
                        if (hexColors.Length != polyhedralsurface.Geometries.Count) {
                            throw new ArgumentOutOfRangeException($"Expected number of colors: {polyhedralsurface.Geometries.Count}, actual: {hexColors.Length}");
                        }
                        triangle = GetTriangle(geometry, hexColors[i]);
                    }
                }
                else {
                    triangle = GetTriangle(geometry, String.Empty);
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
