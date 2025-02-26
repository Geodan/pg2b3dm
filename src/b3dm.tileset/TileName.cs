using System;
using subtree;

namespace B3dm.Tileset
{
    public static class TileName
    {
        public static string GetFileName(Tile tile, bool createGltf, string lodColumn1 = "", int? lod = null)
        {
            var file = $"{tile.Z}_{tile.X}_{tile.Y}";
            if (lodColumn1 != String.Empty) {
                file += $"_{lod}";
            }

            var ext = createGltf ? ".glb" : ".b3dm";
            file += ext;
            return file;
        }

    }
}
