using Npgsql;

namespace B3dm.Tileset;
public static class SpatialIndexChecker
{
    public static bool HasSpatialIndex(NpgsqlConnection conn, string geometry_table, string geometry_column)
    {
        var sql = $"select count(*) from pg_indexes " +
            $"where tablename like '{geometry_table}' " +
            $"and indexdef like '%{geometry_column}%'";

        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        reader.Read();
        var count = reader.GetInt32(0);
        reader.Close();
        conn.Close();
        return count > 0;
    }
}
