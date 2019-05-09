using System.IO;
using glTFLoader;
using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests
{
    public class GeometryToGlbConvertorTests
    {
        [Test]
        public void GeometryToGlbTests()
        {
            // arrange
            var buildingWkb = File.OpenRead(@"testfixtures/building_1_triangles.wkb");
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var translation = new double[] { 539085.1, 6989220.68, 52.98 };

            // act
            var surface = (PolyhedralSurface)g;
            var triangles = Triangulator.GetTriangles(surface);
            var bb = surface.GetBoundingBox3D();
            var gltfArray = Gltf2Loader.GetGltfArray(triangles,bb);
            var material = MaterialMaker.CreateMaterial("Material_house", 139 / 255f, 69 / 255f, 19 / 255f, 1.0f);
            var gltf = Gltf2Loader.ToGltf(gltfArray, translation, material);

            gltf.Gltf.SaveBinaryModel(gltf.Body, @"d:/aaa/test43434.glb");

            // assert
            Assert.IsTrue(gltf!=null);
        }
    }
}
