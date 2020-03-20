using System.Collections.Generic;
using System;
using Wkx;

namespace B3dm.Tileset
{
    public class BoundingBoxCalculator
    {
        public static BoundingBox3D GetBoundingAll(BoundingBox3D bb, double[] translation)
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

        public static BoundingBox3D GetBoundingBox(List<BoundingBox3D> boxes)
        {
            var xmin = double.MaxValue;
            var ymin = double.MaxValue;
            var zmin = double.MaxValue;
            var xmax = double.MinValue;
            var ymax = double.MinValue;
            var zmax = double.MinValue;


            foreach (var box in boxes) {
                xmin = box.XMin < xmin ? box.XMin : xmin;
                ymin = box.YMin < ymin ? box.YMin : ymin;
                zmin = box.ZMin < zmin ? box.ZMin : zmin;
                xmax = box.XMax > xmax ? box.XMax : xmax;
                ymax = box.YMax > ymax ? box.YMax : ymax;
                zmax = box.ZMax > zmax ? box.ZMax : zmax;
            }

            return new BoundingBox3D(xmin, ymin, zmin, xmax, ymax, zmax);
        }
    }
}
