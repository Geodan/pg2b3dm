using Wkx;

namespace B3dm.Tileset
{
    public static class PointExtensions
    {
        public static Point Translate(this Point p, double dx, double dy, double dz)
        {
            return new Point((double)p.X + dx, (double)p.Y + dy, (double)p.Z + dz);
        }

        public static double[] ToVector(this Point p)
        {
            var vector = new double[] { (double)p.X, (double)p.Y, (double)p.Z };

            return vector;
        }

    }
}
