using System.Collections.Generic;
using Npgsql;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class BoundingBoxCalculatorTests
    {
        private string connectionString = "Host=localhost;Username=postgres;Database=postgres;Port=5432;password=postgres";
        private string geometryTable = "delaware_buildings_dupe";
        private string geometryColumn = "geom_triangle";


        [Test]
        public void RotateAndTranslateTest()
        {
            var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var ids = new List<string> { "7072"};
            var translation = new double[] { -8406745.007853176, 4744614.257728589, 38.29 };
            var bb3dExpected = new BoundingBox3D(-6432.158566314727, 1710.0178, -38.29, -6406.6595011129975, 1733.98545, -25.62);
            var extent3d = BoundingBoxRepository.Get3DExtent(conn, geometryTable, geometryColumn, "id", ids.ToArray());
            var extent3drotated = BoundingBoxCalculator.RotateTranslateTransform(extent3d, translation);
            Assert.IsTrue(extent3drotated.Equals(bb3dExpected));
            conn.Close();
        }

    }
}
