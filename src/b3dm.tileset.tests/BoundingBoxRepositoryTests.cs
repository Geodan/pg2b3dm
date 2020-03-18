using System;
using System.Linq;
using Npgsql;
using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests
{
    public class BoundingBoxRepositoryTests
    {
        [Test]
        public void GetBoundingBox3D()
        {
            var connectionString = "Host=localhost;Username=postgres;Database=postgres;Port=5432;password=postgres";
            var geometryTable = "delaware_buildings";
            var geometryColumn = "geom_triangle";
            var idcolumn = "id";

            var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            var bbActual = BoundingBoxRepository.GetBoundingBox3D(conn, geometryTable, geometryColumn);
            var bbExpected = new BoundingBox3D(1231256.4091099831, -4800453.896456448, 4000024.663498499, 1244883.5968566877, -4791281.918551793, 4012180.059993551);
            Assert.IsTrue(bbActual.Equals(bbExpected));

            var actualTranslation = bbActual.GetCenter().ToVector();
            var expectedTranslation = new double[] { 1238070.0029833354, -4795867.9075041208, 4006102.3617460253 };
           
            Assert.IsTrue(actualTranslation.SequenceEqual(expectedTranslation));

            var zupBoxes = TileCutter.GetZupBoxes(conn, geometryTable, geometryColumn, idcolumn, actualTranslation);
            Assert.IsTrue(zupBoxes.Count == 22532);

            var bbox3d = BoundingBoxCalculator.GetBoundingBox(zupBoxes);
            bbox3d.Equals(new BoundingBox3D(-6813.593873352278, -4566.305836349726, -6077.698247526307, 6813.593873352278, 4585.98895232752, 6077.698247525841));
            var bbox = bbox3d.ToBoundingBox();

            var tiles = TileCutter.GetTiles(conn, 2000, geometryTable, geometryColumn, idcolumn, actualTranslation);
            conn.Close();
            Assert.IsTrue(tiles.Count == 25);
            Assert.IsTrue(tiles[0].Count == 31);
            Assert.IsTrue(tiles[0][0].BoundingBox3D.Equals(new BoundingBox3D(-4882.578986671288, 726.3967969436197, 1159.3346086232923, -4860.819284174591, 726.3967969436197, 1184.0800365605392)));
            // was before area weights removed: Assert.IsTrue(tiles[0][0].BoundingBox3D.Equals(new BoundingBox3D(-5074.687212716788, 956.3603996178134, 1482.4032165384851, -5039.462368243374, 956.3603996178134, 1512.4349554707296)));

            var boundingAllActual = TreeSerializer.GetBoundingvolume(tiles);
            var boundingAllActualNew = BoundingBoxCalculatorNew.GetBoundingAllNew(bbActual, actualTranslation);
            var boundingAllExpected = new double[] { 0.0, 9.842, -0.0, 6813.594, 0.0, 0.0, 0.0, 4576.147, 0.0, 0.0, 0.0, 6077.698 };
            Assert.IsTrue(boundingAllActual.box.SequenceEqual(boundingAllExpected));

            var boundingFirstTileActual = TreeSerializer.GetBoundingvolume(tiles[0]);
            var bbFirstTileExpected = new Boundingvolume() { box = new double[] { -4984.148, 809.148, 1281.767, 181.813, 0, 0, 0, 166.279, 0, 0, 0, 242.157 } };
            Assert.IsTrue(boundingFirstTileActual.box.SequenceEqual(boundingFirstTileActual.box));
        }
    }
}
