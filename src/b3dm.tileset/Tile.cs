using System.Collections.Generic;
using Wkx;

namespace B3dm.Tileset
{
    public class Tile
    {
        public int Z { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Tile(int z, int x, int y)
        {
            Z = z;
            X = x;
            Y = y;
        }
        
        public BoundingBox BoundingBox { get; set; }

        public Boundingvolume Boundingvolume { get; set; }

        public int Lod { get; set; }

        public List<Tile> GetChildren()
        {
            var t1 = new Tile(X * 2, Y * 2, Z + 1);
            var t2 = new Tile(X * 2 + 1, Y * 2, Z + 1);
            var t3 = new Tile(X * 2 + 1, Y * 2 + 1, Z + 1);
            var t4 = new Tile(X * 2, Y * 2 + 1, Z + 1);
            return new List<Tile>() { t1, t2, t3, t4 };
        }
        public bool Available { get; set; }
        public double GeometricError { get; set; }

        public List<Tile> Children { get; set; }
    }
}
