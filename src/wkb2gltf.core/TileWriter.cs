using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.Parametric;
using Wkb2Gltf;

namespace pg2b3dm;

using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPosition;

public static class TileWriter
{
    public static byte[] ToTile(List<GeometryRecord> geometries, double[] translation = null, double[] scale = null, string copyright = "", bool addOutlines = false, string defaultColor = "#FFFFFF", string defaultMetallicRoughness = "#008000", bool doubleSided = true, bool createGltf = false, bool YAxisUp = true)
    {
        var attributes = GetAttributes(geometries);
        if (geometries.First().Geometry.GeometryType == Wkx.GeometryType.Point) {
            // Todo1: Handle attributes for points
            return GetSpheresGlb(geometries, translation, defaultColor, defaultMetallicRoughness, doubleSided);
        }

        var triangles = GetTriangles(geometries, translation, scale);

        var bytes = TileCreator.GetTile(attributes, triangles, copyright, addOutlines, defaultColor, defaultMetallicRoughness, doubleSided, createGltf, YAxisUp);

        return bytes;
    }

    private static byte[] GetSpheresGlb(List<GeometryRecord> geometries, double[] translation, string defaultColor, string defaultMetallicRoughness, bool doubleSided)
    {
        var shader = new Shader();
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = defaultColor, MetallicRoughness = defaultMetallicRoughness };
        var material1 = MaterialCreator.CreateMaterial(shader, doubleSided);
        var mesh = new MeshBuilder<VERTEX>("mesh");
        var prim = mesh.UsePrimitive(material1);

        foreach (var geometry in geometries) {
            var pnt = (Wkx.Point)geometry.Geometry;
            var trans = new Vector3((float)(pnt.X - translation[0]), (float)(pnt.Y - translation[1]), (float)(pnt.Z - translation[2]));
            var r = geometry.Radius.HasValue ? geometry.Radius.Value : (float)1.0f;
            mesh.AddSphere(material1, r, Matrix4x4.CreateTranslation(trans));
        }

        var sceneBuilder = new SharpGLTF.Scenes.SceneBuilder();
        sceneBuilder.AddRigidMesh(mesh, Matrix4x4.Identity);
        var model = sceneBuilder.ToGltf2();
        var localTransform = new Matrix4x4(
1, 0, 0, 0,
0, 0, -1, 0,
0, 1, 0, 0,
0, 0, 0, 1);
        model.LogicalNodes.First().LocalTransform = new SharpGLTF.Transforms.AffineTransform(localTransform);

        var bytesPoints = model.WriteGLB().Array;

        return bytesPoints;
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

    private static List<List<Triangle>> GetTriangles(List<GeometryRecord> geomrecords, double[] translation, double[] scale)
    {
        var triangles = new List<List<Triangle>>();
        foreach (var g in geomrecords) {
            var geomTriangles = new List<Triangle>() { };

            geomTriangles.AddRange(g.GetTriangles(translation, scale));
            triangles.Add(geomTriangles);
        }

        return triangles;
    }
}
