using Wkx;

namespace B3dm.Tileset.Extensions;

public static class BoundingBoxExtensions
{
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
        return Radian.ToRadius(angle);
    }

    public static Point GetCenter(this BoundingBox bb, double zmin,  double zmax)
    {
        var x = (bb.XMax + bb.XMin) / 2;
        var y = (bb.YMax + bb.YMin) / 2;
        var z = (zmax + zmin) / 2;
        return new Point(x, y, z);
    }

    public static double[] ToArray(this BoundingBox bb)
    {
        return new double[4] { bb.XMin, bb.YMin, bb.XMax, bb.YMax };
    }
}
