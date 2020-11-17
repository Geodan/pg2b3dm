using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using Wkb2Gltf.Extensions;

namespace Wkb2Gltf
{
    public static class GlbCreator
    {
        public static byte[] GetGlb(List<Triangle> triangles)
        {
            var materialCache = new MaterialsCache();
            var default_hex_color = "#D94F33"; // "#bb3333";
            var defaultMaterial = MaterialCreator.GetDefaultMaterial(default_hex_color);

            var mesh = new MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>("mesh");

            foreach (var triangle in triangles) {
                MaterialBuilder material;
                if (triangle.Shader!=null) {
                    material = materialCache.GetMaterialBuilderByShader(triangle.Shader);
                }
                else {
                    material = defaultMaterial;
                }

                DrawTriangle(triangle, material, mesh);
            }
            var scene = new SceneBuilder();
            scene.AddRigidMesh(mesh, Matrix4x4.Identity);
            var model = scene.ToGltf2();
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
}
