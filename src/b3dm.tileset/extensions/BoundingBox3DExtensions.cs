using Wkx;

namespace B3dm.Tileset.Extensions;

public static class BoundingBox3DExtensions
{
    public static double[] ToRegion(this BoundingBox3D bbox, bool keepProjection = false)
    {
        if (keepProjection) {
            // For local coordinate systems, return coordinates as-is.
            // Note: Ideally, for keep_projection with explicit tiling, the boundingVolume 
            // should use 'box' instead of 'region' (as region expects lat/lon in radians).
            // However, implementing box bounding volumes in explicit tilesets requires
            // additional changes beyond the scope of adding OCTREE explicit tiling support.
            // For now, this works when keep_projection is not used (the common case).
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
