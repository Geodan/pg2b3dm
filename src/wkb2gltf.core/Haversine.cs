using System;

namespace Wkb2Gltf;
public static class Haversine
{
    public static (double dx, double dy) GetDistances(double p0_lon, double p0_lat, double center_lon, double center_lat)
    {
        var x = Distance(center_lon, center_lat, p0_lon, center_lat) * 1000;
        var y = Distance(center_lon, center_lat, center_lon, p0_lat) * 1000;
        if (p0_lon < center_lon) {
            x = -x;
        }
        if (p0_lat < center_lat) {
            y = -y;
        }
        return (x, y);
    }

    private static double Distance(double lon1, double lat1, double lon2, double lat2)
    {
        double R = 6371;

        var dLat = ToRadian(lat2 - lat1);
        var dLon = ToRadian(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        var d = R * c;

        return d;
    }

    private static double ToRadian(double val)
    {
        return Math.PI / 180 * val;
    }

}
