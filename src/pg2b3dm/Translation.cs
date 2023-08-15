using B3dm.Tileset;
using Wkx;

namespace pg2b3dm;
public static class Translation
{
    public static double[] GetTranslation(int sr, Point center_wgs84)
    {
        double[] translation;
        if (sr == 4978) {
            var v3 = SpatialConverter.GeodeticToEcef((double)center_wgs84.X, (double)center_wgs84.Y, 0);
            translation = new double[] { v3.X, v3.Y, v3.Z };
        }
        else {
            translation = SphericalMercator.ToSphericalMercatorFromWgs84((double)center_wgs84.X, (double)center_wgs84.Y);
            translation = new double[] { translation[0], translation[1], 0 };
        }

        return translation;
    }

}
