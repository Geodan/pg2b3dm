using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using Wkb2Gltf.Extensions;
using Wkx;
using System.Linq;

namespace Wkb2Gltf.Tests;

public class GlbCreatorTests
{
    [SetUp]
    public void SetUp()
    {
        Tiles3DExtensions.RegisterExtensions();
    }


    [Test]
    public void GetGlb_WithAllEmptyStringAttributes_ShouldAddZeroWidthSpace()
    {
        // arrange
        var p0 = new Point(0, 0, 0);
        var p1 = new Point(1, 1, 0);
        var p2 = new Point(1, 0, 0);

        var triangle1 = new Triangle(p0, p1, p2, 0);

        var triangles = new List<Triangle>() { triangle1 };

        var attributes = new Dictionary<string, List<object>>
        {
            { "empty_string_field", new List<object> { "", "", "" } }
        };

        // Act
        var result = GlbCreator.GetGlb(
            triangles: new List<List<Triangle>>() { triangles},
            createGltf: true,
            attributes: attributes
        );

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void CreateGlbFromOvertureBuildingIssue210()
    {
        var wkt = "MULTIPOLYGON Z(((5.7945897 45.2104504 0, 5.794611 45.2104414 0, 5.794616 45.210422 0, 5.794607 45.210411 0, 5.794653 45.210392 0, 5.794662 45.210404 0, 5.794693 45.21041 0, 5.794708 45.210403 0, 5.7947283 45.2104284 0, 5.7950367 45.2103109 0, 5.795016 45.210285 0, 5.795032 45.210277 0, 5.79504 45.210257 0, 5.795032 45.210246 0, 5.7950659 45.2102338 0, 5.7949016 45.2100206 0, 5.7948693 45.2100324 0, 5.794856 45.210015 0, 5.794833 45.21001 0, 5.79481 45.210018 0, 5.794786 45.209986 0, 5.794805 45.209977 0, 5.794811 45.209959 0, 5.794802 45.209947 0, 5.794849 45.209929 0, 5.794857 45.209942 0, 5.794883 45.209948 0, 5.794903 45.20994 0, 5.794922 45.2099659 0, 5.7952283 45.209848 0, 5.795208 45.209823 0, 5.795229 45.209814 0, 5.795236 45.209798 0, 5.7952263 45.2097839 0, 5.7952595 45.2097718 0, 5.7950922 45.2095571 0, 5.7950614 45.2095698 0, 5.79505 45.209554 0, 5.795025 45.209548 0, 5.795005 45.209556 0, 5.794978 45.209522 0, 5.794998 45.209515 0, 5.795003 45.209498 0, 5.794993 45.209485 0, 5.795043 45.209466 0, 5.795051 45.20948 0, 5.795078 45.209484 0, 5.795093 45.209479 0, 5.7951129 45.209504 0, 5.7954223 45.2093852 0, 5.795401 45.209357 0, 5.795421 45.209349 0, 5.795427 45.209333 0, 5.795419 45.20932 0, 5.795466 45.209303 0, 5.795474 45.209315 0, 5.795501 45.209321 0, 5.795521 45.209314 0, 5.795545 45.209346 0, 5.795526 45.209353 0, 5.79552 45.209372 0, 5.79553 45.209386 0, 5.7954975 45.2093987 0, 5.7956662 45.2096167 0, 5.795697 45.209604 0, 5.795706 45.209617 0, 5.795736 45.209621 0, 5.795751 45.209614 0, 5.7957717 45.209638 0, 5.7960757 45.2095206 0, 5.796056 45.209496 0, 5.796073 45.20949 0, 5.796083 45.209472 0, 5.796073 45.209459 0, 5.796121 45.209442 0, 5.796127 45.209453 0, 5.796155 45.209459 0, 5.79617 45.209453 0, 5.796198 45.209486 0, 5.796181 45.209491 0, 5.796175 45.209513 0, 5.796182 45.209523 0, 5.796159 45.209533 0, 5.796325 45.209751 0, 5.796349 45.209741 0, 5.796356 45.209752 0, 5.796386 45.209758 0, 5.796404 45.209751 0, 5.796427 45.209782 0, 5.796409 45.209791 0, 5.796402 45.20981 0, 5.79641 45.209822 0, 5.796365 45.20984 0, 5.796356 45.20983 0, 5.796327 45.209823 0, 5.796313 45.209829 0, 5.796301 45.209814 0, 5.795994 45.20993 0, 5.796006 45.209948 0, 5.795986 45.209955 0, 5.795978 45.209972 0, 5.795989 45.209986 0, 5.795967 45.209995 0, 5.79613 45.21021 0, 5.796153 45.210199 0, 5.796163 45.210214 0, 5.796193 45.210221 0, 5.796212 45.210213 0, 5.796223 45.210229 0, 5.796529 45.210111 0, 5.796517 45.210095 0, 5.796536 45.210088 0, 5.796545 45.210067 0, 5.796537 45.210056 0, 5.796576 45.210039 0, 5.796586 45.21005 0, 5.79662 45.210058 0, 5.796634 45.210051 0, 5.796659 45.210083 0, 5.79664 45.210091 0, 5.796631 45.210107 0, 5.796641 45.210121 0, 5.796621 45.21013 0, 5.796786 45.210347 0, 5.796809 45.210337 0, 5.79682 45.210351 0, 5.796846 45.210356 0, 5.796864 45.210348 0, 5.796889 45.210383 0, 5.79687 45.210389 0, 5.796862 45.210406 0, 5.796873 45.21042 0, 5.796827 45.210438 0, 5.796817 45.210426 0, 5.796791 45.210421 0, 5.796775 45.210427 0, 5.796763 45.210411 0, 5.796454 45.210528 0, 5.796466 45.210544 0, 5.796448 45.210552 0, 5.796439 45.21057 0, 5.79645 45.210584 0, 5.796405 45.210601 0, 5.796396 45.210589 0, 5.796368 45.210583 0, 5.796349 45.210588 0, 5.796325 45.210556 0, 5.796341 45.21055 0, 5.79635 45.210532 0, 5.796339 45.21052 0, 5.796359 45.210511 0, 5.796197 45.210294 0, 5.796173 45.210303 0, 5.796164 45.210292 0, 5.796134 45.210283 0, 5.796117 45.210289 0, 5.796106 45.210275 0, 5.7958 45.210393 0, 5.795812 45.210408 0, 5.795792 45.210416 0, 5.795787 45.210434 0, 5.795797 45.210447 0, 5.7957652 45.2104616 0, 5.7959346 45.2106796 0, 5.7959674 45.2106678 0, 5.7959744 45.2106808 0, 5.796002 45.2106868 0, 5.796022 45.2106793 0, 5.796047 45.2107141 0, 5.796028 45.2107206 0, 5.7960217 45.2107387 0, 5.7960324 45.2107511 0, 5.7959834 45.2107701 0, 5.7959734 45.2107549 0, 5.7959477 45.2107503 0, 5.7959297 45.2107573 0, 5.7959115 45.2107336 0, 5.7956061 45.2108505 0, 5.79562 45.210871 0, 5.7956077 45.2108772 0, 5.7956024 45.2108975 0, 5.7956117 45.2109137 0, 5.795558 45.2109328 0, 5.795548 45.210917 0, 5.795527 45.2109138 0, 5.7955077 45.2109217 0, 5.795473 45.210882 0, 5.795493 45.210876 0, 5.7955 45.210857 0, 5.795493 45.2108459 0, 5.7955288 45.2108345 0, 5.7953639 45.21062 0, 5.7953314 45.2106337 0, 5.795318 45.210617 0, 5.795298 45.2106119 0, 5.795275 45.2106218 0, 5.7952551 45.2105982 0, 5.7949514 45.2107133 0, 5.794967 45.210735 0, 5.79495 45.210741 0, 5.79494 45.210762 0, 5.79495 45.2107764 0, 5.7949047 45.2107944 0, 5.794894 45.210781 0, 5.7948694 45.2107774 0, 5.7948497 45.2107843 0, 5.7948247 45.2107503 0, 5.7948447 45.2107429 0, 5.794845 45.210719 0, 5.794842 45.210718 0, 5.794837 45.2107099 0, 5.7948729 45.2106981 0, 5.794709 45.210483 0, 5.794674 45.2104954 0, 5.794664 45.210484 0, 5.7946377 45.2104774 0, 5.7946194 45.2104848 0, 5.7945897 45.2104504 0), (5.7948218 45.2105268 0, 5.7948814 45.2106058 0, 5.7949205 45.210613 0, 5.7950307 45.2105723 0, 5.7950428 45.2105461 0, 5.794982 45.2104669 0, 5.7949441 45.2104594 0, 5.7948339 45.2105001 0, 5.7948218 45.2105268 0), (5.795077 45.2100413 0, 5.795136 45.2101188 0, 5.7951722 45.2101254 0, 5.7952808 45.2100857 0, 5.7952942 45.2100574 0, 5.7952318 45.2099785 0, 5.7951936 45.2099723 0, 5.795087 45.2100139 0, 5.795077 45.2100413 0), (5.7951149 45.2103275 0, 5.7951617 45.2103887 0, 5.7951924 45.2103881 0, 5.7952172 45.2103963 0, 5.7952376 45.2104137 0, 5.7952468 45.2104334 0, 5.7952454 45.2104499 0, 5.7952252 45.2104728 0, 5.7952748 45.2105395 0, 5.795311 45.2105278 0, 5.7953192 45.210541 0, 5.7953488 45.2105475 0, 5.7953696 45.2105399 0, 5.7953864 45.2105616 0, 5.7954845 45.2105228 0, 5.7954751 45.210511 0, 5.7954779 45.2104953 0, 5.7954913 45.2104816 0, 5.7955088 45.2104698 0, 5.7955318 45.2104649 0, 5.7955548 45.2104638 0, 5.7955758 45.2104684 0, 5.7955928 45.2104815 0, 5.7956888 45.210445 0, 5.7956721 45.2104225 0, 5.7956917 45.2104142 0, 5.7956985 45.2103947 0, 5.7956877 45.2103806 0, 5.7957202 45.2103691 0, 5.7956679 45.210303 0, 5.7956457 45.2103037 0, 5.7956249 45.2103016 0, 5.7956035 45.2102957 0, 5.7955874 45.2102851 0, 5.7955801 45.210269 0, 5.795581 45.2102529 0, 5.7955858 45.2102359 0, 5.7956054 45.2102208 0, 5.7955551 45.2101547 0, 5.795519 45.210168 0, 5.795509 45.210155 0, 5.795481 45.210149 0, 5.795465 45.210155 0, 5.7954507 45.2101334 0, 5.795351 45.2101691 0, 5.7953559 45.2101851 0, 5.7953541 45.2102002 0, 5.7953422 45.2102124 0, 5.7953223 45.2102237 0, 5.7952936 45.2102302 0, 5.7952645 45.2102273 0, 5.7952434 45.2102103 0, 5.795143 45.2102488 0, 5.795157 45.2102667 0, 5.7951378 45.2102728 0, 5.7951282 45.2103013 0, 5.7951408 45.2103175 0, 5.7951149 45.2103275 0), (5.7952513 45.2095852 0, 5.7953103 45.209665 0, 5.795374 45.2096773 0, 5.795488 45.2096353 0, 5.7955088 45.209588 0, 5.7954484 45.2095101 0, 5.7953847 45.2094954 0, 5.7952707 45.2095389 0, 5.7952513 45.2095852 0), (5.7953063 45.2098656 0, 5.7954689 45.2100792 0, 5.7955044 45.2100655 0, 5.7955165 45.2100806 0, 5.7955433 45.2100853 0, 5.7955601 45.2100783 0, 5.7955782 45.2101014 0, 5.7958819 45.2099847 0, 5.7958638 45.2099587 0, 5.7958806 45.2099526 0, 5.7958913 45.2099337 0, 5.7958813 45.2099191 0, 5.7959141 45.2099063 0, 5.7957488 45.2096945 0, 5.7957153 45.2097078 0, 5.7957086 45.2096945 0, 5.7956804 45.209686 0, 5.7956603 45.2096926 0, 5.7956442 45.2096709 0, 5.7953371 45.2097881 0, 5.7953532 45.2098108 0, 5.7953251 45.2098211 0, 5.7953183 45.2098372 0, 5.7953318 45.2098542 0, 5.7953063 45.2098656 0), (5.7957333 45.2101765 0, 5.7957933 45.2102544 0, 5.7958263 45.2102615 0, 5.7959357 45.210222 0, 5.7959511 45.210196 0, 5.7958921 45.2101167 0, 5.795855 45.2101077 0, 5.795748 45.2101481 0, 5.7957333 45.2101765 0), (5.796387 45.2103131 0, 5.7964487 45.2103919 0, 5.7964794 45.2103985 0, 5.7965921 45.2103586 0, 5.7966071 45.2103294 0, 5.7965458 45.2102503 0, 5.7965087 45.2102438 0, 5.7963983 45.2102865 0, 5.796387 45.2103131 0)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);
        var translation = new double[] { 0, 0, 0 };
        var triangles = GeometryProcessor.GetTriangles(g, 0);
        Assert.That(triangles.Count == 350);

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "overtureissue210.glb");
        File.WriteAllBytes(fileName, bytes);
    }


    [Test]
    public void CreateGlbForTinWith2Shaders()
    {
        var pipelineWkt = "TIN Z (((0 0 0,0 0 1,0 1 0,0 0 0)),((0 0 0,0 1 0,1 1 0,0 0 0)))";
        var g = Geometry.Deserialize<WktSerializer>(pipelineWkt);
        var translation = new double[] { 0, 0, 0 };
        var shaderColors = GetShaderColors(2);
        var triangles = GeometryProcessor.GetTriangles(g, 100, shadercolors: shaderColors);
        Assert.That(triangles.Count, Is.EqualTo(2));

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tin.glb");
        File.WriteAllBytes(@"tin.glb", bytes);
    }

    [Test]
    public void CreateGlbFromMultilineWith2Shaders()
    {
        var wkt = "MULTILINESTRING ((0 0 0, 1 1 0), (2 2 0, 3 3 0))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        var shaderColors = GetShaderColors(2);

        var triangles = GeometryProcessor.GetTriangles(g, 100, shadercolors: shaderColors);

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles }, createGltf: true, doubleSided: true);
        File.WriteAllBytes(@"multiline_2_shader.glb", bytes);
    }

    [Test]
    public void CreateGlbFromlineWith32Shader()
    {
        var wkt = "LINESTRING (0 0 0, 0 1 0, 1 2 0)";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        var shaderColors = GetShaderColors(32);

        var triangles = GeometryProcessor.GetTriangles(g, 100, shadercolors: shaderColors);

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles }, createGltf: true, doubleSided: true);
        File.WriteAllBytes(@"multipolygon_32_shader.glb", bytes);
    }


    [Test]
    public void CreateGlbFromPolygonWith1Shader()
    {
        var wkt = "POLYGON Z((0 0 0, 0 1 0, 1 1 0, 1 0 0, 0 0 0))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        var shaderColors = GetShaderColors(1);

        var triangles = GeometryProcessor.GetTriangles(g, 100, shadercolors: shaderColors);
        var i = 0;
        foreach (var triangle in triangles) {
            Assert.That(triangle.Shader.PbrMetallicRoughness.BaseColor.Equals(shaderColors.PbrMetallicRoughnessColors.BaseColors[0]));
            i++;
        }

        Assert.That(triangles.Count, Is.EqualTo(2));

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles }, createGltf: true, doubleSided: true);
        File.WriteAllBytes(@"multipolygon_1_shader.glb", bytes);
    }

    [Test]
    public void CreateGlbFromMultipolygonWith1Shaders()
    {
        var wkt = "MULTIPOLYGON Z(((0 0 0, 0 1 0, 1 1 0, 1 0 0, 0 0 0)),((2 2 0, 2 3 0, 3 3 0, 3 2 0, 2 2 0)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        var shaderColors = GetShaderColors(1);

        var triangles = GeometryProcessor.GetTriangles(g, 100, shadercolors : shaderColors );
        var i = 0;
        foreach (var triangle in triangles) {
            Assert.That(triangle.Shader.PbrMetallicRoughness.BaseColor.Equals(shaderColors.PbrMetallicRoughnessColors.BaseColors[0]));
            i++;
        }

        Assert.That(triangles.Count, Is.EqualTo(4));

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles }, createGltf: true, doubleSided : true);
        File.WriteAllBytes(@"multipolygon_1_shader.glb", bytes);
    }


    [Test]
    public void CreateGlbFromMultipolygonWith4Shaders()
    {
        // arrange multipolygon of 2 squares
        var wkt = "MULTIPOLYGON Z(((0 0 0, 0 1 0, 1 1 0, 1 0 0, 0 0 0)),((2 2 0, 2 3 0, 3 3 0, 3 2 0, 2 2 0)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        var shaderColors = GetShaderColors(4);

        var triangles = GeometryProcessor.GetTriangles(g, 100, shadercolors: shaderColors);

        Assert.That(triangles.Count, Is.EqualTo(4));
        var i = 0;
        foreach(var triangle in triangles)
        {
            Assert.That(triangle.Shader.PbrMetallicRoughness.BaseColor.Equals(shaderColors.PbrMetallicRoughnessColors.BaseColors[i]));
            i++;
        }
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles }, createGltf: true, doubleSided: true);
        File.WriteAllBytes(@"multipolygon_4_shader.glb", bytes);

    }

    [Test]
    public void CreateGlbFromMultipolygonWith2Shaders()
    {
        // arrange multipolygon of 2 squares
        var wkt = "MULTIPOLYGON Z(((0 0 0, 0 1 0, 1 1 0, 1 0 0, 0 0 0)),((2 2 0, 2 3 0, 3 3 0, 3 2 0, 2 2 0)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        var shaderColors = GetShaderColors(2);

        var triangles = GeometryProcessor.GetTriangles(g, 100, shadercolors: shaderColors);
        Assert.That(triangles.Count, Is.EqualTo(4));

        Assert.That(triangles[0].Shader.PbrMetallicRoughness.BaseColor.Equals(shaderColors.PbrMetallicRoughnessColors.BaseColors[0]));
        Assert.That(triangles[1].Shader.PbrMetallicRoughness.BaseColor.Equals(shaderColors.PbrMetallicRoughnessColors.BaseColors[0]));
        Assert.That(triangles[2].Shader.PbrMetallicRoughness.BaseColor.Equals(shaderColors.PbrMetallicRoughnessColors.BaseColors[1]));
        Assert.That(triangles[3].Shader.PbrMetallicRoughness.BaseColor.Equals(shaderColors.PbrMetallicRoughnessColors.BaseColors[1]));

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles }, createGltf: true, doubleSided: true);
        File.WriteAllBytes(@"multipolygon_2_shader.glb", bytes);
    }


    private ShaderColors GetShaderColors(int amount)
    {
        var shaderColors = new ShaderColors();
        var metallicRoughness = new PbrMetallicRoughnessColors();
        var random = new Random();
        var baseColors = new List<string>();
        for (var i = 0; i < amount; i++) {
            var color = string.Format("#{0:X6}", random.Next(0x1000000));
            baseColors.Add(color);
        }
        metallicRoughness.BaseColors = baseColors;
        shaderColors.PbrMetallicRoughnessColors = metallicRoughness;
        return shaderColors;
    }

    [Test]
    public void CreateGltfWithAttributesAllNulls()
    {
        // arrange
        var p0 = new Point(0, 0, 0);
        var p1 = new Point(1, 1, 0);
        var p2 = new Point(1, 0, 0);

        var triangle1 = new Triangle(p0, p1, p2, 0);
        var triangle2 = new Triangle(p0, p1, p2, 1);

        var triangles = new List<Triangle>() { triangle1, triangle2 };

        var attributes = new Dictionary<string, List<object>>();
        attributes.Add("test", new List<object>() { DBNull.Value, DBNull.Value });

        var bytes = TileCreator.GetTile(attributes, new List<List<Triangle>>() { triangles }, createGltf: true);

        Assert.That(bytes != null);
    }


    [Test]
    public void CreateGltfWithAttributeColumnAllNulls()
    {
        // arrange
        var p0 = new Point(0, 0, 0);
        var p1 = new Point(1, 1, 0);
        var p2 = new Point(1, 0, 0);

        var triangle1 = new Triangle(p0, p1, p2, 0);
        var triangle2 = new Triangle(p0, p1, p2, 1);

        var triangles = new List<Triangle>() { triangle1, triangle2 };

        var attributes = new Dictionary<string, List<object>>();
        attributes.Add("names", new List<object>() { "test0", "test1"});
        attributes.Add("test", new List<object>() { DBNull.Value, DBNull.Value });

        var bytes = TileCreator.GetTile(attributes, new List<List<Triangle>>() { triangles }, createGltf: true);

        Assert.That(bytes != null);
    }

    [Test]
    public void CreateGltfWithNullAttributesTest()
    {
        // arrange
        var p0 = new Point(0, 0, 0);
        var p1 = new Point(1, 1, 0);
        var p2 = new Point(1, 0, 0);

        var triangle1 = new Triangle(p0, p1, p2, 0);
        var triangle2 = new Triangle(p0, p1, p2, 1);

        var triangles = new List<Triangle>() { triangle1, triangle2 };

        var attributes = new Dictionary<string, List<object>>();
        attributes.Add("id_string", new List<object>() { "1", DBNull.Value });
        attributes.Add("id_int8", new List<object>() { (sbyte)100, DBNull.Value });
        attributes.Add("id_uint8", new List<object>() { (byte)100, DBNull.Value });
        attributes.Add("id_int16", new List<object>() { (short)100, DBNull.Value });
        attributes.Add("id_uint16", new List<object>() { (ushort)100, DBNull.Value });
        attributes.Add("id_uint32", new List<object>() { (uint)1, DBNull.Value });
        attributes.Add("id_int64", new List<object>() { (long)1, DBNull.Value });
        attributes.Add("id_uint64", new List<object>() { (ulong)1, DBNull.Value });
        attributes.Add("id_float", new List<object>() { (float)1, DBNull.Value });
        attributes.Add("id_decimal", new List<object>() { (decimal)1, DBNull.Value });
        attributes.Add("id_double", new List<object>() { (double)1, DBNull.Value });
        // note: null values are not supported for array types (including vector3 and matrix4x4)

        var bytes = TileCreator.GetTile(attributes, new List<List<Triangle>>() { triangles }, createGltf: true);
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "gltf_withnullattributes.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert
        var model = ModelRoot.Load(fileName);
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
    }

    [Test]
    public void CreateGltfWithAttributesTest()
    {
        // arrange
        var p0 = new Point(0, 0, 0);
        var p1 = new Point(1, 1, 0);
        var p2 = new Point(1, 0, 0);

        var triangle1 = new Triangle(p0, p1, p2, 0);

        var triangles = new List<Triangle>() { triangle1};

        var attributes = new Dictionary<string, List<object>>();
        attributes.Add("id_bool", new List<object>() { true });
        attributes.Add("id_int8", new List<object>() { (sbyte)100 });
        attributes.Add("id_uint8", new List<object>() { (byte)100 });
        attributes.Add("id_int16", new List<object>() { (short)100 });
        attributes.Add("id_uint16", new List<object>() { (ushort)100 });
        attributes.Add("id_int32", new List<object>() { 1 });
        attributes.Add("id_uint32", new List<object>() { (uint)1 });
        attributes.Add("id_int64", new List<object>() { (long)1 });
        attributes.Add("id_uint64", new List<object>() { (ulong)1 });
        attributes.Add("id_decimal", new List<object>() { (decimal)1 });
        attributes.Add("id_float", new List<object>() { (float)1 });
        attributes.Add("id_double", new List<object>() { (double)1 });
        attributes.Add("id_string", new List<object>() { "1" });
        attributes.Add("id_vector3", new List<object>() { new decimal[] { 0, 1, 2 } }); ;
        attributes.Add("id_datetime", new List<object>() { DateTime.Now });
        attributes.Add("id_matrix4x4", new List<object>() { new decimal[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } });
        attributes.Add("id_shorts", new List<object>() { new short[] { 0, 1 } });
        attributes.Add("id_booleans", new List<object>() { new bool[] { true, false } });
        attributes.Add("id_strings", new List<object>() { new string[] { "hallo", "hallo1"} });
        attributes.Add("id_ints", new List<object>() { new [] { 1, 2 } });
        attributes.Add("id_longs", new List<object>() { new long[] { 1, 2 } });
        attributes.Add("id_floats", new List<object>() { new float[] { 1, 2 } });
        attributes.Add("id_doubles", new List<object>() { new double[] { 1, 2 } });
        attributes.Add("id_decimals", new List<object>() { new decimal[] { 1, 2 } });
        attributes.Add("id_datetimes", new List<object>() { new DateTime[] { DateTime.Now, DateTime.Now} });


        decimal[,] array = new decimal[5, 3];
        var random = new Random();
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 3; j++) {
                array[i, j] = (decimal)random.NextDouble();
            }
        }
        attributes.Add("idVector3s", new List<object>() { array });

        var matrix = new decimal[2, 16];
        for (var i = 0; i < matrix.GetLength(0); i++) {
            for (var  j = 0; j < matrix.GetLength(1); j++) {
                matrix[i, j] = (decimal)random.NextDouble();
            }
        }
        attributes.Add("idMatrix4x4ss", new List<object>() { matrix});

        var random_decimals = new decimal[,] { { 1.23m, 4.56m } };
        attributes.Add("random_decimals", new List<object>() { random_decimals });

        // act
        var bytes = TileCreator.GetTile(attributes, new List<List<Triangle>>() { triangles }, createGltf: true);
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "gltf_withattributes.glb");
        File.WriteAllBytes(fileName, bytes);


        // assert
        var model = ModelRoot.Load(fileName);
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
    }

    [Test]
    public void CreateGlbWithInteriorRing()
    {
        // arrange
        var wkt = "POLYGON Z ((-75.55329332999997 39.101191216000075 0,-75.55324867699994 39.101184972000055 0,-75.55321256999997 39.101297267000064 0,-75.55286213099998 39.10123284600007 0,-75.55278609799996 39.10147265200004 0,-75.55273937899995 39.10146597100004 0,-75.55273290199995 39.10148368100005 0,-75.55249205599995 39.10143683100006 0,-75.55252592399995 39.101320928000064 0,-75.55243013499995 39.101299453000024 0,-75.55249910199996 39.10107472900006 0,-75.55243817199994 39.10106216400004 0,-75.55260907299999 39.10051940000005 0,-75.55279111499993 39.10055427900005 0,-75.55280219199994 39.10052833700007 0,-75.55342259199995 39.100650028000075 0,-75.55338423099994 39.10078085500004 0,-75.55341641699994 39.100788409000074 0,-75.55329332999997 39.101191216000075 0),(-75.55273513599997 39.101084135000065 0,-75.55288693999995 39.10111439600007 0,-75.55289197899998 39.10110587500003 0,-75.55285487799995 39.10106216400004 0,-75.55288545899998 39.10105070700007 0,-75.55289078899995 39.10104343000006 0,-75.55297369599998 39.10099946400004 0,-75.55302623199998 39.10104551200004 0,-75.55304452299998 39.10102677900005 0,-75.55310796299995 39.100821739000025 0,-75.55282236499994 39.10077075500004 0,-75.55273513599997 39.101084135000065 0))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);
        var triangles = GeometryProcessor.GetTriangles(g, 100) ;

        // act
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ogc_fid_13967.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert
        var model = ModelRoot.Load(fileName);
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
    }

    [Test]
    public void CreateGlbWithDefaultColor()
    {
        // arrange
        var wkt = "MULTIPOLYGON Z (((133836.921875 463013.96875 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133830.484375 463000 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522)),((133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133842.046875 463004.65625 -0.250999987125397,133833.359375 462999.84375 2.655999898910522)),((133831.21875 462998.65625 0.360000014305115,133831.21875 462998.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 2.776999950408936,133831.21875 462998.65625 0.360000014305115)),((133828.03125 463009.09375 2.638000011444092,133828.03125 463009.09375 2.555000066757202,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.638000011444092)),((133833.359375 462999.84375 2.776999950408936,133833.359375 462999.84375 2.655999898910522,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.776999950408936)),((133836.921875 463013.96875 -0.331999987363815,133836.921875 463013.96875 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 2.638000011444092,133836.921875 463013.96875 -0.331999987363815)),((133830.484375 463000 0.358999997377396,133830.484375 463000 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133831.21875 462998.65625 0.360000014305115,133830.484375 463000 0.358999997377396)),((133842.046875 463004.65625 -0.250999987125397,133842.046875 463004.65625 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522,133836.921875 463013.96875 -0.331999987363815,133842.046875 463004.65625 -0.250999987125397)),((133826.078125 463008.03125 0.354000002145767,133826.078125 463008.03125 -2.280999898910522,133830.484375 463000 -2.280999898910522,133830.484375 463000 0.358999997377396,133826.078125 463008.03125 0.354000002145767)),((133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133826.078125 463008.03125 0.354000002145767,133828.03125 463009.09375 2.555000066757202)),((133842.046875 463004.65625 -0.250999987125397,133836.921875 463013.96875 -0.331999987363815,133828.03125 463009.09375 2.638000011444092,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.655999898910522,133842.046875 463004.65625 -0.250999987125397)),((133828.03125 463009.09375 2.555000066757202,133826.078125 463008.03125 0.354000002145767,133830.484375 463000 0.358999997377396,133831.21875 462998.65625 0.360000014305115,133833.359375 462999.84375 2.776999950408936,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.555000066757202)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);
        var multipolygon = ((MultiPolygon)g);
        var triangles = GeometryProcessor.GetTriangles(multipolygon, 100);

        // act
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ams_building.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert
        var model = ModelRoot.Load(fileName);
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
    }

    [Test]
    public void CreateGlbWithSingleColor()
    {
        // arrange
        var wkt = "MULTIPOLYGON Z (((133836.921875 463013.96875 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133830.484375 463000 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522)),((133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133842.046875 463004.65625 -0.250999987125397,133833.359375 462999.84375 2.655999898910522)),((133831.21875 462998.65625 0.360000014305115,133831.21875 462998.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 2.776999950408936,133831.21875 462998.65625 0.360000014305115)),((133828.03125 463009.09375 2.638000011444092,133828.03125 463009.09375 2.555000066757202,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.638000011444092)),((133833.359375 462999.84375 2.776999950408936,133833.359375 462999.84375 2.655999898910522,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.776999950408936)),((133836.921875 463013.96875 -0.331999987363815,133836.921875 463013.96875 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 2.638000011444092,133836.921875 463013.96875 -0.331999987363815)),((133830.484375 463000 0.358999997377396,133830.484375 463000 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133831.21875 462998.65625 0.360000014305115,133830.484375 463000 0.358999997377396)),((133842.046875 463004.65625 -0.250999987125397,133842.046875 463004.65625 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522,133836.921875 463013.96875 -0.331999987363815,133842.046875 463004.65625 -0.250999987125397)),((133826.078125 463008.03125 0.354000002145767,133826.078125 463008.03125 -2.280999898910522,133830.484375 463000 -2.280999898910522,133830.484375 463000 0.358999997377396,133826.078125 463008.03125 0.354000002145767)),((133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133826.078125 463008.03125 0.354000002145767,133828.03125 463009.09375 2.555000066757202)),((133842.046875 463004.65625 -0.250999987125397,133836.921875 463013.96875 -0.331999987363815,133828.03125 463009.09375 2.638000011444092,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.655999898910522,133842.046875 463004.65625 -0.250999987125397)),((133828.03125 463009.09375 2.555000066757202,133826.078125 463008.03125 0.354000002145767,133830.484375 463000 0.358999997377396,133831.21875 462998.65625 0.360000014305115,133833.359375 462999.84375 2.776999950408936,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.555000066757202)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);
        var multipolygon = ((MultiPolygon)g);
        var triangles = GeometryProcessor.GetTriangles(multipolygon, 100);

        // act
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ams_building_single_color.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert
        var model = ModelRoot.Load(fileName);
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
    }

    [Test]
    public void CreateGlbWithDegeneratedTriangleShouldNotGiveException()
    {
        var degeneratedTriangle = new Triangle(new Point(0, 0, 0), new Point(0, 0, 0), new Point(0, 0, 0), 0);
        var triangles = new List<Triangle>() { degeneratedTriangle };
        var t = new List<List<Triangle>>() { triangles };
        var bytes = GlbCreator.GetGlb(t);
        // there are 8 small triangles (area) that are removed.
        Assert.That(bytes, Is.EqualTo(null));
    }

    [Test]
    public void CreateGlbWithSingleColorShader()
    {
        // arrange
        var wkt = "MULTIPOLYGON Z (((133836.921875 463013.96875 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133830.484375 463000 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522)),((133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133842.046875 463004.65625 -0.250999987125397,133833.359375 462999.84375 2.655999898910522)),((133831.21875 462998.65625 0.360000014305115,133831.21875 462998.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 2.776999950408936,133831.21875 462998.65625 0.360000014305115)),((133828.03125 463009.09375 2.638000011444092,133828.03125 463009.09375 2.555000066757202,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.638000011444092)),((133833.359375 462999.84375 2.776999950408936,133833.359375 462999.84375 2.655999898910522,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.776999950408936)),((133836.921875 463013.96875 -0.331999987363815,133836.921875 463013.96875 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 2.638000011444092,133836.921875 463013.96875 -0.331999987363815)),((133830.484375 463000 0.358999997377396,133830.484375 463000 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133831.21875 462998.65625 0.360000014305115,133830.484375 463000 0.358999997377396)),((133842.046875 463004.65625 -0.250999987125397,133842.046875 463004.65625 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522,133836.921875 463013.96875 -0.331999987363815,133842.046875 463004.65625 -0.250999987125397)),((133826.078125 463008.03125 0.354000002145767,133826.078125 463008.03125 -2.280999898910522,133830.484375 463000 -2.280999898910522,133830.484375 463000 0.358999997377396,133826.078125 463008.03125 0.354000002145767)),((133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133826.078125 463008.03125 0.354000002145767,133828.03125 463009.09375 2.555000066757202)),((133842.046875 463004.65625 -0.250999987125397,133836.921875 463013.96875 -0.331999987363815,133828.03125 463009.09375 2.638000011444092,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.655999898910522,133842.046875 463004.65625 -0.250999987125397)),((133828.03125 463009.09375 2.555000066757202,133826.078125 463008.03125 0.354000002145767,133830.484375 463000 0.358999997377396,133831.21875 462998.65625 0.360000014305115,133833.359375 462999.84375 2.776999950408936,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.555000066757202)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);
        var multipolygon = ((MultiPolygon)g);
        var shaderColors = new ShaderColors();
        var metallicRoughness = new PbrMetallicRoughnessColors();

        // use 1 basecolor
        var random = new Random();
        var baseColors = new List<string>();
        var color = string.Format("#{0:X6}", random.Next(0x1000000));
        baseColors.Add(color);

        metallicRoughness.BaseColors = baseColors;

        shaderColors.PbrMetallicRoughnessColors = metallicRoughness;

        // act
        var triangles = GeometryProcessor.GetTriangles(multipolygon, 100, shadercolors: shaderColors);
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ams_building_multiple_colors.glb");
        File.WriteAllBytes(fileName, bytes);

        var model = ModelRoot.Load(fileName);

        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
    }


    [Test]
    public void CreateGlbWithShader()
    {
        // arrange
        var wkt = "MULTIPOLYGON Z (((133836.921875 463013.96875 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133830.484375 463000 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522)),((133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133842.046875 463004.65625 -0.250999987125397,133833.359375 462999.84375 2.655999898910522)),((133831.21875 462998.65625 0.360000014305115,133831.21875 462998.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 2.776999950408936,133831.21875 462998.65625 0.360000014305115)),((133828.03125 463009.09375 2.638000011444092,133828.03125 463009.09375 2.555000066757202,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.638000011444092)),((133833.359375 462999.84375 2.776999950408936,133833.359375 462999.84375 2.655999898910522,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.776999950408936)),((133836.921875 463013.96875 -0.331999987363815,133836.921875 463013.96875 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 2.638000011444092,133836.921875 463013.96875 -0.331999987363815)),((133830.484375 463000 0.358999997377396,133830.484375 463000 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133831.21875 462998.65625 0.360000014305115,133830.484375 463000 0.358999997377396)),((133842.046875 463004.65625 -0.250999987125397,133842.046875 463004.65625 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522,133836.921875 463013.96875 -0.331999987363815,133842.046875 463004.65625 -0.250999987125397)),((133826.078125 463008.03125 0.354000002145767,133826.078125 463008.03125 -2.280999898910522,133830.484375 463000 -2.280999898910522,133830.484375 463000 0.358999997377396,133826.078125 463008.03125 0.354000002145767)),((133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133826.078125 463008.03125 0.354000002145767,133828.03125 463009.09375 2.555000066757202)),((133842.046875 463004.65625 -0.250999987125397,133836.921875 463013.96875 -0.331999987363815,133828.03125 463009.09375 2.638000011444092,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.655999898910522,133842.046875 463004.65625 -0.250999987125397)),((133828.03125 463009.09375 2.555000066757202,133826.078125 463008.03125 0.354000002145767,133830.484375 463000 0.358999997377396,133831.21875 462998.65625 0.360000014305115,133833.359375 462999.84375 2.776999950408936,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.555000066757202)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);
        var multipolygon = ((MultiPolygon)g);
        var shaderColors = new ShaderColors();
        var metallicRoughness = new PbrMetallicRoughnessColors();


        // create 30 random basecolors
        var random = new Random();
        var baseColors = new List<string>();
        for (var i = 0; i < 30; i++) {
            var color = string.Format("#{0:X6}", random.Next(0x1000000));
            baseColors.Add(color);
        }

        metallicRoughness.BaseColors = baseColors;

        shaderColors.PbrMetallicRoughnessColors = metallicRoughness;

        // act
        var triangles = GeometryProcessor.GetTriangles(multipolygon, 100, shadercolors: shaderColors);
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ams_building_multiple_colors.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert (each triangle becomes a primitive because colors
        var model = ModelRoot.Load(fileName);

        // there are 8 small triangles (area) that are removed.
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(30));
    }

    [Test]
    public void CreateGlbWithWrongNumberOfColorsGivesArgumentOfRangeException()
    {
        // arrange
        var wkt = "MULTIPOLYGON Z (((133836.921875 463013.96875 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133830.484375 463000 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522)),((133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 -2.280999898910522,133842.046875 463004.65625 -2.280999898910522,133842.046875 463004.65625 -0.250999987125397,133833.359375 462999.84375 2.655999898910522)),((133831.21875 462998.65625 0.360000014305115,133831.21875 462998.65625 -2.280999898910522,133833.359375 462999.84375 -2.280999898910522,133833.359375 462999.84375 2.655999898910522,133833.359375 462999.84375 2.776999950408936,133831.21875 462998.65625 0.360000014305115)),((133828.03125 463009.09375 2.638000011444092,133828.03125 463009.09375 2.555000066757202,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.638000011444092)),((133833.359375 462999.84375 2.776999950408936,133833.359375 462999.84375 2.655999898910522,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.776999950408936)),((133836.921875 463013.96875 -0.331999987363815,133836.921875 463013.96875 -2.280999898910522,133828.03125 463009.09375 -2.280999898910522,133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 2.638000011444092,133836.921875 463013.96875 -0.331999987363815)),((133830.484375 463000 0.358999997377396,133830.484375 463000 -2.280999898910522,133831.21875 462998.65625 -2.280999898910522,133831.21875 462998.65625 0.360000014305115,133830.484375 463000 0.358999997377396)),((133842.046875 463004.65625 -0.250999987125397,133842.046875 463004.65625 -2.280999898910522,133836.921875 463013.96875 -2.280999898910522,133836.921875 463013.96875 -0.331999987363815,133842.046875 463004.65625 -0.250999987125397)),((133826.078125 463008.03125 0.354000002145767,133826.078125 463008.03125 -2.280999898910522,133830.484375 463000 -2.280999898910522,133830.484375 463000 0.358999997377396,133826.078125 463008.03125 0.354000002145767)),((133828.03125 463009.09375 2.555000066757202,133828.03125 463009.09375 -2.280999898910522,133826.078125 463008.03125 -2.280999898910522,133826.078125 463008.03125 0.354000002145767,133828.03125 463009.09375 2.555000066757202)),((133842.046875 463004.65625 -0.250999987125397,133836.921875 463013.96875 -0.331999987363815,133828.03125 463009.09375 2.638000011444092,133830.1875 463005.34375 2.644999980926514,133833.359375 462999.84375 2.655999898910522,133842.046875 463004.65625 -0.250999987125397)),((133828.03125 463009.09375 2.555000066757202,133826.078125 463008.03125 0.354000002145767,133830.484375 463000 0.358999997377396,133831.21875 462998.65625 0.360000014305115,133833.359375 462999.84375 2.776999950408936,133830.1875 463005.34375 2.644999980926514,133828.03125 463009.09375 2.555000066757202)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);
        var multipolygon = ((MultiPolygon)g);

        var shaderColors = new ShaderColors();
        var metallicRoughness = new PbrMetallicRoughnessColors();
        metallicRoughness.BaseColors = (from geo in multipolygon.Geometries
                                        let random = new Random()
                                        let color = String.Format("#{0:X6}", random.Next(0x1000000))
                                        select color).ToList();

        // accidentally remove 1:
        metallicRoughness.BaseColors.RemoveAt(metallicRoughness.BaseColors.Count - 1);

        var specularGlosiness = new PbrSpecularGlossinessColors();
        specularGlosiness.DiffuseColors = metallicRoughness.BaseColors;

        shaderColors.PbrMetallicRoughnessColors = metallicRoughness;
        shaderColors.PbrSpecularGlossinessColors = specularGlosiness;

        // act
        try {
            var triangles = GeometryProcessor.GetTriangles(multipolygon, 100, shadercolors: shaderColors);
        }
        catch (Exception ex) {
            // assert
            Assert.That(ex != null, Is.True);
            Assert.That(ex is ArgumentOutOfRangeException, Is.True);
            Assert.That(ex.Message.Contains("Diffuse, BaseColor"), Is.True);
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

        var materialGreen = new MaterialBuilder().
            WithDoubleSide(true).
            WithMetallicRoughnessShader().
            WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(0, 1, 0, 1));

        var materialWhite = new MaterialBuilder().
            WithDoubleSide(true).
            WithMetallicRoughnessShader().
            WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 1, 1, 1));

        var mesh = new MeshBuilder<VertexPositionNormal>("mesh");
        DrawTriangle(triangle1, materialWhite, mesh);
        DrawTriangle(triangle2, materialGreen, mesh);
        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);

        // act
        var model = scene.ToGltf2();

        // assert
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(2));
    }

    [Test]
    public static void CreateGlbForNonTriangulatedGeometry()
    {
        // arrange
        var wkt = "POLYHEDRALSURFACE Z (((0 0 0, 0 1 0, 1 1 0, 1 0 0, 0 0 0)),((0 0 0, 0 1 0, 0 1 1, 0 0 1, 0 0 0)), ((0 0 0, 1 0 0, 1 0 1, 0 0 1, 0 0 0)), ((1 1 1, 1 0 1, 0 0 1, 0 1 1, 1 1 1)),((1 1 1, 1 0 1, 1 0 0, 1 1 0, 1 1 1)),((1 1 1, 1 1 0, 0 1 0, 0 1 1, 1 1 1)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        // act
        var triangles = GeometryProcessor.GetTriangles(g, 100);

        // assert
        Assert.That(((PolyhedralSurface)g).Geometries.Count, Is.EqualTo(6));
        Assert.That(triangles.Count, Is.EqualTo(12));
    }

    [Test]
    public static void CreateGlbForMultiLineString()
    {
        var pipelineWkt = "MULTILINESTRING Z ((-10 0 0,0 0 0,0 10 0), (5 0 0, 45 0 0))";
        var g = Geometry.Deserialize<WktSerializer>(pipelineWkt);
        var translation = new double[] { 0, 0, 0 };
        var triangles = GeometryProcessor.GetTriangles(g, 100, translation);
        Assert.That(triangles.Count, Is.EqualTo(48));

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "multilinestring.glb");
        File.WriteAllBytes(@"multilinestring.glb", bytes);
    }
    
    [Test]
    public static void CreateGlbForTin1()
    {
        var pipelineWkt = "TIN Z (((0 0 0,0 0 1,0 1 0,0 0 0)),((0 0 0,0 1 0,1 1 0,0 0 0)))";
        var g = Geometry.Deserialize<WktSerializer>(pipelineWkt);
        var translation = new double[] { 0, 0, 0 };

        var triangles = GeometryProcessor.GetTriangles(g, 100, translation);
        Assert.That(triangles.Count, Is.EqualTo(2));

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tin.glb");
        File.WriteAllBytes(@"tin.glb", bytes);
    }

    [Test]
    public static void CreateGlbForLineString()
    {
        var pipelineWkt = "LINESTRING Z (3889684.581681901 332934.74905057804 5026963.34752543,3889660.4475074895 333024.45069048833 5026975.961092257,3889653.556938037 333049.9679558418 5026979.669808809,3889653.7643391364 333050.8015457351 5026979.4833347425)";
        var g = Geometry.Deserialize<WktSerializer>(pipelineWkt);
        var translation = new double[] { 3889587.5, 333387.5625, 5026956 };
        var triangles = GeometryProcessor.GetTriangles(g, 100, translation);
        Assert.That(triangles.Count, Is.EqualTo(48));
    }

    [Test]
    public static void CreateGlbForSimpleBuilding()
    {
        // arrange
        var buildingDelawareWkt = "POLYHEDRALSURFACE Z (((1237196.52254261 -4794569.11324542 4006730.36853675,1237205.09930114 -4794565.00723136 4006732.61840877,1237198.22281801 -4794557.02527831 4006744.21497578,1237196.52254261 -4794569.11324542 4006730.36853675)),((1237198.22281801 -4794557.02527831 4006744.21497578,1237189.64607418 -4794561.13128501 4006741.96510802,1237196.52254261 -4794569.11324542 4006730.36853675,1237198.22281801 -4794557.02527831 4006744.21497578)),((1237199.14544946 -4794579.27792655 4006738.92021596,1237207.72222617 -4794575.17190377 4006741.17009276,1237200.84572844 -4794567.18993371 4006752.76668446,1237199.14544946 -4794579.27792655 4006738.92021596)),((1237200.84572844 -4794567.18993371 4006752.76668446,1237192.26896643 -4794571.29594914 4006750.51681191,1237199.14544946 -4794579.27792655 4006738.92021596,1237200.84572844 -4794567.18993371 4006752.76668446)),((1237205.09930114 -4794565.00723136 4006732.61840877,1237196.52254261 -4794569.11324542 4006730.36853675,1237207.72222617 -4794575.17190377 4006741.17009276,1237205.09930114 -4794565.00723136 4006732.61840877)),((1237207.72222617 -4794575.17190377 4006741.17009276,1237199.14544946 -4794579.27792655 4006738.92021596,1237196.52254261 -4794569.11324542 4006730.36853675,1237207.72222617 -4794575.17190377 4006741.17009276)),((1237196.52254261 -4794569.11324542 4006730.36853675,1237189.64607418 -4794561.13128501 4006741.96510802,1237199.14544946 -4794579.27792655 4006738.92021596,1237196.52254261 -4794569.11324542 4006730.36853675)),((1237199.14544946 -4794579.27792655 4006738.92021596,1237192.26896643 -4794571.29594914 4006750.51681191,1237189.64607418 -4794561.13128501 4006741.96510802,1237199.14544946 -4794579.27792655 4006738.92021596)),((1237189.64607418 -4794561.13128501 4006741.96510802,1237198.22281801 -4794557.02527831 4006744.21497578,1237192.26896643 -4794571.29594914 4006750.51681191,1237189.64607418 -4794561.13128501 4006741.96510802)),((1237192.26896643 -4794571.29594914 4006750.51681191,1237200.84572844 -4794567.18993371 4006752.76668446,1237198.22281801 -4794557.02527831 4006744.21497578,1237192.26896643 -4794571.29594914 4006750.51681191)),((1237198.22281801 -4794557.02527831 4006744.21497578,1237205.09930114 -4794565.00723136 4006732.61840877,1237200.84572844 -4794567.18993371 4006752.76668446,1237198.22281801 -4794557.02527831 4006744.21497578)),((1237200.84572844 -4794567.18993371 4006752.76668446,1237207.72222617 -4794575.17190377 4006741.17009276,1237205.09930114 -4794565.00723136 4006732.61840877,1237200.84572844 -4794567.18993371 4006752.76668446)))";
        var colors = new List<string>() { "#385E0F", "#385E0F", "#FF0000", "#FF0000", "#EEC900", "#EEC900", "#EEC900", "#EEC900", "#EEC900", "#EEC900", "#EEC900", "#EEC900" };
        var g = Geometry.Deserialize<WktSerializer>(buildingDelawareWkt);
        var polyhedralsurface = ((PolyhedralSurface)g);
        var center = polyhedralsurface.GetCenter();

        var shaderColors = new ShaderColors();
        var metallicRoughness = new PbrMetallicRoughnessColors();
        metallicRoughness.BaseColors = colors;
        shaderColors.PbrMetallicRoughnessColors = metallicRoughness;

        var triangles = GeometryProcessor.GetTriangles(polyhedralsurface, 100, shadercolors: shaderColors);
        CheckNormal(triangles[2], center);
        Assert.That(triangles.Count == 12, Is.True);

        // act
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "simle_building.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert
    }

    private static void CheckNormal(Triangle t, Point center)
    {
        // arrange
        var normal = t.GetNormal();
        var p0 = t.ToVectors().Item1;
        var vertexDistance = (p0 - center.ToVector()).Length();
        var withNormalDistance = (p0 + normal - center.ToVector()).Length();

        // assert
        Assert.That(withNormalDistance > vertexDistance, Is.True);
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
