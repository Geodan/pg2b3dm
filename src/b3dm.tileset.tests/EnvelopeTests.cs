using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class EnvelopeTests
    {
        [Test]
        public void FirstTest()
        {
            var connectionString = "Host=localhost;Username=postgres;Database=postgres;Port=5432;password=postgres";
            var geometryTable = "delaware_buildings";
            var geometryColumn = "geom_triangle";
            var idcolumn = "id";
            var conn = new NpgsqlConnection(connectionString);
            conn.Open();


            var translation = new double[] { 1238070.0029833354, -4795867.9075041208, 4006102.3617460253 };
            var ids = "9467,12713,11816,10290,2392,11186,2569,693,4293,20337,7074,7754,6015,11580,6430,9859,4211,4699,7075,7078,1060,13092,1489,13561,861,1263,1515,12805,3965,2972,7079";
            var selectedIds = ids.Split(',');

            /**
            var zupBoxes = TileCutter.GetZupBoxes(conn, geometryTable, geometryColumn, idcolumn, translation);
            var tileZupBoxes = new List<BoundingBox3D>();
            foreach(var bb in zupBoxes) {
                if (selectedIds.Contains(bb.Id)) {
                    tileZupBoxes.Add(bb);
                }
            }*/

            // var bvol = TileCutter.GetBoundingvolume(tileZupBoxes);
            var bvol1 = TileCutter.GetTileBoundingBoxNew(conn, geometryTable, geometryColumn, idcolumn, translation, selectedIds);

            Assert.IsTrue(bvol1.box.SequenceEqual(new double[] { -4984.148, 809.148, 1281.767, 181.813, 0, 0, 0, 166.279, 0, 0, 0, 242.157 }));

            conn.Close();
        }


    }
}
