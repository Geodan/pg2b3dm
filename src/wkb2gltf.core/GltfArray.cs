using System;
using System.Linq;

namespace Wkb2Gltf
{
    public class GltfArray
    {
        private byte[] vertices;
        private int n;

        public GltfArray(byte[] Vertices)
        {
            vertices = Vertices;
            n = (int)Math.Round((double)vertices.Length / 12, 0);

        }
        public byte[] Vertices {
            get {
                return vertices;
            }
        }
        public byte[] Normals { get; set; }
        public byte[] Ids { get { return BinaryConvertor.ToBinary(new float[n]); } }

        public int Count { get { return n; } }
        public byte[] Uvs { get; set; }
        public BoundingBox3D BBox { get; set; }

        public byte[] AsBinary()
        {
            return Vertices.Concat(Normals).Concat(Ids).ToArray();
        }
        // body = vertices + normals + ids + uvs
    }
}
