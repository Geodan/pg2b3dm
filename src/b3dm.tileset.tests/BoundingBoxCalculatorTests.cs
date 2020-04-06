using System.Collections.Generic;
using Npgsql;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class BoundingBoxCalculatorTests
    {
        [Test]
        public void BoundingBoxCalculatorTest()
        {
            // arrange
            var bboxes = new List<BoundingBox3D>();
            var bbox1 = new BoundingBox3D(0, 0, 0, 1, 1, 1);
            bboxes.Add(bbox1);

            // act
            var box = BoundingBoxCalculator.GetBoundingBox(bboxes);

            // assert
            Assert.IsTrue(box.XMin == 0);
            Assert.IsTrue(box.YMin == 0);
            Assert.IsTrue(box.XMax == 1);
            Assert.IsTrue(box.YMax == 1);
        }
        private string connectionString = "Host=localhost;Username=postgres;Database=postgres;Port=5432;password=postgres";
        private string geometryTable = "delaware_buildings_dupe";
        private string geometryColumn = "geom_triangle";


        [Test]
        public void RotateAndTranslateTest()
        {
            var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var ids = new List<string> { "7072" };
            var translation = new double[] { -8406745.007853176, 4744614.257728589, 38.29 };
            var bvol3dActual = TileCutter.GetTileBoundingBoxNew(conn, geometryTable, geometryColumn, "id", translation, ids.ToArray());
            conn.Close();
            var bb3dExpected = new BoundingBox3D(-6432.158566314727, 1733.985445620492, -38.28999999999989, -6406.6595011129975, 1733.985445620492, -25.61999999999989);
            Assert.IsTrue(bvol3dActual.Equals(bb3dExpected));

            var bb3d = new BoundingBox3D(-8413177.166419491, 4746324.27552427, 0, -8413151.66735429, 4746348.243174209, 12.67);
            var actualRotatedAndTranslatedBoundingVolumne = BoundingBoxCalculator.RotateTranslateTransform(bb3d, translation);
            Assert.IsTrue(actualRotatedAndTranslatedBoundingVolumne.Equals(bb3dExpected));

        }
    }
}
