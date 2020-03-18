using System;
using System.Collections.Generic;
using Wkx;

namespace B3dm.Tileset
{
    public class Tile
    {
        private int id;
        private BoundingBox bb;
        private List<string> ids;
        
        public Tile(int id, BoundingBox bb, List<string> ids)
        {
            this.id = id;
            this.bb = bb;
            this.ids = ids;
        }

        public int Id {
            get { return id; }
        }

        public BoundingBox BoundingBox {
            get { return bb; }
        }

        public List<string> Ids {
            get { return ids; }
        }
    }
}
