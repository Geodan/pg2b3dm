using System;
using Wkx;

namespace B3dm.Tileset
{
    public static class BoundingBoxCalculatorNew
    {
        public static BoundingBox3D GetBoundingAllNew(BoundingBox3D bb, double[] translation)
        {
            var from = RotateXAndTranslate(bb.FromPoint(), translation);
            var to = RotateXAndTranslate(bb.ToPoint(), translation);
            var bbNew = new BoundingBox3D((double)from.X, (double)from.Y, (double)from.Z, (double)to.X, (double)to.Y, (double)to.Z);
            var boundingBox = bbNew.TransformYToZNew();
            return boundingBox;
        }

        private static Point RotateXAndTranslate(Point p, double[] translation)
        {
            return p.Translate(translation[0] * -1, translation[1] * -1, translation[2] * -1).RotateX(Math.PI / 2);
        }
    }
}
