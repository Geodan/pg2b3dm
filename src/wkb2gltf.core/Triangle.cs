using System;
using System.Numerics;
using Wkx;

namespace Wkb2Gltf
{
    public class Triangle
    {
        private readonly Point p0, p1, p2;
        public Triangle(Point p0, Point p1, Point p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }

        public Point GetP0()
        {
            return p0;
        }

        public Point GetP1()
        {
            return p1;
        }

        public Point GetP2()
        {
            return p2;
        }

        public Vector3 GetNormal()
        {
            var u = p1.Minus(p0);
            var vector_u = new Vector3(u.X, u.Y, u.Z);
            var v = p2.Minus(p0);
            var vector_v = new Vector3(v.X, v.Y, v.Z);
            var c = Vector3.Cross(vector_u, vector_v);
            var n = Vector3.Normalize(c);
            return n;
        }

        public string Color { get; set; }

        public bool IsWellFormed()
        {
            var c01 = !p0.Equals(p1);
            var c02 = !p1.Equals(p2);
            var c03 = !p2.Equals(p0);

            if (c01 && c02 && c03) {
                return true;
            }
            return false;
        }

        public double Area()
        {
            double a = p0.DistanceTo(p1);
            double b = p1.DistanceTo(p2);
            double c = p2.DistanceTo(p1);
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }
    }

}

public static class PointExt
{
    public static double DistanceTo(this Point p, Point other)
    {
        var deltax = (p.X - other.X).Value;
        var deltay = (p.Y - other.Y).Value;
        var deltaz = (p.Z - other.Z).Value;
        var distance = Math.Sqrt(
            (deltax * deltax) +
            (deltay * deltay) +
            (deltaz * deltaz));
        return distance;
    }
}
