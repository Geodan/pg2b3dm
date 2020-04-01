using System.Collections.Generic;

namespace B3dm.Tileset
{
    public static class RecursiveTileCounter
    {
        public static int CountTiles(List<Tile> tiles, int startValue)
        {
            foreach (var tile in tiles) {
                startValue++;
                if (tile.Child != null) {
                    startValue = CountTiles(new List<Tile> { tile.Child }, startValue);
                }
            }
            return startValue;
        }

    }
}
