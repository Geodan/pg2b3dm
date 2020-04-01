using System.Collections.Generic;

namespace B3dm.Tileset
{
    public class GeometricErrorCalculator
    {
        public static double[] GetGeometricErrors(int maxGeometricError, List<int> lods)
        {
            var nrOfLods = lods.Count;

            var res = new List<double>();

            if (lods.Count > 1) {
                var step = maxGeometricError / nrOfLods;

                for (var i = 1; i <= lods.Count; i++) {
                    res.Add(maxGeometricError - i * step);
                }
            }
            else {
                // only 1 leaf so make it 0
                res.Add(0);
            }

            return res.ToArray(); ;
        } 
    }
}
