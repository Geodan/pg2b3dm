using System.Collections.Generic;
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

        public float[] Flatten()
        {
            var floats = new List<float>();
            floats.AddRange(GetP0().ToArray());
            floats.AddRange(GetP1().ToArray());
            floats.AddRange(GetP2().ToArray());
            return floats.ToArray();
        }
    }
}
