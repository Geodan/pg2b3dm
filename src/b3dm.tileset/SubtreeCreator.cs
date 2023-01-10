using System.Collections.Generic;
using subtree;

namespace B3dm.Tileset;

public static class SubtreeCreator
{
    public static byte[] GenerateSubtreefile(List<Tile> tiles)
    {
        var subtreeTiles = new List<subtree.Tile>();
        foreach (var t in tiles) {
            subtreeTiles.Add(new subtree.Tile(t.Z, t.X, t.Y, t.Available));
        }

        var mortonIndices = MortonIndex.GetMortonIndices(subtreeTiles);
        var subtreebytes = SubtreeWriter.ToBytes(mortonIndices.tileAvailability, mortonIndices.contentAvailability);
        return subtreebytes;
    }
}
