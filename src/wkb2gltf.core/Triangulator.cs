using System.Collections.Generic;
using Wkx;

namespace Wkb2Gltf
{
    public static class Triangulator
    {
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
            if (geometry.IsEmpty) {
                var z = 0;
            }
            var pnts = geometry.ExteriorRing.Points;
            //if (pnts.Count != 4) {
            //    var p = 0;
            //}
            //     (because triangle), maybe add error handling for this.
            var triangle = new Triangle(pnts[0], pnts[1], pnts[2]);
            return new TriangleCollection() { triangle };
        }
    }
}
