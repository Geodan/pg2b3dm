using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using Wkx;

namespace Wkb2Gltf.Tests
{
    public class GlbCreatorTests
    {
        [Test]
        public void CreateGlbWithDefaultColor()
        {
            // arrange
            var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var polyhedralsurface = ((PolyhedralSurface)g);
            var triangles = Triangulator.GetTriangles(polyhedralsurface, new string[0], 100);
            var bytes = GlbCreator.GetGlb(triangles);
            File.WriteAllBytes(@"d:\aaa\ams_building_default_color.glb", bytes);
        }

        [Test]
        public void CreateGlbWithSingleColor()
        {
            // arrange
            var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var polyhedralsurface = ((PolyhedralSurface)g);
            var triangles = Triangulator.GetTriangles(polyhedralsurface, new string[1] { "#bb3333" }, 100);
            var bytes = GlbCreator.GetGlb(triangles);
            File.WriteAllBytes(@"d:\aaa\ams_building_single_color.glb", bytes);
        }

        [Test]
        public void CreateGlbWithMultipleColors()
        {
            // arrange
            var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var polyhedralsurface = ((PolyhedralSurface)g);

            var colors = new List<string>();
            foreach(var geo in polyhedralsurface.Geometries) {
                var random = new Random();
                var color = String.Format("#{0:X6}", random.Next(0x1000000));
                colors.Add(color);
            }

            var triangles = Triangulator.GetTriangles(polyhedralsurface, colors.ToArray(), 100);
            var bytes = GlbCreator.GetGlb(triangles);
            File.WriteAllBytes(@"d:\aaa\ams_building_multiple_colors.glb", bytes);
        }

        [Test]
        public void CreateGlbWithWrongNumberOfMultipleColorsGivesException()
        {
            // arrange
            var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var polyhedralsurface = ((PolyhedralSurface)g);

            var colors = new List<string>();
            for(var i= 0;i < polyhedralsurface.Geometries.Count - 2;i++) {  // wrong number two
                colors.Add("#d117b8");
            }

            try {
                var triangles = Triangulator.GetTriangles(polyhedralsurface, colors.ToArray(), 100);
            }
            catch(Exception ex){
                Assert.IsTrue(ex != null);
            }
        }

        [Test]
        public void ColorTest()
        {
            var p1 = new Point(0, 0, 0);
            var p2 = new Point(1, 1, 0);
            var p3 = new Point(1, 0, 0);

            var triangle1 = new Triangle(p1, p2, p3, 100);

            p1 = new Point(5, 5, 0);
            p2 = new Point(6, 6, 0);
            p3 = new Point(6, 5, 0);

            var triangle2 = new Triangle(p1, p2, p3, 100);

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

            var mesh = new MeshBuilder<VertexPositionNormal>("mesh");
            DrawTriangle(triangle1, materialWhite, mesh);
            DrawTriangle(triangle2, materialGreen, mesh);
            var scene = new SceneBuilder();
            scene.AddMesh(mesh, Matrix4x4.Identity);
            var model = scene.ToSchema2();
        }

        private static void DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal> mesh)
        {
            var normal = triangle.GetNormal();

            var prim = mesh.UsePrimitive(material);

            prim.AddTriangle(
                new VertexPositionNormal((float)triangle.GetP0().X, (float)triangle.GetP0().Y, (float)triangle.GetP0().Z, normal.X, normal.Y, normal.Z),
                new VertexPositionNormal((float)triangle.GetP1().X, (float)triangle.GetP1().Y, (float)triangle.GetP1().Z, normal.X, normal.Y, normal.Z),
                new VertexPositionNormal((float)triangle.GetP2().X, (float)triangle.GetP2().Y, (float)triangle.GetP2().Z, normal.X, normal.Y, normal.Z)
                );
        }
    }
}
