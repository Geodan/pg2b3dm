using System;
using System.Drawing;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

namespace Wkb2Gltf
{
    public static class GlbCreator
    {
        private static Vector4 ColorToVector4(float r, float g, float b)
        {
            return new Vector4(r / 255, g / 255, b / 255, 1);
        }

        private static MaterialBuilder GetMaterial(float r, float g, float b)
        {
            var material = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", ColorToVector4(r, g, b));
            return material;
        }

        public static byte[] GetGlb(TriangleCollection triangles)
        {
            var materialSchuindak = GetMaterial(255, 85, 85);
            var materialMuur = GetMaterial(255, 255, 255);

            var mesh = new MeshBuilder<VertexPositionNormal>("mesh");


            foreach (var triangle in triangles) {
                MaterialBuilder material = null;
                var normal = triangle.GetNormal();
                // todo: find better formulas here
                if (normal.Y > 0 && normal.X > -0.1) {
                    material = materialSchuindak;

                    if (!String.IsNullOrEmpty(triangle.Color)) {
                        var c = ColorTranslator.FromHtml(triangle.Color);
                        material = GetMaterial(c.R, c.G, c.B);
                    }
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


        private static void DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal> mesh)
        {
            var normal = triangle.GetNormal();
            var prim = mesh.UsePrimitive(material);
            prim.AddTriangle(
                new VertexPositionNormal((float)triangle.GetP0().X, (float)triangle.GetP0().Y, (float)triangle.GetP0().Z, normal.X, normal.Y, normal.Z),
                new VertexPositionNormal((float)triangle.GetP1().X, (float)triangle.GetP1().Y, (float)triangle.GetP1().Z, normal.X, normal.Y, normal.Z),
                new VertexPositionNormal((float)triangle.GetP2().X, (float)triangle.GetP2().Y, (float)triangle.GetP2().Z, normal.X, normal.Y, normal.Z)
                );
        }
    }
}
