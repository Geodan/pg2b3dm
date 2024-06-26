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

namespace Wkb2Gltf.Tests;

public class GlbCreatorTests
{
    [SetUp]
    public void SetUp()
    {
        Tiles3DExtensions.RegisterExtensions();
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
        attributes.Add("id_matrix4x4", new List<object>() { new decimal[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } });
        attributes.Add("id_shorts", new List<object>() { new short[] { 0, 1 } });
        attributes.Add("id_booleans", new List<object>() { new bool[] { true, false } });
        attributes.Add("id_strings", new List<object>() { new string[] { "hallo", "hallo1"} });
        attributes.Add("id_ints", new List<object>() { new [] { 1, 2 } });
        attributes.Add("id_longs", new List<object>() { new long[] { 1, 2 } });
        attributes.Add("id_floats", new List<object>() { new float[] { 1, 2 } });
        attributes.Add("id_doubles", new List<object>() { new double[] { 1, 2 } });
        attributes.Add("id_decimals", new List<object>() { new decimal[] { 1, 2 } });

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
        File.WriteAllBytes(@"test.glb", bytes);
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
        Assert.That(triangles.Count, Is.EqualTo(2048));

        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "multilinestring.glb");
        File.WriteAllBytes(@"multilinestring.glb", bytes);
    }

    [Test]
    public static void CreateGlbForLineString()
    {
        var pipelineWkt = "LINESTRING Z (3889684.581681901 332934.74905057804 5026963.34752543,3889660.4475074895 333024.45069048833 5026975.961092257,3889653.556938037 333049.9679558418 5026979.669808809,3889653.7643391364 333050.8015457351 5026979.4833347425)";
        var g = Geometry.Deserialize<WktSerializer>(pipelineWkt);
        var translation = new double[] { 3889587.5, 333387.5625, 5026956 };
        var triangles = GeometryProcessor.GetTriangles(g, 100, translation);
        Assert.That(triangles.Count, Is.EqualTo(1024));
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
