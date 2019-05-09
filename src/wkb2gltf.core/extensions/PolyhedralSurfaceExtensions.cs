using Wkx;

namespace Wkb2Gltf
{
    public static class PolyhedralSurfaceExtensions
    {
        public static BoundingBox3D GetBoundingBox3D(this PolyhedralSurface surface)
        {
            var bb = new BoundingBox3D();
            bb.XMax = -double.MaxValue;
            bb.YMax = -double.MaxValue;
            bb.ZMax = -double.MaxValue;

            bb.XMin = double.MaxValue;
            bb.YMin = double.MaxValue;
            bb.ZMin = double.MaxValue;

            foreach (var geometry in surface.Geometries)
            {
                foreach(var point in geometry.ExteriorRing.Points)
                {
                    if (point.X<bb.XMin)
                    {
                        bb.XMin = (double)point.X;
                    }
                    if (point.Y < bb.YMin)
                    {
                        bb.YMin = (double)point.Y;
                    }
                    if (point.Z < bb.ZMin)
                    {
                        bb.ZMin = (double)point.Z;
                    }

                    if (point.X > bb.XMax)
                    {
                        bb.XMax = (double)point.X;
                    }
                    if (point.Y > bb.YMax)
                    {
                        bb.YMax = (double)point.Y;
                    }
                    if (point.Z > bb.ZMax)
                    {
                        bb.ZMax = (double)point.Z;
                    }
                }
            }
            return bb;
        }
    }
}
