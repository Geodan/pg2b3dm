using Wkx;

namespace B3dm.Tileset
{
    public class BoundingBoxCalculator
    {
        public static BoundingBox3D TranslateRotateX(BoundingBox3D bb, double[] translation, double rotation)
        {
            var from = TranslateRotateX(bb.FromPoint(), translation, rotation);
            var to = TranslateRotateX(bb.ToPoint(), translation, rotation);
            var bbNew = new BoundingBox3D((double)from.X, (double)from.Y, (double)from.Z, (double)to.X, (double)to.Y, (double)to.Z);
            var boundingBox = bbNew.TransformYToZ();
            return boundingBox;
        }

        public static Point TranslateRotateX(Point p, double[] translation, double rotation)
        {
            var p1 = Translate(p, translation);
            var p2 = RotateX(p1, rotation);
            return p2;
        }

        public static Point RotateXTranslate(Point p, double[] translation, double rotation)
        {
            var rotated = RotateX(p, rotation);
            var p1 = Translate(rotated, translation);
            return p1;
        }

        public static Point Translate(Point p, double[] translation)
        {
            return p.Translate(translation[0], translation[1], translation[2]);
        }

        public static Point RotateX(Point p, double rotation)
        {
            return p.RotateX(rotation);
        }
    }
}
