using Wkx;

namespace B3dm.Tileset
{
    public static class B3dmTile
    {
        public static BoundingBox GetExtent(BoundingBox extent, double size, int i, int j)
        {
            var xmin = extent.XMin + i * size;
            var ymin = extent.YMin + j * size;
            var xmax = extent.XMin + (i + 1) * size;
            var ymax = extent.YMin + (j + 1) * size;
            return new BoundingBox(xmin, ymin, xmax, ymax);
        }
    }
}
