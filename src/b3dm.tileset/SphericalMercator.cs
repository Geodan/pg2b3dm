using System;

namespace B3dm.Tileset.extensions;

public static class SphericalMercator
{
    public static double[] ToSphericalMercatorFromWgs84(double Longitude, double Latitude)
    {
        var x = Longitude * 20037508.34 / 180;
        var y = Math.Log(Math.Tan((90 + Latitude) * Math.PI / 360)) / (Math.PI / 180);
        y = y * 20037508.34 / 180;
        return new double[] { x, y };
    }
}
