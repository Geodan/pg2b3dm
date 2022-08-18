using System;

namespace pg2b3dm
{
    public static class SphericalMercator
    {
        public static double[] ToSphericalMercatorFromWgs84(double Longitude, double Latitude)
        {
            var x = Longitude * 20037508.34 / 180;
            var y = Math.Log(Math.Tan((90 + Latitude) * Math.PI / 360)) / (Math.PI / 180);
            y = y * 20037508.34 / 180;
            return new double[] { x, y };
        }

        public static double[] ToWgs84FromSphericalMercator(double x, double y)
        {
            var lon = x * 180 / 20037508.34;
            var lat = Math.Atan(Math.Exp(y * Math.PI / 20037508.34)) * 360 / Math.PI - 90;
            return new double[] { lon, lat };
        }
    }
}
