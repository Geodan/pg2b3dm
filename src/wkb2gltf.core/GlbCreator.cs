using System;
using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

namespace Wkb2Gltf
{
    public static class GlbCreator
    {
        public static List<string> GetColors(TriangleCollection triangles)
        {
            var res = new List<string>();
            foreach (var t in triangles) {
                if (!res.Contains(t.Color)) {
                    res.Add(t.Color);
                }
            }
            return res;
        }


        private static bool IsRoof(Vector3 triangleNormal, Vector3 triangleTop)
        {
            var checkX = Math.Abs(triangleNormal.X*-1 - triangleTop.X) < 0.9;
            var checkY = Math.Abs(triangleNormal.Y*-1 - triangleTop.Y) < 0.8;
            var checkZ = Math.Abs(triangleNormal.Z*-1 - triangleTop.Z) < 0.9;

            if (checkY && checkX && checkZ) {
                return true;
            } 
            return false;
        }

        public static byte[]  GetGlb(TriangleCollection triangles, double[] translation)
        {
            var normal_translation = new Vector3((float)translation[0], (float)translation[2], (float)translation[1]);
            var normalized_translation = Vector3.Normalize(normal_translation);
            var colors = GetColors(triangles);
            var materialCache = new MaterialCache(colors);
            var materialSchuindak = MaterialCache.CreateMaterial(255, 85, 85);
            var materialMuur = MaterialCache.CreateMaterial(255, 255, 255);
            var mesh = new MeshBuilder<VertexPositionNormal>("mesh");
            //mesh.VertexPreprocessor.SetDebugPreprocessors();
            var degenerated_triangles = 0;

            foreach (var triangle in triangles) {
                MaterialBuilder material = null;

                var normal = triangle.GetNormal();

                var isRoof = IsRoof(normal, normalized_translation);

                if (isRoof) {
                    material = materialSchuindak;

                    // material = materialRed;

                    if (!string.IsNullOrEmpty(triangle.Color)) {
                        material = materialCache.GetMaterialBuilderByColor(triangle.Color);
                    }
                }
                else {
                    material = materialMuur;
                }

                var isadded = DrawTriangle(triangle, material, mesh);
                if (!isadded) {
                    degenerated_triangles++;
                }
            }

            var model = ModelRoot.CreateModel();
            model.CreateMeshes(mesh);
            model.UseScene("Default")
                .CreateNode()
                .WithMesh(model.LogicalMeshes[0]);
            var bytes = model.WriteGLB().Array;

            return bytes;
        }


        private static bool DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal> mesh)
        {
            var normal = triangle.GetNormal();
            var prim = mesh.UsePrimitive(material);
            var indices = prim.AddTriangle(
                new VertexPositionNormal((float)triangle.GetP0().X, (float)triangle.GetP0().Y, (float)triangle.GetP0().Z, normal.X, normal.Y, normal.Z),
                new VertexPositionNormal((float)triangle.GetP1().X, (float)triangle.GetP1().Y, (float)triangle.GetP1().Z, normal.X, normal.Y, normal.Z),
                new VertexPositionNormal((float)triangle.GetP2().X, (float)triangle.GetP2().Y, (float)triangle.GetP2().Z, normal.X, normal.Y, normal.Z)
                );
            return indices.Item1 > 0;
        }
    }
}
