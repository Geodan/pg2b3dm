﻿using System.Globalization;
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

    public Point FromPoint()
    {
        return new Point(XMin, YMin, ZMin);
    }
    public Point ToPoint()
    {
        return new Point(XMax, YMax, ZMax);
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

    // this one is used by pg2b3dm
    public BoundingBox3D TransformYToZ()
    {
        var res = new BoundingBox3D();
        res.XMin = XMin;
        res.YMin = ZMin;
        res.ZMin = YMin * -1;
        res.XMax = XMax;
        res.YMax = ZMax;
        res.ZMax = YMax * -1;
        return res;
    }

    // this one is used for tranforming for translation + bbox to bbox
    public BoundingBox3D TransformZToY()
    {
        var res = new BoundingBox3D();
        res.XMin = XMin;
        res.YMin = ZMin * -1;
        res.ZMin = YMin;
        res.XMax = XMax;
        res.YMax = ZMax * -1;
        res.ZMax = YMax;
        return res;
    }

    public override string ToString()
    {
        return $"{XMin.ToString(CultureInfo.InvariantCulture)},{YMin.ToString(CultureInfo.InvariantCulture)},{ZMin.ToString(CultureInfo.InvariantCulture)},{XMax.ToString((CultureInfo.InvariantCulture))},{YMax.ToString((CultureInfo.InvariantCulture))},{ZMax.ToString((CultureInfo.InvariantCulture))}";
    }


    public BoundingBox ToBoundingBox()
    {
        return new BoundingBox(XMin, YMin, XMax, YMax);
    }

    public double ExtentX()
    {
        return (XMax - XMin);
    }
    public double ExtentY()
    {
        return (YMax - YMin);
    }
    public double ExtentZ()
    {
        return (ZMax - ZMin);
    }

    public double[] GetBox()
    {
        var center = GetCenter();
        var xAxis = ExtentX() / 2;
        var yAxis = ExtentY() / 2;
        var zAxis = ExtentZ() / 2;

        var result = new double[] { (double)center.X, (double)center.Y, (double)center.Z,
            xAxis,0,0,0,yAxis,0,0,0,zAxis
        };
        return result;
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
