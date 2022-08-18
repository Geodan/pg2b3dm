using System;
using Wkx;

namespace B3dm.Tileset.extensions
{
    public static class BoundingBoxExtensions
    {
        public static double ExtentX(this BoundingBox bb)
        {
            return bb.XMax - bb.XMin;
        }
        public static double ExtentY(this BoundingBox bb)
        {
            return bb.YMax - bb.YMin;
        }

        public static BoundingBox ToSpherical(this BoundingBox bb)
        {
            var from = SphericalMercator.ToSphericalMercatorFromWgs84(bb.XMin, bb.YMin);
            var to = SphericalMercator.ToSphericalMercatorFromWgs84(bb.XMax, bb.YMax);
            return new BoundingBox(from[0], from[1], to[0], to[1]);
        }

        public static BoundingBox ToLatLon(this BoundingBox bb)
        {
            var from = SphericalMercator.ToWgs84FromSphericalMercator(bb.XMin, bb.YMin);
            var to = SphericalMercator.ToWgs84FromSphericalMercator(bb.XMax, bb.YMax);
            return new BoundingBox(from[0], from[1], to[0], to[1]);
        }

        public static BoundingBox ToRadians(this BoundingBox bb)
        {
            var minx = ConvertToRadians(bb.XMin);
            var miny = ConvertToRadians(bb.YMin);
            var maxx = ConvertToRadians(bb.XMax);
            var maxy = ConvertToRadians(bb.YMax);
            return new BoundingBox(minx, miny, maxx, maxy);
        }

        public static double[] ToRegion(this BoundingBox bb, double minheight, double maxheight)
        {
            return new double[] { bb.XMin, bb.YMin, bb.XMax, bb.YMax, minheight, maxheight };
        }

        private static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}
