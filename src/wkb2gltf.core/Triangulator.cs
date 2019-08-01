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
                var triangles = Triangulator.GetTriangles(surface);
                triangleCollection.AddRange(triangles);
            }

            return triangleCollection;
        }

        public static TriangleCollection GetTriangles(PolyhedralSurface polyhedralsurface)
        {
            var geometries = polyhedralsurface.Geometries;
            var allTriangles = GetTriangles(geometries);
            return allTriangles;
        }

        public static TriangleCollection GetTriangles(List<Polygon> geometries)
        {
            var allTriangles = new TriangleCollection();
            foreach (var geometry in geometries) {
                var triangles = GetTriangles(geometry);
                allTriangles.AddRange(triangles);
            }

            return allTriangles;
        }

        public static TriangleCollection GetTriangles(Polygon geometry)
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
            return new TriangleCollection() { triangle };
        }
    }
}
