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
            var allTriangles = new TriangleCollection();
            foreach (var geometry in geometries) {
                var triangles = GetTriangles(geometry, hexColor);
                allTriangles.AddRange(triangles);
            }

            return allTriangles;
        }

        public static TriangleCollection GetTriangles(Polygon geometry, string hexColor = "")
        {
            var pnts = geometry.ExteriorRing.Points;
            //if (pnts.Count != 4) {
            //    var p = 0;
            //}
            //     (because triangle), maybe add error handling for this.

            //if (pnts[1].Equals(pnts[2])){
            //    var p = 0;
            //}
            var triangle = new Triangle(pnts[0], pnts[1], pnts[2]);
            if (hexColor != string.Empty) {
                triangle.Color = hexColor;
            }
            return new TriangleCollection() { triangle };
        }
    }
}
