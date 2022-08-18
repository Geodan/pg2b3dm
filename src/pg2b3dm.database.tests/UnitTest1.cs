using B3dm.Tileset;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace pg2b3dm.database.tests
{
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

        //[Test]
        public void ImplicitTilingTest()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var conn = new NpgsqlConnection(config["DB_CONNECTION_STRING"]);
            var bbox3d = BoundingBoxRepository.GetBoundingBox3DForTable(conn, "delaware_buildings", "geom_triangle");
            var bbox = new Wkx.BoundingBox(bbox3d.XMin, bbox3d.YMin, bbox3d.XMax, bbox3d.YMax);

            var translation = bbox3d.GetCenter().ToVector();

            var tiles = ImplicitTiling.GenerateTiles("delaware_buildings", conn, 4978, "geom_triangle", "id",
                bbox,
                50,
                new Tile(0,0,0),
                new List<Tile>(),
                string.Empty,
                translation,
                "shaders",
                string.Empty,
                "output/content",
                skipCreateTiles: true);
            Assert.IsTrue(tiles.Count == 16);
        }
    }
}
