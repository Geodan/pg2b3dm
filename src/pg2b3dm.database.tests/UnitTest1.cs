using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using DotNet.Testcontainers.Builders;
using Npgsql;
using NUnit.Framework;
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
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();
        await _containerPostgres.StartAsync();
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
