using System.IO;
using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using Wkx;
using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPositionNormal;

namespace Wkb2Gltf.Tests
{
    public class WkbCreatorTests
    {

        [Test]
        public void CreateWkbTest()
        {
            // arrange
            var buildingWkb = File.OpenRead(@"testfixtures/coloring_issue.wkb");
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var polyhedralsurface = ((PolyhedralSurface)g);
            var triangleCollection = Triangulator.GetTriangles(polyhedralsurface);

            // act
            var bytes = GlbCreator.GetGlb(triangleCollection, new double[] { 3916436, 297015, 5008536 });

            // Save bytes to binary file...
            File.WriteAllBytes(@"d:\aaa\test1.glb",bytes);

            // assert
            Assert.IsTrue(bytes != null);
        }


        [Test]
        public void CreateWkb1Test()
        {
            // arrange
            var buildingWkb = File.OpenRead(@"testfixtures/8123.wkb");
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var polyhedralsurface = ((PolyhedralSurface)g);
            var triangleCollection = Triangulator.GetTriangles(polyhedralsurface);

            // act
            var bytes = GlbCreator.GetGlb(triangleCollection, new double[] { 3916436, 297015, 5008536 });

            // Save bytes to binary file...
            File.WriteAllBytes(@"d:\aaa\test1.glb", bytes);

            // assert
            Assert.IsTrue(bytes != null);
        }


        [Test]
        public void ColorTest()
        {
            var p1 = new Point(0, 0, 0);
            var p2 = new Point(1, 1, 0);
            var p3 = new Point(1, 0, 0);

            var triangle1 = new Triangle(p1, p2, p3);

            p1 = new Point(5, 5, 0);
            p2 = new Point(6, 6, 0);
            p3 = new Point(6, 5, 0);

            var triangle2 = new Triangle(p1, p2, p3);

            var materialRed = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", new Vector4(1, 0, 0, 1));

            var materialGreen = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", new Vector4(0, 1, 0, 1));

            var materialWhite = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam("BaseColor", new Vector4(1, 1, 1, 1));

            var mesh = new MeshBuilder<VERTEX>("mesh");
            DrawTriangle(triangle1, materialWhite, mesh);
            DrawTriangle(triangle2, materialGreen, mesh);

            var model = ModelRoot.CreateModel();
            model.CreateMeshes(mesh);
        }

        private static void DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VERTEX> mesh)
        {
            var normal = triangle.GetNormal();

            var prim = mesh.UsePrimitive(material);

            prim.AddTriangle(
                new VERTEX((float)triangle.GetP0().X, (float)triangle.GetP0().Y, (float)triangle.GetP0().Z, normal.X, normal.Y, normal.Z),
                new VERTEX((float)triangle.GetP1().X, (float)triangle.GetP1().Y, (float)triangle.GetP1().Z, normal.X, normal.Y, normal.Z),
                new VERTEX((float)triangle.GetP2().X, (float)triangle.GetP2().Y, (float)triangle.GetP2().Z, normal.X, normal.Y, normal.Z)
                );
        }
    }
}
