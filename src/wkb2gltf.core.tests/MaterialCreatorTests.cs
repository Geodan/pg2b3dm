using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;

namespace Wkb2Gltf.Tests
{
    using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPosition;
    public class MaterialCreatorTests
    {
        [Test]
        public void ColorConvertTest()
        {
            var hex = "#E6008000";
            var color = ColorTranslator.FromHtml(hex);
            Assert.IsTrue(color.A == 230);
            Assert.IsTrue(color.R == 0);
            Assert.IsTrue(color.G == 128);
            Assert.IsTrue(color.B == 0);
            var hex2 = ColorTranslator.ToHtml(color);
            Assert.IsTrue(hex.EndsWith(hex2.Substring(1,hex2.Length-1)));
        }

        [Test]
        public void MetallicRoughnessBaseColorTest()
        {
            // arrange
            var shader = new Shader();
            // #008000 = green https://www.color-hex.com/color/008000 (0,128,0))
            shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = "#008000", MetallicRoughness = "" };


            // act
            var material = MaterialCreator.CreateMaterial(shader);

            // asssert
            Assert.IsNotNull(material);
            var rgba = (Vector4)material.Channels.First().Parameters["RGBA"];
            Assert.IsTrue(rgba.X == 0);
            Assert.IsTrue(Math.Round(rgba.Y * 255) == 128);
            Assert.IsTrue(rgba.Z == 0);
            Assert.IsTrue(rgba.W == 1);

            SaveSampleModel(material, "basecolor");
        }

        [Test]
        public void MetallicRoughnessBaseColorEmissiveTest()
        {
            // arrange
            var shader = new Shader();
            var emissive = new Vector3(10, 20, 30);
            // #008000 = green https://www.color-hex.com/color/008000 (0,128,0))
            shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = "#008000"};
            shader.EmissiveColor = ColorTranslator.ToHtml(Color.FromArgb(10, 20, 30));
            // act
            var material = MaterialCreator.CreateMaterial(shader);

            // asssert
            Assert.IsNotNull(material);
            var rgba = (Vector4)material.Channels.ToArray()[1].Parameters["RGBA"];
            Assert.IsTrue(rgba.X == 0);
            Assert.IsTrue(Math.Round(rgba.Y * 255) == 128);
            Assert.IsTrue(rgba.Z == 0);
            Assert.IsTrue(rgba.W == 1);
            var emissiveParams = material.Channels.ToArray()[0].Parameters;
            var emissiveVector3 = (Vector3)emissiveParams[KnownProperty.RGB];
            Assert.AreEqual(emissive.X, Math.Round(emissiveVector3.X*255));
            Assert.AreEqual(emissive.Y, Math.Round(emissiveVector3.Y * 255));
            Assert.AreEqual(emissive.Z, Math.Round(emissiveVector3.Z * 255));
            SaveSampleModel(material, "basecolor");
        }


        [Test]
        public void MetallicRoughnessTest()
        {
            // arrange
            var shader = new Shader();
            // #008000 = green https://www.color-hex.com/color/008000 (0,128,0))
            var metallicFactor = 0.2f;
            var roughnessFactor = 0.4f;

            var c = Color.FromArgb((int)(metallicFactor * 255), ((int)(roughnessFactor * 255)), 0);
            var hex = ColorTranslator.ToHtml(c);

            shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = "#008000", MetallicRoughness = hex };

            // act
            var material = MaterialCreator.CreateMaterial(shader);

            // asssert
            Assert.IsNotNull(material);
            Assert.IsTrue(material.Channels.Count == 2);
            var metallicRoughnessParams = material.Channels.ToArray()[1].Parameters;
            Assert.IsTrue(metallicRoughnessParams[KnownProperty.MetallicFactor] == metallicFactor);
            Assert.IsTrue(metallicRoughnessParams[KnownProperty.RoughnessFactor] == roughnessFactor);

            SaveSampleModel(material, "metallic");
        }

        [Test]
        public void SpecularGlossinessWithDiffuseTest()
        {
            var shader = new Shader();
            var diffuse = Color.FromArgb(10, 20, 30, 40); // diffuse rgb + alpha
            var specularGlossiness = Color.FromArgb(50, 60, 70, 80); // specular red, green, blue + glossiness
            shader.PbrSpecularGlossiness = new PbrSpecularGlossiness() { DiffuseColor = ColorTranslator.ToHtml(diffuse), SpecularGlossiness = ColorTranslator.ToHtml(specularGlossiness) };
            // act
            var material = MaterialCreator.CreateMaterial(shader);

            // asssert
            Assert.IsNotNull(material);
            Assert.IsTrue(material.Channels.Count == 2);
            SaveSampleModel(material, "specularglossinesswithdiffuse");
        }


        [Test]
        public void SpecularGlossinessTest()
        {
            var shader = new Shader();
            var specularGlossiness = Color.FromArgb(50, 60, 70, 80); // specular red, green, blue + glossiness
            shader.PbrSpecularGlossiness = new PbrSpecularGlossiness() { SpecularGlossiness = ColorTranslator.ToHtml(specularGlossiness) };
            // act
            var material = MaterialCreator.CreateMaterial(shader);

            // asssert
            Assert.IsNotNull(material);
            Assert.IsTrue(material.Channels.Count == 1);
            SaveSampleModel(material, "specularglossiness");
        }

        private static void SaveSampleModel(MaterialBuilder material, string name)
        {
            var mesh = new MeshBuilder<VERTEX>("mesh");

            var prim = mesh.UsePrimitive(material);
            prim.AddTriangle(new VERTEX(-10, 0, 0), new VERTEX(10, 0, 0), new VERTEX(0, 10, 0));
            prim.AddTriangle(new VERTEX(10, 0, 0), new VERTEX(-10, 0, 0), new VERTEX(0, -10, 0));
            var scene = new SharpGLTF.Scenes.SceneBuilder();
            scene.AddRigidMesh(mesh, Matrix4x4.Identity);

            var model = scene.ToGltf2();
            model.SaveGLB(@$"{name}.glb");
        }
    }
}
