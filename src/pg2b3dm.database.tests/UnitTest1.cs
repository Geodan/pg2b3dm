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
    }
}
