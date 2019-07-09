using System.Collections.Generic;

namespace Wkb2Gltf.extensions
{
    public static class TriangleCollectionExtensions
    {

        public static byte[] PositionsToBinary(this TriangleCollection triangleCollection)
        {
            var floats = new List<float>();
            foreach (var triangle in triangleCollection) {
                floats.AddRange(triangle.Flatten());
            }
            var bytes = BinaryConvertor.ToBinary(floats.ToArray());
            return bytes;
        }


        public static byte[] NormalsToBinary(this TriangleCollection triangleCollection)
        {
            var normals = triangleCollection.GetNormals();
            var faces = triangleCollection.GetFaces(normals);

            var floats = new List<float>();
            foreach (var face in faces) {
                floats.Add(face.X);
                floats.Add(face.Y);
                floats.Add(face.Z);
            }
            var bytes = BinaryConvertor.ToBinary(floats.ToArray());
            return bytes;
        }

    }
}
