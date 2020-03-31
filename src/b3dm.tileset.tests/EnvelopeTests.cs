using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class EnvelopeTests
    {
        private string connectionString = "Host=localhost;Username=postgres;Database=postgres;Port=5432;password=postgres";
        private string geometryTable = "delaware_buildings";
        private string geometryColumn = "geom_triangle";

        [Test]
        public void GetSpatialReferenceTest()
        {
            var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var actualSR = SpatialReferenceRepository.GetSpatialReference(conn, geometryTable, geometryColumn);

            var expectedSR = 3857;

            Assert.IsTrue(expectedSR == actualSR);

            conn.Close();
        }
    }
}
