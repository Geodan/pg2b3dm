using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using Microsoft.Extensions.Configuration;
using Npgsql;
using subtree;

namespace pg2b3dm.database.tests;

public class UnitTest1
{
    [Test]
    public void FirstTest()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var conn = new NpgsqlConnection(config["DB_CONNECTION_STRING"]);
        var sql = "select count(*) from delaware_buildings";
        var records = DatabaseReader.ReadScalar(conn, sql);
        Assert.IsTrue(records == 360);
    }

    [Test]
    public void ImplicitTilingTest()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var conn = new NpgsqlConnection(config["DB_CONNECTION_STRING"]);
        var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, "delaware_buildings", "geom_triangle");

        var center_wgs84 = bbox_wgs84.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);

        var implicitTiler = new QuadtreeTiler(conn, "delaware_buildings", 4978, "geom_triangle", 50, string.Empty,
            new double[] { translation.X, translation.Y, translation.Z },
            "shaders",
            string.Empty,
            string.Empty,
            "output/content",
            new List<int>() { 0 },
            skipCreateTiles: true);        
            var tiles = implicitTiler.GenerateTiles(
            bbox_wgs84,
            new Tile(0,0,0),
            new List<Tile>());
        Assert.IsTrue(tiles.Count == 29);
    }

    [Test]
    public void LodTest()
    {

        Directory.CreateDirectory("output/content");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var conn = new NpgsqlConnection(config["DB_CONNECTION_STRING"]);
        var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, "delaware_buildings_lod", "geom_triangle");

        var center_wgs84 = bbox_wgs84.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);

        var implicitTiler = new QuadtreeTiler(conn, "delaware_buildings_lod", 4978, "geom_triangle", 10, string.Empty,
            new double[] { translation.X, translation.Y, translation.Z },
            "shaders",
            string.Empty,
            "lodcolumn",
            "output/content",
            new List<int>() { 0,1 },
            skipCreateTiles: false);
        var tiles = implicitTiler.GenerateTiles(
        bbox_wgs84,
        new Tile(0, 0, 0),
        new List<Tile>());
        Assert.IsTrue(tiles.Count == 125);

    }
}
