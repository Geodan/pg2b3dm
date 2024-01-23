using System;
using System.Numerics;

namespace Wkb2Gltf;

public static class SpatialConverter
{
    public static Vector3 GeodeticToEcef(double lon, double lat, double alt)
    {
        var ellipsoid = new Ellipsoid();
        var equatorialRadius = ellipsoid.SemiMajorAxis;
        var eccentricity = ellipsoid.Eccentricity;
        var latitudeRadians = ToRadius(lat);
        var longitudeRadians = ToRadius(lon);
        var altitudeRadians = alt;
        var num = equatorialRadius / Math.Sqrt(1.0 - Math.Pow(eccentricity, 2.0) * Math.Pow(Math.Sin(latitudeRadians), 2.0));
        var x = (num + altitudeRadians) * Math.Cos(latitudeRadians) * Math.Cos(longitudeRadians);
        var y = (num + altitudeRadians) * Math.Cos(latitudeRadians) * Math.Sin(longitudeRadians);
        var z = ((1.0 - Math.Pow(eccentricity, 2.0)) * num + altitudeRadians) * Math.Sin(latitudeRadians);
        return new Vector3((float)x, (float)y, (float)z);
    }

    private static double ToRadius(double degrees)
    {
        double radians = (Math.PI / 180) * degrees;
        return (radians);
    }
}
