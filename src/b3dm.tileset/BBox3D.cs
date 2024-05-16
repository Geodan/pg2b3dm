using System.Collections.Generic;
using Wkx;

namespace B3dm.Tileset;
public static  class BBox3D
{
    public static double[] GetBoundingBoxPoints(Geometry geometry)
    {
        List<Point> points;
        double z_min;
        double z_max;
        if (geometry is Polygon) {
            // this happens when the z values are the same for all points
            var polygon = (Polygon)geometry;
            points = polygon.ExteriorRing.Points;
            z_min = (double)points[0].Z;
            z_max = z_min + 1;
        }
        else {
            // this happens when the z values are not the same for all points
            var polyhedral = (PolyhedralSurface)geometry;
            points = polyhedral.Geometries[0].ExteriorRing.Points;
            z_min = (double)points[0].Z;
            z_max = (double)polyhedral.Geometries[1].ExteriorRing.Points[0].Z;
        }
        var result = new double[] { (double)points[0].X, (double)points[0].Y, (double)points[2].X, (double)points[2].Y, z_min, z_max };
        return result;
    }
}
