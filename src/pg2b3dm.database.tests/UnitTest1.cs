using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using B3dm.Tileset.settings;
using DotNet.Testcontainers.Builders;
using Npgsql;
using subtree;
using Testcontainers.PostgreSql;
using Wkb2Gltf;

namespace pg2b3dm.database.tests;

public class UnitTest1
{
    private PostgreSqlContainer _containerPostgres;

    [SetUp]
    public async Task Setup()
    {
        _containerPostgres = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:16-3.4-alpine")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
        .Build();
        await _containerPostgres.StartAsync();
        var initScript1 = File.ReadAllText("./postgres-db/1_create_delaware_table.sql");
        await _containerPostgres.ExecScriptAsync(initScript1);
        var initScript2 = File.ReadAllText("./postgres-db/2_create_delaware_table.sql");
        await _containerPostgres.ExecScriptAsync(initScript2);
        var initScript3 = File.ReadAllText("./postgres-db/3_create_geom_table.sql");
        await _containerPostgres.ExecScriptAsync(initScript3);
        var initScript4 = File.ReadAllText("./postgres-db/4_create_arvieux_batiments.sql");
        await _containerPostgres.ExecScriptAsync(initScript4);
    }

    [TearDown]
    public async Task TeardownOnce()
    {
        await _containerPostgres.StopAsync();
        await _containerPostgres.DisposeAsync();
    }

    [Test]
    public void TestArvieuxBuildingsOctree()
    {
        OutputDirectoryCreator.GetFolders("output");
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var bbox_table = BoundingBoxRepository.GetBoundingBoxForTable(conn, "arvieux_batiments", "geom");

        var center_wgs84 = bbox_table.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y,  };

        var bbox = bbox_table.bbox;
        var zmin = bbox_table.zmin;
        var zmax = bbox_table.zmax;

        var boundingBox3D = new BoundingBox3D() { XMin = bbox.XMin, YMin = bbox.YMin, ZMin = zmin, XMax = bbox.XMax, YMax = bbox.YMax, ZMax = zmax };

        var inputTable = new InputTable() {
            TableName = "arvieux_batiments",
            GeometryColumn = "geom",
            EPSGCode = 5698,
            AttributeColumns = "id"
        };
        var tilingSettings = new TilingSettings()
        {
            MaxFeaturesPerTile = 50,
            SkipCreateTiles = true,
            BoundingBox = bbox_table.bbox,
        };

        var stylingSettings = new StylingSettings();

        var tilesetSettings = new TilesetSettings() {
            Translation = trans
        };

        var implicitTiler = new OctreeTiler(connectionString, inputTable, tilingSettings, stylingSettings, tilesetSettings);
        var tiles = implicitTiler.GenerateTiles3D(boundingBox3D, 0, new Tile3D(0, 0, 0, 0 ), new List<Tile3D>());

        Assert.That(tiles.Count, Is.EqualTo(25));
    }

    [Test]
    public void TestArvieuxBuildingsOctreeKeepProjection()
    {
        OutputDirectoryCreator.GetFolders("output");
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var bbox_table = BoundingBoxRepository.GetBoundingBoxForTable(conn, "arvieux_batiments", "geom", true);

        var center = bbox_table.bbox.GetCenter();
        var trans = new double[] { center.X ?? throw new InvalidOperationException("center.X is null"), center.Y ?? throw new InvalidOperationException("center.Y is null"), 0 };

        var bbox = bbox_table.bbox;
        var zmin = bbox_table.zmin;
        var zmax = bbox_table.zmax;

        var boundingBox3D = new BoundingBox3D() { XMin = bbox.XMin, YMin = bbox.YMin, ZMin = zmin, XMax = bbox.XMax, YMax = bbox.YMax, ZMax = zmax };

        var inputTable = new InputTable() {
            TableName = "arvieux_batiments",
            GeometryColumn = "geom",
            EPSGCode = 5698
        };
        var tilingSettings = new TilingSettings() {
            MaxFeaturesPerTile = 50,
            SkipCreateTiles = true,
            BoundingBox = bbox_table.bbox,
            KeepProjection = true
        };

        var stylingSettings = new StylingSettings();

        var tilesetSettings = new TilesetSettings() {
            Translation = trans
        };

        var implicitTiler = new OctreeTiler(connectionString, inputTable, tilingSettings, stylingSettings, tilesetSettings);
        var tiles = implicitTiler.GenerateTiles3D(boundingBox3D, 0, new Tile3D(0, 0, 0, 0), new List<Tile3D>());

        Assert.That(tiles.Count, Is.EqualTo(25));
    }


    [Test]
    public void HasSpatialIndexTest()
    {
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var hasSpatialIndex = SpatialIndexChecker.HasSpatialIndex(conn, "delaware_buildings", "geom_triangle");
        Assert.That(hasSpatialIndex == false);
    }

    [Test]
    public void FirstTest()
    {
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var sql = "select count(*) from delaware_buildings";
        var records = DatabaseReader.ReadScalar(conn, sql);
        Assert.That(records, Is.EqualTo(360));
    }

    [Test]
    public void ImplicitTilingTest()
    {
        OutputDirectoryCreator.GetFolders("output");
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, "delaware_buildings", "geom_triangle");

        var center_wgs84 = bbox_wgs84.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y, translation.Z };
        var inputTable = new InputTable()
        {
            TableName = "delaware_buildings",
            GeometryColumn = "geom_triangle",
            EPSGCode = 4326
        };


        var stylingSettings = new StylingSettings();

        var implicitTiler = new 
            QuadtreeTiler(connectionString, inputTable, stylingSettings, 50,
            trans,
            "output/content",
            new List<int>() { 0 },
            skipCreateTiles: true,
            useImplicitTiling: true);
        var tiles = implicitTiler.GenerateTiles(
        bbox_wgs84.bbox,
        new Tile(0, 0, 0),
        new List<Tile>());
        Assert.That(tiles.Count, Is.EqualTo(17));
    }

    [Test]
    public void LodTest()
    {
        OutputDirectoryCreator.GetFolders("output");
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, "delaware_buildings_lod", "geom_triangle");

        var center_wgs84 = bbox_wgs84.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y, translation.Z };
        var inputTable = new InputTable()
        {
            TableName = "delaware_buildings_lod",
            GeometryColumn = "geom_triangle",
            ShadersColumn = "shaders",
            LodColumn = "lodcolumn",
            EPSGCode = 4326
        };
        var stylingSettings = new StylingSettings();
        var implicitTiler = new QuadtreeTiler(connectionString, 
            inputTable, stylingSettings, 10,
            trans,
            "output/content",
            new List<int>() { 0, 1 },
            skipCreateTiles: true,
            useImplicitTiling: true);
        var tiles = implicitTiler.GenerateTiles(
        bbox_wgs84.bbox,
        new Tile(0, 0, 0),
        new List<Tile>());
        Assert.That(tiles.Count, Is.EqualTo(89));
    }
    
    
    [Test]
    public void GeometryTest()
    {
        OutputDirectoryCreator.GetFolders("output");
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, "geom_test", "geom3d");
        var center_wgs84 = bbox_wgs84.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y, translation.Z };
        var inputTable = new InputTable()
        {
            TableName = "geom_test",
            GeometryColumn = "geom3d",
            EPSGCode = 4326
        };
        var stylingSettings = new StylingSettings();
        var implicitTiler = new QuadtreeTiler(connectionString, inputTable, stylingSettings, 50,
            trans,
            "output/content",
            new List<int>() { 0 },
            skipCreateTiles: false,
            useImplicitTiling: true);
        var tile = new Tile(0, 0, 0) {
            BoundingBox = bbox_wgs84.bbox.ToArray()
        };
        var tiles = implicitTiler.GenerateTiles(
            bbox_wgs84.bbox,
            tile,
            new List<Tile>(), createGltf:true);
        Assert.That(tiles.Count, Is.EqualTo(1));
    }
}
