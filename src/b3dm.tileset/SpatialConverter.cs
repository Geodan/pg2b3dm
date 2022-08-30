using System;
using System.Numerics;

namespace B3dm.Tileset
{
    public static class SpatialConverter
    {
        public static Vector3 GeodeticToEcef(double lon, double lat, double alt)
        {
            var ellipsoid = new Ellipsoid();
            double equatorialRadius = ellipsoid.SemiMajorAxis;
            double eccentricity = ellipsoid.Eccentricity;
            double latitudeRadians = Radian.ToRadius(lat);
            double longitudeRadians = Radian.ToRadius(lon);
            double altitudeRadians = alt;
            double num = equatorialRadius / Math.Sqrt(1.0 - Math.Pow(eccentricity, 2.0) * Math.Pow(Math.Sin(latitudeRadians), 2.0));
            double x = (num + altitudeRadians) * Math.Cos(latitudeRadians) * Math.Cos(longitudeRadians);
            double y = (num + altitudeRadians) * Math.Cos(latitudeRadians) * Math.Sin(longitudeRadians);
            double z = ((1.0 - Math.Pow(eccentricity, 2.0)) * num + altitudeRadians) * Math.Sin(latitudeRadians);
            return new Vector3((float)x, (float)y, (float)z);
        }
    }
}
