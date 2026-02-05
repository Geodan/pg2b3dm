using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Wkx;

namespace B3dm.Tileset;

public static class FeatureCountRepository
{
    public static int CountFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection = false, HashSet<string> excludeHashes = null)
    {
        var select = $"COUNT({geometry_column})";
        var where = GeometryRepository.GetWhere(geometry_column, from, to, query, source_epsg, keepProjection);
        
        // Add hash exclusion filter using parameterized query
        if (excludeHashes != null && excludeHashes.Count > 0) {
            where += $" AND MD5(ST_AsBinary({geometry_column})::text) != ALL(@excludeHashes)";
        }

        var sql = $"SELECT {select} FROM {geometry_table} WHERE {where}";
        conn.Open();
        try {
            using var cmd = new NpgsqlCommand(sql, conn);
            if (excludeHashes != null && excludeHashes.Count > 0) {
                cmd.Parameters.AddWithValue("excludeHashes", excludeHashes.ToArray());
            }
            using var reader = cmd.ExecuteReader();
            reader.Read();
            var count = reader.GetInt32(0);
            return count;
        }
        finally {
            conn.Close();
        }
    }
}
