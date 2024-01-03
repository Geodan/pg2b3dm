using System.Numerics;

namespace B3dm.Tileset;
public static class Matrix
{
    public static Matrix4x4 GetMatrix(Vector3 position, Vector3 eastNormalize, Vector3 northNormalized, Vector3 upNormalize)
    {
        var res = new Matrix4x4();
        res.M11 = eastNormalize.X;
        res.M12 = eastNormalize.Y;
        res.M13 = eastNormalize.Z;
        res.M14 = 0;

        res.M21 = northNormalized.X;
        res.M22 = northNormalized.Y;
        res.M23 = northNormalized.Z;
        res.M24 = 0;

        res.M31 = upNormalize.X;
        res.M32 = upNormalize.Y;
        res.M33 = upNormalize.Z;
        res.M34 = 0;

        res.M41 = position.X;
        res.M42 = position.Y;
        res.M43 = position.Z;
        res.M44 = 1;
        return res;
    }
}