using System.Diagnostics;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPositionNormal;

namespace Wkb2Gltf
{
    public static class GlbCreator
    {
        public static byte[] GetGlb(TriangleCollection triangles)
        {

            var materialPlatdak = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", new Vector4(187/255, 187/255, 187/255, 1));

            var materialSchuindak = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", new Vector4(1, 218/255, 153/255, 1));

            var materialMuur = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", new Vector4(1, 1, 1, 1));

            var materialRed = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", new Vector4(1,0,0, 1));

            var materialGreen = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", new Vector4(0, 1, 0, 1));


            var mesh = new MeshBuilder<VERTEX>("mesh");


            foreach (var triangle in triangles) {
                MaterialBuilder material = null;
                var normal = triangle.GetNormal();

                // todo: find better formulas here
                if (normal.Y > 0 && normal.X > -0.1) {
                    material = materialSchuindak;
                }
                else {
                    material = materialMuur;
                }

                DrawTriangle(triangle, material, mesh);
            }

            var model = ModelRoot.CreateModel();
            model.CreateMeshes(mesh);
            model.UseScene("Default")
                .CreateNode()
                .WithMesh(model.LogicalMeshes[0]);
            var bytes = model.WriteGLB().Array;

            return bytes;
        }

        private static void DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VERTEX> mesh)
        {
            var normal = triangle.GetNormal();
            var prim = mesh.UsePrimitive(material);
            prim.AddTriangle(
                new VERTEX((float)triangle.GetP0().X, (float)triangle.GetP0().Y, (float)triangle.GetP0().Z, normal.X, normal.Y, normal.Z),
                new VERTEX((float)triangle.GetP1().X, (float)triangle.GetP1().Y, (float)triangle.GetP1().Z, normal.X, normal.Y, normal.Z),
                new VERTEX((float)triangle.GetP2().X, (float)triangle.GetP2().Y, (float)triangle.GetP2().Z, normal.X, normal.Y, normal.Z)
                );
        }
    }
}
