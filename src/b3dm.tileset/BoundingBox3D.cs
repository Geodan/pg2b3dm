using System.Globalization;
using Wkx;

namespace B3dm.Tileset;

public class BoundingBox3D
{
    public BoundingBox3D()
    {
    }
    public BoundingBox3D(double XMin, double YMin, double ZMin, double XMax, double YMax, double ZMax)
    {
        this.XMin = XMin;
        this.YMin = YMin;
        this.ZMin = ZMin;
        this.XMax = XMax;
        this.YMax = YMax;
        this.ZMax = ZMax;

    }

    public BoundingBox3D(double[] box)
    {
        var center = new Point(box[0], box[1], box[2]);
        XMin = (double)center.X - box[3];
        YMin = (double)center.Y - box[7];
        ZMin = (double)center.Z - box[11];
        XMax = (double)center.X + box[3];
        YMax = (double)center.Y + box[7];
        ZMax = (double)center.Z + box[11];
    }

    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double ZMin { get; set; }
    public double ZMax { get; set; }

    public Point GetCenter()
    {
        var x = (XMax + XMin) / 2;
        var y = (YMax + YMin) / 2;
        var z = (ZMax + ZMin) / 2;
        return new Point(x, y, z);
    }

    public override string ToString()
    {
        return $"{XMin.ToString(CultureInfo.InvariantCulture)},{YMin.ToString(CultureInfo.InvariantCulture)},{ZMin.ToString(CultureInfo.InvariantCulture)},{XMax.ToString((CultureInfo.InvariantCulture))},{YMax.ToString((CultureInfo.InvariantCulture))},{ZMax.ToString((CultureInfo.InvariantCulture))}";
    }

    public override bool Equals(object other)
    {
        var o_box = (BoundingBox3D)other;
        var xmin = Comparer.IsSimilar(XMin, o_box.XMin);
        var ymin = Comparer.IsSimilar(YMin, o_box.YMin);
        var zmin = Comparer.IsSimilar(ZMin, o_box.ZMin);
        var xmax = Comparer.IsSimilar(XMax, o_box.XMax);
        var ymax = Comparer.IsSimilar(YMax, o_box.YMax);
        var zmax = Comparer.IsSimilar(ZMax, o_box.ZMax);
        return xmin && ymin && zmin && xmax && ymax && zmax;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
