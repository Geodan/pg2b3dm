using System;
using System.Collections.Generic;
using System.Linq;

namespace B3dm.Tileset
{
    public class TileSet
    {
        public Root root { get; set; }
        public double geometricError { get; set; }
        public Asset asset { get; set; }
    }

    public class Child
    {
        public List<Child> children { get; set; }
        public double[] transform { get; set; }
        public double geometricError { get; set; }
        public string refine { get; set; }
        public Boundingvolume boundingVolume { get; set; }
        public Content content { get; set; }

    }

    public class Root : Child
    {
    }

    public class Boundingvolume
    {
        private double[] _box;
        public double[] box {
            get {
                return this._box;
            }
            set {
                _box = value.Select(d => Math.Round(d,3)).ToArray();
            }
        }
    }

    public class Content
    {
        public string uri { get; set; }
    }

    public class Asset
    {
        public string generator { get; set; }

        public string version { get; set; }
    }
}
