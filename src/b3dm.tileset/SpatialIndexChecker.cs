using Npgsql;

namespace B3dm.Tileset;
public static class SpatialIndexChecker
{
    public static bool HasSpatialIndex(NpgsqlConnection conn, string geometry_table, string geometry_column)
    {
        var schema = "public";
        if(geometry_table.Contains('.')) {
            var items = geometry_table.Split('.', 2);
            schema = items[0];
            geometry_table = items[1];
        }

        var sql = $"select count(*) from pg_indexes " +
            $"where schemaname like @schema and tablename like @geometry_table " +
            $"and indexdef like @index";
        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("schema", schema);
        cmd.Parameters.AddWithValue("geometry_table", geometry_table);
        cmd.Parameters.AddWithValue("index", "%"+geometry_column + "%");

        var reader = cmd.ExecuteReader();
        reader.Read();
        var count = reader.GetInt32(0);
        reader.Close();
        conn.Close();
        return count > 0;
    }
}
