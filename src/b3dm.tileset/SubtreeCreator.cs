using System.Collections.Generic;
using System.Linq;
using B3dm.Tileset.Extensions;
using subtree;

namespace B3dm.Tileset;

public static class SubtreeCreator
{
    public static byte[] GenerateSubtreefile(List<Tile> tiles)
    {
        var mortonIndices = MortonIndex.GetMortonIndices(tiles);
        var subtreebytes = SubtreeWriter.ToBytes(mortonIndices.tileAvailability, mortonIndices.contentAvailability);
        return subtreebytes;
    }

    public static Dictionary<Tile, byte[]> GenerateSubtreefileRoot(List<Tile> tiles)
    {
        var subtreeFiles = new Dictionary<Tile, byte[]>();
        var firstAvailable = tiles.FirstOrDefault(s => s.Available);
        var maxLevel = tiles.Max(s => s.Z);

        var subtreeLevel = firstAvailable.Z - 1;

        var mortonIndices = MortonIndex.GetMortonIndices(tiles);
        var offset1 = LevelOffset.GetLevelOffset(firstAvailable.Z);
        var tileAvailability = mortonIndices.tileAvailability.Substring(0, offset1);
        var childSubtreeAvailabilty = Availability.GetLevelAvailability(tileAvailability,subtreeLevel);
        var subtreeRootbytes = SubtreeWriter.ToBytes(tileAvailability, subtreeAvailability: childSubtreeAvailabilty);
        subtreeFiles.Add(new Tile(0, 0, 0), subtreeRootbytes);

        // now create the subtree files
        var ba = BitArray2DCreator.GetBitArray2D(childSubtreeAvailabilty);
        // list of subtree files to create
        for (var x = 0; x < ba.GetWidth(); x++) {
            for (var y = 0; y < ba.GetHeight(); y++) {
                if (ba.Get(x, y)) {
                    var t = new Tile(subtreeLevel, x, y);
                    var subtreeTiles = GetSubtreeTiles(tiles,t);
                    var rootTile = new Tile(0, 0, 0);
                    rootTile.Available= true;
                    subtreeTiles.Add(rootTile);
                    var mortonIndicesSubtree = MortonIndex.GetMortonIndices(subtreeTiles);
                    var subtreebytes = SubtreeWriter.ToBytes(mortonIndicesSubtree.tileAvailability, mortonIndicesSubtree.contentAvailability);
                    subtreeFiles.Add(t,subtreebytes);
                }
            }
        }

        return subtreeFiles;
    }

    public static List<Tile> GetSubtreeTiles(List<Tile> tiles, Tile tile )
    {
        var res = new List<Tile>();

        foreach (var t in tiles) {
            if (tile.HasChild(t)&&t.Z==2) {
                var levels = t.Z - tile.Z;
                var subtreeTile = t.GetParent(levels);
                subtreeTile.Available = t.Available;
                res.Add(subtreeTile);
            }
        }
        return res;
    }
}