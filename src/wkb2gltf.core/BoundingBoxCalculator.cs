using System.Collections.Generic;

namespace Wkb2Gltf
{
    public class BoundingBoxCalculator
    {
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
