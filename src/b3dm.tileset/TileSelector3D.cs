using System.Collections.Generic;
using System.Linq;
using subtree;

namespace B3dm.Tileset;
public static class TileSelector3D
{
    public static List<Tile3D> Select(List<Tile3D> tiles, Tile3D root, int fromLevel, int toLevel)
    {
        if (tiles.Count == 1 && tiles.First().Level == root.Level) {
            return tiles;
        }
        var result = new List<Tile3D>();
        for (var level = fromLevel; level <= toLevel; level++) {
            var selected = tiles.FindAll(t => t.Level == level && t.Available && root.HasChild(t));
            result.AddRange(selected);
        }
        return result;
    }
}
