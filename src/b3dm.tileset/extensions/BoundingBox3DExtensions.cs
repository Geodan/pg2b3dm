using Wkx;

namespace B3dm.Tileset.Extensions;

public static class BoundingBox3DExtensions
{
    public static double[] ToRegion(this BoundingBox3D bbox, bool keepProjection = false)
    {
        if (keepProjection) {
            // For local coordinate systems, we can't use region (lat/lon)
            // This should use box instead, but for now return basic region
            return new double[] { bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax, bbox.ZMin, bbox.ZMax };
        }
        else {
            // Convert to radians for WGS84
            var bbox2d = new BoundingBox(bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax);
            var radians = bbox2d.ToRadians();
            return new double[] { radians.XMin, radians.YMin, radians.XMax, radians.YMax, bbox.ZMin, bbox.ZMax };
        }
    }
}
