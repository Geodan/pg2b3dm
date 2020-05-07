using System;

namespace B3dm.Tileset
{
    public static class LodQuery
    {
        public static string GetLodQuery(string lodcolumn, int lod)
        {
            return lodcolumn != String.Empty ? $"and {lodcolumn}={lod}" : "";
        }
    }
}
