using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using Wkb2Gltf.extensions;
using Wkb2Gltf.Extensions;

namespace Wkb2Gltf;

public static class GlbCreator
{
    public static byte[] GetGlb(List<List<Triangle>> triangles, string copyright = "", bool addOutlines=false, string defaultColor = "#FFFFFF")
    {
        var materialCache = new MaterialsCache();
        var shader = new Shader();
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = defaultColor, MetallicRoughness = "" };
        var defaultMaterial = MaterialCreator.CreateMaterial(shader);

        var mesh = new MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>("mesh");

        foreach (var tri in triangles) {
            foreach (var triangle in tri) {
                MaterialBuilder material;

                if (triangle.Shader != null) {
                    material = materialCache.GetMaterialBuilderByShader(triangle.Shader);
                }
                else {
                    material = defaultMaterial;
                }

                DrawTriangle(triangle, material, mesh);
            }
        }
        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);
        var model = scene.ToGltf2();
        model.Asset.Copyright = copyright;
        var localTransform = new Matrix4x4(
1, 0, 0, 0,
0, 0, -1, 0,
0, 1, 0, 0,
0, 0, 0, 1);
        model.LogicalNodes.First().LocalTransform = new SharpGLTF.Transforms.AffineTransform(localTransform);

        if (addOutlines) {
            model.LogicalMeshes[0].Primitives[0].AddOutlines(triangles);
        }

        var bytes = model.WriteGLB().Array;

        return bytes;
    }

    private static bool DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty> mesh)
    {
        var normal = triangle.GetNormal();
        var prim = mesh.UsePrimitive(material);
        var vectors = triangle.ToVectors();
        var indices = prim.AddTriangleWithBatchId(vectors, normal, triangle.GetBatchId());
        return indices.Item1 > 0;
    }
}
