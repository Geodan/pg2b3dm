using System.Data;
using System.Runtime.InteropServices;
using B3dm.Tileset;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Wkx;

namespace pg2b3dm.tests
{
    public class Tests
    {
        private string db = @"testfixtures/delaware.sqlite";

        [Test]
        public void ReadGeometry()
        {
            var connectString = $"Data Source={db}";
            var connection = new SqliteConnection(connectString);
            connection.Open();
            SpatialLoader(connection);
            var sql = "select ST_ASBinary(GEOMETRY)as geometry from bldg_footprints";
            var polygon = GetGeometry(connection, sql);
            connection.Close();
            Assert.IsTrue(polygon.GeometryType == GeometryType.Polygon);
        }

        [Test]
        public void CountTest()
        {
            var connectString = $"Data Source={db}";
            var connection = new SqliteConnection(connectString);
            connection.Open();
            SpatialLoader(connection);
            var sql = "SELECT count(*) FROM bldg_footprints";
            var res = DatabaseReader.ReadScalar(connection, sql);
            connection.Close();
            Assert.AreEqual(res, 22532);
        }

        private void SpatialLoader(SqliteConnection connection)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                SpatialiteLoader.Load(connection);
            }
            else {
                connection.LoadExtension("mod_spatialite");
            }
        }

        private Geometry GetGeometry(IDbConnection conn, string sql)
        {
            var command = conn.CreateCommand();
            command.CommandText = sql;
            var reader = command.ExecuteReader();
            reader.Read();
            var res = (byte[])reader.GetValue(0);
            Assert.IsTrue(res.Count() == 109);

            var geom = Geometry.Deserialize<WkbSerializer>(new MemoryStream(res));
            return geom;
        }
    }
}