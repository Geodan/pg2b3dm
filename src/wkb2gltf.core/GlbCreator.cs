using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace Wkb2Gltf
{
    public static class GlbCreator
    {
        public static byte[] GetGlb(TriangleCollection triangles)
        {
            var materialCache = new MaterialsCache();
            var default_hex_color = "#bb3333";
            var defaultMaterial = materialCache.GetMaterialBuilderByColor(default_hex_color);

            var mesh = new MeshBuilder<VertexPositionNormal>("mesh");

            foreach (var triangle in triangles) {
                MaterialBuilder material;
                if (!string.IsNullOrEmpty(triangle.Color)) {
                    material = materialCache.GetMaterialBuilderByColor(triangle.Color);
                }
                else {
                    material = defaultMaterial;
                }

                DrawTriangle(triangle, material, mesh);
            }
            var scene = new SceneBuilder();
            scene.AddMesh(mesh, Matrix4x4.Identity);
            var model = scene.ToSchema2();
            var bytes = model.WriteGLB().Array;

            return bytes;
        }


        private static bool DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal> mesh)
        {
            var normal = triangle.GetNormal();
            var prim = mesh.UsePrimitive(material);
            var vectors = triangle.ToVectors();

            var indices = prim.AddTriangle(
                new VertexPositionNormal((float)triangle.GetP0().X, (float)triangle.GetP0().Y, (float)triangle.GetP0().Z, normal.X, normal.Y, normal.Z),
                new VertexPositionNormal((float)triangle.GetP1().X, (float)triangle.GetP1().Y, (float)triangle.GetP1().Z, normal.X, normal.Y, normal.Z),
                new VertexPositionNormal((float)triangle.GetP2().X, (float)triangle.GetP2().Y, (float)triangle.GetP2().Z, normal.X, normal.Y, normal.Z)
                );
            return indices.Item1 > 0;
        }
    }
}
