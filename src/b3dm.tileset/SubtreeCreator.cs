using System;
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

    public static Dictionary<Tile, byte[]> GenerateSubtreefiles(List<Tile> tiles)
    {
        var subtreeFiles = new Dictionary<Tile, byte[]>();
        var maxLevel = tiles.Max(s => s.Z);

        // generate child subtree files at halfway the levels
        // this formula could be adjusted for specific cases
        var subtreeLevel = (int)Math.Ceiling(((double)maxLevel+1) / 2);

        if (subtreeLevel == 1) {
            var subtreeRoot = GenerateSubtreefile(tiles);
            subtreeFiles.Add(new Tile(0, 0, 0), subtreeRoot);
            return subtreeFiles;
        }

        var mortonIndices = MortonIndex.GetMortonIndices(tiles);
        var childSubtreeAvailabilty = Availability.GetLevelAvailability(mortonIndices.tileAvailability, subtreeLevel);

        var offset = LevelOffset.GetLevelOffset(subtreeLevel);
        var tileAvailability = mortonIndices.tileAvailability.Substring(0, offset);
        var contentAvailability = mortonIndices.contentAvailability.Substring(0, offset);

        var availabilityLength = tileAvailability.Length;

        // write the root subtree file
        var subtreeRootbytes = SubtreeWriter.ToBytes(tileAvailability, contentAvailability, childSubtreeAvailabilty);
        subtreeFiles.Add(new Tile(0, 0, 0), subtreeRootbytes);

        // now create the subtree files
        var ba = BitArray2DCreator.GetBitArray2D(childSubtreeAvailabilty);
        for (var x = 0; x < ba.GetWidth(); x++) {
            for (var y = 0; y < ba.GetHeight(); y++) {
                if (ba.Get(x, y)) {
                    var t = new Tile(subtreeLevel, x, y);
                    var subtreeTiles = GetSubtreeTiles(tiles,t);
                    var mortonIndicesSubtree = MortonIndex.GetMortonIndices(subtreeTiles);
                    var subtreebytes = SubtreeWriter.ToBytes(Fill(mortonIndicesSubtree.tileAvailability, availabilityLength), Fill(mortonIndicesSubtree.contentAvailability, availabilityLength));
                    subtreeFiles.Add(t,subtreebytes);
                }
            }
        }

        return subtreeFiles;
    }


    public static string Fill(string availability, int targetLength)
    {
        var l = availability.Length;
        var res = availability + new string('0', targetLength - l);
        return res;
    }

    public static List<Tile> GetSubtreeTiles(List<Tile> tiles, Tile tile )
    {
        var res = new List<Tile>();

        foreach (var t in tiles) {
            if (tile.HasChild(t)) {
                var levels = t.Z - tile.Z;
                var subtreeTile = t.GetParent(levels);
                subtreeTile.Available = t.Available;
                res.Add(subtreeTile);
            }
            else if (t.Z == tile.Z && t.X==tile.X && t.Y==tile.Y) {
                var rootTile = new Tile(0, 0, 0);
                rootTile.Available = t.Available;
                res.Add(rootTile);
            }
        }
        return res;
    }
}