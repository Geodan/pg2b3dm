using System.Collections.Generic;
using Npgsql;
using Wkx;

namespace B3dm.Tileset;

public static class FeatureCountRepository
{
    public static int CountFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection = false, HashSet<string> excludeHashes = null)
    {
        var select = $"COUNT({geometry_column})";
        var where = GeometryRepository.GetWhere(geometry_column, from, to, query, source_epsg, keepProjection, excludeHashes);

        var sql = $"SELECT {select} FROM {geometry_table} WHERE {where}";
        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        reader.Read();
        var count = reader.GetInt32(0);
        reader.Close();
        conn.Close();
        return count;
    }
}
