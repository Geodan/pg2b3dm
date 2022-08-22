using System;
using System.Numerics;
using CoordinateSharp;
using Wkx;

namespace B3dm.Tileset
{
    public static class EcefConverter
    {
        public static Point Ecef2lla(Vector3 input)
        {
            var ecef = new ECEF(input.X / 1000, input.Y / 1000, input.Z / 1000);
            var c = ECEF.ECEFToLatLong(ecef);
            var h = ConvertToAngle(c.Cartesian.Z);
            return new Point(c.Longitude.DecimalDegree, c.Latitude.DecimalDegree, h);

        }

        private static double ConvertToAngle(double radian)
        {
            return radian * 180 / Math.PI;
        }
    }
}
