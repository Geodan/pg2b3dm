﻿namespace Wkb2Gltf;

public class Ellipsoid
{
    public Ellipsoid()
    {
        SemiMajorAxis = 6378137;
        SemiMinorAxis = 6356752.3142478326;
        Eccentricity = 0.081819190837553915;
    }
    public double SemiMajorAxis { get; }
    public double SemiMinorAxis { get; }

    public double Eccentricity { get; }

}
