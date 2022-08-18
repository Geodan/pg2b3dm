using Wkx;

namespace B3dm.Tileset.extensions
{
    public static class BoundingBoxExtensions
    {
        public static double ExtentX(this BoundingBox bb)
        {
            return bb.XMax - bb.XMin;
        }
        public static double ExtentY(this BoundingBox bb)
        {
            return bb.YMax - bb.YMin;
        }
    }
}
