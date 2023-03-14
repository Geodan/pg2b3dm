using System.Collections.Generic;

namespace Wkb2Gltf.outlines;
public static class Triangles
{
    public static List<Triangle> SelectByIndex(List<Triangle> triangles, List<uint> indices)
    {
        var res = new List<Triangle>();
        foreach (var indice in indices) {
            res.Add(triangles[(int)indice]);
        }
        return res;
    }
}
