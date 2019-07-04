using System.IO;
using glTFLoader;
using NUnit.Framework;
// using Triangulator;
using Wkx;

namespace Wkb2Gltf.Tests
{
    public class GltfLoaderTests
    {
        [Test]
        public void LoadSampleGltf()
        {
            var gltf = Interface.LoadModel(@"testfixtures/2CylinderEngine.gltf");
            var stream = File.OpenRead(@"testfixtures/2CylinderEngine0.bin");
            var reader = new BinaryReader(stream);
            var bytes = reader.ReadBytes((int)reader.BaseStream.Length);
            foreach (var buf in gltf.Buffers) {
                buf.Uri = null;
            }
            gltf.SaveBinaryModel(bytes, "test.glb");
        }

        [Test]
        public void GenerateGltfTest()
        {
            // arrange
            var tempPath = Path.GetTempPath();
            var buildingWkb = File.OpenRead(@"testfixtures/building.wkb");
            var g = Wkx.Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var surface = ((PolyhedralSurface)g);
            var translation = new double[] { 1842015.125, 5177109.25, 247.87364196777344};
            var triangles = Triangulator.GetTriangles(surface);
            var bb = surface.GetBoundingBox3D();
            var gltfArray = Gltf2Loader.GetGltfArray(triangles, bb);
            var material = MaterialMaker.CreateMaterial("Material_house", 139 / 255f, 69 / 255f, 19 / 255f, 1.0f);
            var gltf = Gltf2Loader.ToGltf(gltfArray, translation, material);
            gltf.Gltf.SaveBinaryModel(gltf.Body, @"d:\aaa\hihi.glb");
        }

    }
}
