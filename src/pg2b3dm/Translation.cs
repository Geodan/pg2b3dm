using Wkb2Gltf;
using Wkx;

namespace pg2b3dm;
public static class Translation
{
    public static double[] ToEcef(Point center_wgs84)
    {
        double[] translation;
        var v3 = SpatialConverter.GeodeticToEcef((double)center_wgs84.X, (double)center_wgs84.Y, 0);
        translation = [v3.X, v3.Y, v3.Z];

        return translation;
    }
}
