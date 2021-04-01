using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using Wkb2Gltf.Extensions;
using System.IO;
using System;
using System.Diagnostics;
using SharpGLTF.Schema2;
using System.Runtime.InteropServices;

namespace Wkb2Gltf
{
    public static class GlbCreator
    {
        public static byte[] GetGlb(List<Triangle> triangles, string outputPath, bool compress = false, int? precision = null)
        {
            var materialCache = new MaterialsCache();
            var default_hex_color = "#D94F33"; // "#bb3333";
            var defaultMaterial = MaterialCreator.GetDefaultMaterial(default_hex_color);

            var mesh = new MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>("mesh");

            foreach (var triangle in triangles) {
                MaterialBuilder material;
                if (triangle.Shader != null) {
                    material = materialCache.GetMaterialBuilderByShader(triangle.Shader);
                }
                else {
                    material = defaultMaterial;
                }

                DrawTriangle(triangle, material, mesh, precision);
            }
            var scene = new SceneBuilder();
            scene.AddRigidMesh(mesh, Matrix4x4.Identity);
            var model = scene.ToGltf2();
            var bytes = model.WriteGLB().Array;

            if(compress) {
                bytes = Compress(bytes, outputPath);
            }

            return bytes;
        }

        // Experimental
        private static byte[] Compress(byte[] glb, string outputPath)
        {
            bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            var uncompressed = Path.Combine(outputPath, "uncompressed.glb");
            var compressed = Path.Combine(outputPath, "compressed.glb");

            File.WriteAllBytes(uncompressed, glb);

            var fileName = IsWindows ? "cmd.exe" : IsLinux ? "/bin/bash" : throw new NotImplementedException("Compress not implemented for platform");
            var arguments = IsWindows ? $@"/C gltf-pipeline -i {uncompressed} -d -o {compressed}" : $"-c \"gltf-pipeline -i {uncompressed} -d -o {compressed} --draco.compressionLevel 8 --draco.quantizePositionBits 12 --draco.quantizeTexcoordBits 8\"";

            var process = new Process() {
                StartInfo = new ProcessStartInfo {
                    FileName = fileName,
                    WorkingDirectory = outputPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };
            process.Start();
            while (!process.StandardOutput.EndOfStream) {
                string line = process.StandardOutput.ReadLine();
            }

            process.Close();
            process.Dispose();
            var byteData = File.ReadAllBytes(compressed);

            File.Delete(uncompressed);
            File.Delete(compressed);

            return byteData;
        }

        private static bool DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty> mesh, int? precision = null)
        {
            var normal = triangle.GetNormal();
            var prim = mesh.UsePrimitive(material);
            var vectors = triangle.ToVectors();
            if(precision.HasValue)
            {
                vectors.Item1 = UpdateVector3Precision(vectors.Item1, precision.Value);
                vectors.Item2 = UpdateVector3Precision(vectors.Item2, precision.Value);
                vectors.Item3 = UpdateVector3Precision(vectors.Item3, precision.Value);
            }

            var indices = prim.AddTriangleWithBatchId(vectors, normal, triangle.GetBatchId());
            return indices.Item1 > 0;
        }

        private static Vector3 UpdateVector3Precision(Vector3 vec, int precision) 
        {
            var x = Math.Round(Convert.ToDecimal(vec.X), precision);
            var y = Math.Round(Convert.ToDecimal(vec.X), precision);
            var z = Math.Round(Convert.ToDecimal(vec.X), precision);
            return new Vector3((float)x, (float)y, (float)z);
        }
    }
}
