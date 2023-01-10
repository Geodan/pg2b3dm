using System.Collections.Generic;
using Wkb2Gltf;

namespace pg2b3dm;

public static class B3dmWriter
{
    public static byte[] ToB3dm(List<GeometryRecord> geometries, string copyright="")
    {
        var triangleCollection = GetTriangles(geometries);

        var attributes = GetAttributes(geometries);

        var b3dm = B3dmCreator.GetB3dm(attributes, triangleCollection, copyright);

        var bytes = b3dm.ToBytes();
        return bytes;
    }

    private static Dictionary<string, List<object>> GetAttributes(List<GeometryRecord> geometries)
    {
        var res = new Dictionary<string, List<object>>();

        foreach (var geom in geometries) {
            foreach (var attr in geom.Attributes) {
                if (!res.ContainsKey(attr.Key)) {
                    res.Add(attr.Key, new List<object> { attr.Value });
                }
                else {
                    res[attr.Key].Add(attr.Value);
                }
            }
        }
        return res;
    }

    private static List<Triangle> GetTriangles(List<GeometryRecord> geomrecords)
    {
        var triangleCollection = new List<Triangle>();
        foreach (var g in geomrecords) {
            var triangles = g.GetTriangles();
            triangleCollection.AddRange(triangles);
        }

        return triangleCollection;
    }
}
