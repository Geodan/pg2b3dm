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


    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double ZMin { get; set; }
    public double ZMax { get; set; }
}
