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
        public static byte[] GetGlb(List<Triangle> triangles, string outputPath, bool compress = false)
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

                DrawTriangle(triangle, material, mesh);
            }
            var scene = new SceneBuilder();
            scene.AddRigidMesh(mesh, Matrix4x4.Identity);
            var model = scene.ToGltf2();
            var bytes = model.WriteGLB().Array;

            //if(compress) {
            bytes = Compress(bytes, outputPath);
            //}

            return bytes;
        }

        // Experimental
        private static byte[] Compress(byte[] glb, string outputPath)
        {
            bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            //var tempDirectory = Path.Combine(Directory.GetCurrentDirectory(), "temp");
            var uncompressed = Path.Combine(outputPath, "uncompressed.glb");
            var compressed = Path.Combine(outputPath, "compressed.glb");

            //Directory.CreateDirectory(tempDirectory);
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
