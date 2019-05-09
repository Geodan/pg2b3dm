using Wkb2Gltf;
using Wkx;

namespace B3dm.Tileset
{
    public static class BoundingBoxExtensions
    {
        public static bool Inside(this BoundingBox bb, Point point)
        {
            var xinside = (bb.XMin <= point.X) && (bb.XMax > point.X);
            var yinside = (bb.YMin <= point.Y) && (bb.YMax > point.Y);
            return (xinside && yinside);
        }

        public static bool Inside(this BoundingBox bb, BoundingBox3D box3d)
        {
            var min_point = new Point(box3d.XMin, box3d.YMin, box3d.ZMin);
            var min_is_inside = bb.Inside(min_point);
            var max_point = new Point(box3d.XMax, box3d.YMax, box3d.ZMax);
            var max_is_inside = bb.Inside(max_point);
            return (min_is_inside && max_is_inside);
        }

    }
}
