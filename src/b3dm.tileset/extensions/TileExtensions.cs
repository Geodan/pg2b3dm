using System;
using subtree;

namespace B3dm.Tileset.Extensions;
public static class TileExtensions
{
    public static Tile GetParent(this Tile t, int levels = 1)
    {
        var parent = new Tile(levels, t.X >> levels, t.Y >> levels);
        var rootCurrent = new Tile(levels + 1, parent.X * 2 * levels, parent.Y * 2 * levels);
        var Y = t.Y- rootCurrent.Y;
        var X = t.X - rootCurrent.X;
        return new Tile(levels, X, Y);
    }
}
