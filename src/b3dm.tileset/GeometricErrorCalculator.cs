using System;

namespace B3dm.Tileset;

public class GeometricErrorCalculator
{
    public static double GetGeometricError(double maxGeometricError, double geometricErrorFactor, int z, int lod = 0)
    {
        var geometricError = maxGeometricError / Math.Pow(geometricErrorFactor, z);

        if (lod>0) {
            geometricError = geometricError / Math.Pow(2, lod);
        }
        return geometricError;
    }
}
