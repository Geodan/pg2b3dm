using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;

namespace Wkb2Gltf.Tests;

using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPosition;
public class MaterialCreatorTests
{
    [Test]
    public void ColorConvertTest()
    {
        var hex = "#FF000055";
        var color = RgbaColor.FromHex(hex);
        Assert.That(color.A == 85);
        Assert.That(color.R == 255, Is.True);
        Assert.That(color.G == 0, Is.True);
        Assert.That(color.B == 0, Is.True);
    }

    [Test]
    public void MetallicRoughnessBaseColorTest()
    {
        // arrange
        var shader = new Shader();
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = "#00800081", MetallicRoughness = "" };

        // act
        var material = MaterialCreator.CreateMaterial(shader, true, AlphaMode.BLEND);

        // asssert
        Assert.That(material != null);
        var rgba = (Vector4)material.Channels.First().Parameters["RGBA"];
        Assert.That(rgba.X == 0, Is.True);
        Assert.That(Math.Round(rgba.Y * 255) == 128, Is.True);
        Assert.That(rgba.Z == 0, Is.True);
        Assert.That(Math.Round(rgba.W * 255) == 129, Is.True);

        SaveSampleModel(material, "basecolor");
    }

    [Test]
    public void MetallicRoughnessBaseColorWithMaskAlphaModeTest()
    {
        // arrange
        var shader = new Shader();
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = "#00800081", MetallicRoughness = "" };

        // act
        var material = MaterialCreator.CreateMaterial(shader, true, AlphaMode.MASK, 0.5f);

        // assert
        Assert.That(material != null);
        Assert.That(material.AlphaMode, Is.EqualTo(AlphaMode.MASK));
        Assert.That(material.AlphaCutoff, Is.EqualTo(0.5f));
    }

    [Test]
    public void MetallicRoughnessBaseColorWithMaskAlphaModeAndCustomCutoffTest()
    {
        // arrange
        var shader = new Shader();
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = "#00800081", MetallicRoughness = "" };

        // act
        var material = MaterialCreator.CreateMaterial(shader, true, AlphaMode.MASK, 0.3f);

        // assert
        Assert.That(material != null);
        Assert.That(material.AlphaMode, Is.EqualTo(AlphaMode.MASK));
        Assert.That(material.AlphaCutoff, Is.EqualTo(0.3f));
    }

    [Test]
    public void MetallicRoughnessBaseColorEmissiveTest()
    {
        // arrange
        var shader = new Shader();
        var emissive = new Vector3(10, 20, 30);
        // #008000 = green https://www.color-hex.com/color/008000 (0,128,0))
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = "#008000" };
        shader.EmissiveColor = RgbaColor.ToHex(Color.FromArgb(10, 20, 30));
        // act
        var material = MaterialCreator.CreateMaterial(shader);

        // asssert
        Assert.That(material != null);
        var rgba = (Vector4)material.Channels.ToArray()[1].Parameters["RGBA"];
        Assert.That(rgba.X == 0, Is.True);
        Assert.That(Math.Round(rgba.Y * 255) == 128, Is.True);
        Assert.That(rgba.Z == 0, Is.True);
        Assert.That(rgba.W == 1, Is.True);
        var emissiveParams = material.Channels.ToArray()[0].Parameters;
        var emissiveVector3 = (Vector3)emissiveParams[KnownProperty.RGB];
        Assert.That(Math.Round(emissiveVector3.X * 255), Is.EqualTo(emissive.X));
        Assert.That(Math.Round(emissiveVector3.Y * 255), Is.EqualTo(emissive.Y));
        Assert.That(Math.Round(emissiveVector3.Z * 255), Is.EqualTo(emissive.Z));
        SaveSampleModel(material, "emmisive");
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
        var hex = RgbaColor.ToHex(c);

        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = "#008000", MetallicRoughness = hex };

        // act
        var material = MaterialCreator.CreateMaterial(shader);

        // asssert
        Assert.That(material != null);
        Assert.That(material.Channels.Count == 2, Is.True);
        var metallicRoughnessParams = material.Channels.ToArray()[1].Parameters;
        Assert.That(metallicRoughnessParams[KnownProperty.MetallicFactor] == metallicFactor, Is.True);
        Assert.That(metallicRoughnessParams[KnownProperty.RoughnessFactor] == roughnessFactor, Is.True);

        SaveSampleModel(material, "metallic");
    }

    [Test]
    public void SpecularGlossinessWithDiffuseTest()
    {
        var shader = new Shader();
        var diffuse = Color.FromArgb(10, 20, 30, 40); // diffuse rgb + alpha
        var specularGlossiness = Color.FromArgb(50, 60, 70, 80); // specular red, green, blue + glossiness
        shader.PbrSpecularGlossiness = new PbrSpecularGlossiness() { DiffuseColor = RgbaColor.ToHex(diffuse), SpecularGlossiness = RgbaColor.ToHex(specularGlossiness) };
        // act
        var material = MaterialCreator.CreateMaterial(shader);

        // asssert
        Assert.That(material!=null);
        Assert.That(material.Channels.Count == 2, Is.True);
        SaveSampleModel(material, "specularglossinesswithdiffuse");
    }


    [Test]
    public void SpecularGlossinessTest()
    {
        var shader = new Shader();
        var specularGlossiness = Color.FromArgb(50, 60, 70, 80); // specular red, green, blue + glossiness
        shader.PbrSpecularGlossiness = new PbrSpecularGlossiness() { SpecularGlossiness = RgbaColor.ToHex(specularGlossiness) };
        // act
        var material = MaterialCreator.CreateMaterial(shader);

        // asssert
        Assert.That(material != null);
        Assert.That(material.Channels.Count == 1, Is.True);
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
