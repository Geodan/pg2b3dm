using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using Microsoft.Extensions.Configuration;
using Npgsql;
using subtree;
using Wkb2Gltf;

namespace pg2b3dm.database.tests;

public class UnitTest1
{
    [Test]
    public void HasSpatialIndexTest() {

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var conn = new NpgsqlConnection(config["DB_CONNECTION_STRING"]);
        var hasSpatialIndex = SpatialIndexChecker.HasSpatialIndex(conn, "delaware_buildings", "geom_triangle");
        Assert.That(hasSpatialIndex==false);
    }


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
        Assert.That(records, Is.EqualTo(360));
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

        var center_wgs84 = bbox_wgs84.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y, translation.Z };
        var implicitTiler = new QuadtreeTiler(conn, "delaware_buildings", 4326, "geom_triangle", 50, string.Empty,
            trans,
            string.Empty,
            string.Empty,
            "output/content",
            new List<int>() { 0 },
            skipCreateTiles: true);        
            var tiles = implicitTiler.GenerateTiles(
            bbox_wgs84.bbox,
            new Tile(0,0,0),
            new List<Tile>());
        Assert.That(tiles.Count, Is.EqualTo(29));
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

        var center_wgs84 = bbox_wgs84.bbox.GetCenter();
        var translation = SpatialConverter.GeodeticToEcef((double)center_wgs84.X!, (double)center_wgs84.Y!, 0);
        var trans = new double[] { translation.X, translation.Y, translation.Z };

        var implicitTiler = new QuadtreeTiler(conn, "delaware_buildings_lod", 4326, "geom_triangle", 10, string.Empty,
            trans,
            string.Empty,
            "lodcolumn",
            "output/content",
            new List<int>() { 0,1 },
            skipCreateTiles: true);
        var tiles = implicitTiler.GenerateTiles(
        bbox_wgs84.bbox,
        new Tile(0, 0, 0),
        new List<Tile>());
        Assert.That(tiles.Count, Is.EqualTo(145));
    }
}
