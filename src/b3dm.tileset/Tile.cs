using System;
using Wkx;

namespace B3dm.Tileset
{
    public class Tile
    {
        private int id;
        private BoundingBox bb;
        private Int64 features;
        
        public Tile(int id, BoundingBox bb, Int64 features)
        {
            this.id = id;
            this.bb = bb;
            this.features = features;
        }

        public int Id {
            get { return id; }
        }

        public BoundingBox BoundingBox {
            get { return bb; }
        }

        public Int64 Features {
            get { return features; }
        }



    }
}
