using Wkx;

namespace B3dm.Tileset
{
    public class BoundingBoxCalculator
    {
        public static BoundingBox3D RotateTranslateTransform(BoundingBox3D bb, double[] translation, double rotation)
        {
            var from = RotateXAndTranslate(bb.FromPoint(), translation, rotation);
            var to = RotateXAndTranslate(bb.ToPoint(), translation, rotation);
            var bbNew = new BoundingBox3D((double)from.X, (double)from.Y, (double)from.Z, (double)to.X, (double)to.Y, (double)to.Z);
            var boundingBox = bbNew.TransformYToZ();
            return boundingBox;
        }

        private static Point RotateXAndTranslate(Point p, double[] translation, double rotation)
        {
            var p1 = Translate(p, translation);
            return RotateX(p1, rotation);
        }

        private static Point Translate(Point p, double[] translation)
        {
            return p.Translate(translation[0]*-1, translation[1]*-1, translation[2]*-1);
        }

        private static Point RotateX(Point p, double rotation)
        {
            return p.RotateX(rotation);
        }
    }
}
