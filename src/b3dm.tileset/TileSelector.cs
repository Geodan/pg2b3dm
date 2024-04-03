using System.Collections.Generic;
using System.Linq;
using subtree;

namespace B3dm.Tileset;
public static class TileSelector
{
    public static List<Tile> Select(List<Tile> tiles, Tile root, int fromLevel, int toLevel)
    {
        if (tiles.Count == 1 && tiles.First().Z == root.Z) {
            return tiles;
        }
        var result = new List<Tile>();
        for (var z = fromLevel; z <= toLevel; z++) {
            var selected = tiles.FindAll(t => t.Z == z && t.Available && root.HasChild(t));
            result.AddRange(selected);
        }
        return result;
    }
}
