namespace pg2b3dm
{
    public class TileSet
    {
        public Root root { get; set; }
        public int geometricError { get; set; }
        public Asset asset { get; set; }
    }

    public class Root
    {
        public Child[] children { get; set; }
        public double[] transform { get; set; }
        public int geometricError { get; set; }
        public string refine { get; set; }
        public Boundingvolume boundingVolume { get; set; }
    }

    public class Boundingvolume
    {
        public double[] box { get; set; }
    }

    public class Child
    {
        public Child[] children { get; set; }
        public float geometricError { get; set; }
        public string refine { get; set; }
        public Boundingvolume boundingVolume { get; set; }
        public Content content { get; set; }
    }

    public class Content
    {
        public string uri { get; set; }
    }

    public class Asset
    {
        public string version { get; set; }
    }
}
