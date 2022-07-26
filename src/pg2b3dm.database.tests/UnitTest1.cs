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
            Assert.IsTrue(records == 22532);
        }

        [Test]
        public void ImplicitTilingTest()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var conn = new NpgsqlConnection(config["DB_CONNECTION_STRING"]);
            var bbox = new Wkx.BoundingBox(1231256.4091099831, -4800453.8964564484, 1244883.5968566877, -4791281.9185517933);
            var tiles = ImplicitTiling.GenerateTiles("delaware_buildings", conn, 4978, "geom_triangle", "id",
                bbox,
                1000,
                new subtree.Tile(0,0,0),
                new List<subtree.Tile>(),
                string.Empty,
                new double[] { 1238070.0029833354, -4795867.9075041208, 4006102.3617460253 },
                "shaders",
                string.Empty,
                "output/content",
                skipCreateTiles: true);
            Assert.IsTrue(tiles.Count == 67);
        }
    }
}
