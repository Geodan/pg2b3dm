using System.Collections.Generic;
using Npgsql;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class GetTilesTester
    {
        [Test]
        public void FirstTest()
        {
            var conn = new NpgsqlConnection("Host=localhost;Username=postgres;Password=postgres;Database=postgres;Port=5432");
            conn.Open();
            var extent = 2000;
            var box3d = new BoundingBox3D(-8414816.874355044, 4734484.544997846, 0, -8398673.141351309, 4754743.970459332, 76.58);
            var tiles = TileCutter.GetTiles(conn, extent, "delaware_buildings", " geom_triangle", box3d, 3857, new List<int> { 0, 1 }, new double[] { 250, 0 }, "lod");
            conn.Close();
            Assert.IsTrue(tiles.Count == 53);
            Assert.IsTrue(tiles[0].Id == 1);
            Assert.IsTrue(tiles[0].Children.Count == 1);
            Assert.IsTrue(tiles[0].Children[0].Id == 2);
        }
    }
}
