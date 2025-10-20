using B3dm.Tileset;
using B3dm.Tileset.Extensions;
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
    }

    [TearDown]
    public async Task TeardownOnce()
    {
        await _containerPostgres.StopAsync();
        await _containerPostgres.DisposeAsync();
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
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, "delaware_buildings", "geom_triangle");

        var center_wgs84 = bbox_wgs84.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y, translation.Z };
        var implicitTiler = new QuadtreeTiler(conn, "delaware_buildings", 4326, "geom_triangle", 50, string.Empty,
            trans,
            "shaders",
            string.Empty,
            string.Empty,
            "output/content",
            new List<int>() { 0 },
            skipCreateTiles: true);
        var tiles = implicitTiler.GenerateTiles(
        bbox_wgs84.bbox,
        new Tile(0, 0, 0),
        new List<Tile>());
        Assert.That(tiles.Count, Is.EqualTo(29));
    }

    [Test]
    public void LodTest()
    {

        Directory.CreateDirectory("output/content");
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, "delaware_buildings_lod", "geom_triangle");

        var center_wgs84 = bbox_wgs84.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y, translation.Z };

        var implicitTiler = new QuadtreeTiler(conn, "delaware_buildings_lod", 4326, "geom_triangle", 10, string.Empty,
            trans,
            "shaders",
            string.Empty,
            "lodcolumn",
            "output/content",
            new List<int>() { 0, 1 },
            skipCreateTiles: true);
        var tiles = implicitTiler.GenerateTiles(
        bbox_wgs84.bbox,
        new Tile(0, 0, 0),
        new List<Tile>());
        Assert.That(tiles.Count, Is.EqualTo(145));
    }
    
    
    [Test]
    public void GeometryTest()
    {
        var connectionString = _containerPostgres.GetConnectionString();
        var conn = new NpgsqlConnection(connectionString);
        var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, "geom_test", "geom3d");
        Directory.CreateDirectory("output/content");
        var center_wgs84 = bbox_wgs84.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y, translation.Z };
        var implicitTiler = new QuadtreeTiler(conn, "geom_test", 4326, "geom3d", 50, string.Empty,
            trans,
            string.Empty,
            string.Empty,
            string.Empty,
            "output/content",
            new List<int>() { 0 },
            skipCreateTiles: false);
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
