using System.Numerics;
using Wkx;

namespace B3dm.Tileset.extensions
{
    public static class BoundingBox3DExtensions
    {
        public static BoundingBox ToWgs84(this BoundingBox3D bbox3d)
        {
            var from = EcefConverter.Ecef2lla(new Vector3((float)bbox3d.XMin, (float)bbox3d.YMin, (float)bbox3d.ZMin));
            var to = EcefConverter.Ecef2lla(new Vector3((float)bbox3d.XMax, (float)bbox3d.YMax, (float)bbox3d.ZMax));

            var fromY = from.Y;
            var toY = to.Y;

            if(fromY> toY) {
                var tmp = fromY;
                fromY = toY;
                toY = tmp;
            }

            return new BoundingBox((double)from.X, (double)fromY, (double)to.X, (double)toY);

        }
    }
}
