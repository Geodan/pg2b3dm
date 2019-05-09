namespace B3dm.Tileset
{
    public sealed class Counter
    {
        private static Counter instance = null;
        private static readonly object padlock = new object();

        Counter()
        {
        }

        public int Count { get; set; }

        public static Counter Instance {
            get {
                lock (padlock) {
                    if (instance == null) {
                        instance = new Counter();
                    }
                    return instance;
                }
            }
        }
    }
}
