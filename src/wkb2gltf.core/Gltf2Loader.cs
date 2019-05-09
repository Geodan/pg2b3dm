using System.Collections.Generic;
using glTFLoader.Schema;
using Wkb2Gltf.extensions;

namespace Wkb2Gltf
{
    public static class Gltf2Loader
    {

        public static GltfAll ToGltf(GltfArray gltfArray, double[] translation, Material material, string buffer_uri = "")
        {
            var body = gltfArray.AsBinary();
            var gltf = GetGltf(gltfArray, translation, material, buffer_uri);
            var all = new GltfAll() { Gltf = gltf, Body = body };
            return all;
        }

        public static GltfArray GetGltfArray(TriangleCollection triangles, BoundingBox3D bb)
        {
            var bytesVertices = triangles.PositionsToBinary();
            var bytesNormals = triangles.NormalsToBinary();

            var gltfArray = new GltfArray(bytesVertices) {
                Normals = bytesNormals,
                BBox = bb
            };
            return gltfArray;
        }

        private static Gltf GetGltf(GltfArray gltfArray, double[] translation, Material material, string buffer_uri = "")
        {
            var gltf = new Gltf {
                Asset = GetAsset(),
                Scene = 0,
                Materials = new Material[] { material },
                Nodes = GetNodes(),
                Buffers = GetBuffers(gltfArray.Vertices.Length, buffer_uri),
                Meshes = GetMeshes(),
                BufferViews = GetBufferViews(gltfArray.Vertices.Length),
                Accessors = GetAccessors(gltfArray.BBox, gltfArray.Count)
            };
            gltf.Scenes = GetScenes(gltf.Nodes.Length);
            return gltf;
        }

        private static Scene[] GetScenes(int nodes)
        {
            var scene = new Scene {
                Nodes = new int[nodes]
            };
            return new Scene[] { scene };
        }

        private static Accessor[] GetAccessors(BoundingBox3D bb, int n)
        {
            // q: max and min are reversed in next py code?
            var max = new float[3] { (float)bb.YMin, (float)bb.ZMin, (float)bb.XMin };
            var min = new float[3] { (float)bb.YMax, (float)bb.ZMax, (float)bb.XMax };
            var accessor = GetAccessor(0, n, min, max, Accessor.TypeEnum.VEC3);
            max = new float[3] { 1, 1, 1 };
            min = new float[3] { -1, -1, -1 };
            var accessorNormals = GetAccessor(1, n, min, max, Accessor.TypeEnum.VEC3);
            var batchLength = 1;
            max = new float[1] { batchLength };
            min = new float[1] { 0 };
            var accessorBatched = GetAccessor(2, n, min, max, Accessor.TypeEnum.SCALAR);
            return new Accessor[] { accessor, accessorNormals, accessorBatched };
        }

        private static Accessor GetAccessor(int bufferView, int n, float[] min, float[] max, Accessor.TypeEnum type)
        {
            var accessor = new Accessor();
            accessor.BufferView = bufferView;
            accessor.ByteOffset = 0;
            accessor.ComponentType = Accessor.ComponentTypeEnum.FLOAT;
            accessor.Count = n;
            accessor.Min = min;
            accessor.Max = max;
            accessor.Type = type;
            return accessor;
        }

        private static BufferView[] GetBufferViews(int verticesLength)
        {
            // q: whats the logic here?
            var bv1 = GetBufferView(verticesLength, 0);
            var bv2 = GetBufferView(verticesLength, verticesLength);
            var bv3 = GetBufferView(verticesLength / 3, 2 * verticesLength);
            return new BufferView[] { bv1, bv2, bv3 };
        }

        private static BufferView GetBufferView(int byteLength, int byteOffset)
        {
            var bufferView1 = new BufferView();
            bufferView1.Buffer = 0;
            bufferView1.ByteLength = byteLength;
            bufferView1.ByteOffset = byteOffset;
            bufferView1.Target = BufferView.TargetEnum.ARRAY_BUFFER;
            return bufferView1;
        }

        private static Mesh[] GetMeshes()
        {
            var mesh = new Mesh();

            var attributes = new Dictionary<string, int>();
            attributes.Add("POSITION", 0);
            attributes.Add("NORMAL", 1);
            attributes.Add("_BATCHID", 2);

            var primitive = new MeshPrimitive();
            primitive.Attributes = attributes;
            primitive.Material = 0;
            primitive.Mode = MeshPrimitive.ModeEnum.TRIANGLES;
            mesh.Primitives = new MeshPrimitive[] { primitive };
            return new Mesh[] { mesh };
        }

        private static Buffer[] GetBuffers(int verticesLength, string buffer_uri = "")
        {
            var byteLength = verticesLength * 2;
            byteLength += verticesLength / 3;

            var buffer = new Buffer() {
                ByteLength = byteLength
            };
            if (!string.IsNullOrEmpty(buffer_uri)) {
                buffer.Uri = buffer_uri;
            } 

            return new Buffer[] { buffer };
        }

        private static Node[] GetNodes()
        {
            var node = new Node() {
                Matrix = new float[] {1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
                Mesh = 0
            };
            return new Node[] { node };
        }

        private static Material[] GetMaterials()
        {
            var material = new Material() {
                Name = "Material",
                PbrMetallicRoughness = new MaterialPbrMetallicRoughness() { MetallicFactor = 0 },
            };
            return new Material[] { material };
        }

        private static Asset GetAsset()
        {
            var asset = new Asset();
            asset.Generator = "Glt.Core";
            asset.Version = "2.0";
            return asset;
        }
    }
}
