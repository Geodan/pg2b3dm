using System.Collections.Generic;
using System.Numerics;
using Wkx;

namespace Wkb2Gltf
{
    public static class PointExtensions
    {
        public static Vector3 Minus(this Point p, Point other)
        {
            var x = p.X - other.X;
            var y = p.Y - other.Y;
            var z = p.Z - other.Z;
            return new Vector3((float)x,(float)y,(float)z);
        }

        public static List<float> ToArray(this Point p)
        {
            var floats = new List<float>();
            floats.Add((float)p.X);
            floats.Add((float)p.Y);
            floats.Add((float)p.Z);
            return floats;
        }
    }
}
