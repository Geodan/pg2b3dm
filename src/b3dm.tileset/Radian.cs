using System;

namespace B3dm.Tileset;

public static class Radian
{
    public static double ToRadius(double degrees)
    {
        double radians = (Math.PI / 180) * degrees;
        return (radians);
    }
}
