using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using Wkb2Gltf.extensions;
using Wkb2Gltf.Extensions;

namespace Wkb2Gltf;

public static class GlbCreator
{
    public static byte[] GetGlb(List<List<Triangle>> triangles, string copyright = "", bool addOutlines = false, string defaultColor = "#FFFFFF", string defaultMetallicRoughness = "#008000", bool defaultDoubleSided = true, Dictionary<string, List<object>> attributes = null, bool createGltf = false, bool doubleSided=false)
    {
        var materialCache = new MaterialsCache();
        var shader = new Shader();
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = defaultColor, MetallicRoughness = defaultMetallicRoughness };
        var defaultMaterial = MaterialCreator.CreateMaterial(shader, defaultDoubleSided);

        var meshBatchId = new MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>("mesh");
        var meshFeatureIds = new MeshBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty>("mesh");

        foreach (var tri in triangles) {
            foreach (var triangle in tri) {
                MaterialBuilder material;

                if (triangle.Shader != null) {
                    material = materialCache.GetMaterialBuilderByShader(triangle.Shader, doubleSided);
                }
                else {
                    material = defaultMaterial;
                }

                if (createGltf) {
                    DrawTriangleWithFeatureId(triangle, material, meshFeatureIds);
                }
                else {
                    DrawTriangleWithBatchId(triangle, material, meshBatchId);
                }
            }
        }
        var scene = new SceneBuilder();
        if(createGltf) {
            scene.AddRigidMesh(meshFeatureIds, Matrix4x4.Identity);
        }
        else {
            scene.AddRigidMesh(meshBatchId, Matrix4x4.Identity);
        }
        var model = scene.ToGltf2();
        model.Asset.Copyright = copyright;

        var localTransform = new Matrix4x4(
1, 0, 0, 0,
0, 0, -1, 0,
0, 1, 0, 0,
0, 0, 0, 1);
        model.LogicalNodes.First().LocalTransform = new SharpGLTF.Transforms.AffineTransform(localTransform);

        if (addOutlines) {
            CesiumExtensions.RegisterExtensions();

            foreach (var primitive in model.LogicalMeshes[0].Primitives) {
                primitive.AddOutlines();
            }
        }

        if (createGltf && attributes!=null && attributes.Count>0) {
            var ext = model.InitializeMetadataExtension("propertyTable", attributes.First().Value.Count);
            foreach (var attribute in attributes) {
                var type = attribute.Value.FirstOrDefault().GetType();
                // todo: do not cast all these types to float
                if(type == typeof(decimal) || type == typeof(double) || type == typeof(float)) {
                    var list = attribute.Value.ConvertAll(x => Convert.ToSingle(x));
                    model.AddMetadata(ext, attribute.Key, list);
                }
                else if (type == typeof(uint)) {
                    var list = attribute.Value.ConvertAll(x => (uint)x);
                    model.AddMetadata(ext, attribute.Key, list);
                }
                else if (type == typeof(int)) {
                    var list = attribute.Value.ConvertAll(x => (int)x);
                    model.AddMetadata(ext, attribute.Key, list);
                }
                else {
                    var list = attribute.Value.ConvertAll(x => x.ToString());
                    model.AddMetadata(ext, attribute.Key, list);
                }

                // todo add other types?
            }
        }


        var bytes = model.WriteGLB().Array;

        return bytes;
    }

    private static bool DrawTriangleWithBatchId(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty> mesh)
    {
        var normal = triangle.GetNormal();
        var prim = mesh.UsePrimitive(material);
        var vectors = triangle.ToVectors();
        var indices = prim.AddTriangleWithBatchId(vectors, normal, triangle.GetBatchId());
        return indices.Item1 > 0;
    }

    private static bool DrawTriangleWithFeatureId(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty> mesh)
    {
        var normal = triangle.GetNormal();
        var prim = mesh.UsePrimitive(material);
        var vectors = triangle.ToVectors();
        var indices = prim.AddTriangleWithFeatureId(vectors, normal, triangle.GetBatchId());
        return indices.Item1 > 0;
    }

}
