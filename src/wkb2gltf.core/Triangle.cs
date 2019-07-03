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
    }
}
