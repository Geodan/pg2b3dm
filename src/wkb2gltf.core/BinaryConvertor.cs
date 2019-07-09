using System.IO;

namespace Wkb2Gltf
{
    public static class BinaryConvertor
    {
        public static byte[] ToBinary(float[] array1)
        {
            var ms = new MemoryStream();
            var binaryWriter = new BinaryWriter(ms);
            foreach (var p in array1)
            {
                binaryWriter.Write(p);
            }
            var bytes = ms.ToArray();
            ms.Close();
            return bytes;
        }
    }
}
