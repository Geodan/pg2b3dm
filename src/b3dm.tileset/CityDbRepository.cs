using System;
using Npgsql;

namespace B3dm.Tileset;

public static class CityDbRepository
{
    public static bool Is3dCityDbV5OrHigher(NpgsqlConnection conn)
    {
        const string sql = @"
SELECT COUNT(*) = 10
FROM (
    VALUES
      ('geometry_data','id'),
      ('geometry_data','geometry'),
      ('surface_data_mapping','geometry_data_id'),
      ('surface_data_mapping','surface_data_id'),
      ('surface_data_mapping','texture_mapping'),
      ('surface_data','id'),
      ('surface_data','tex_image_id'),
      ('tex_image','id'),
      ('tex_image','image_data'),
      ('tex_image','mime_type')
) req(table_name, column_name)
JOIN information_schema.columns c
  ON c.table_schema = 'citydb'
 AND c.table_name = req.table_name
 AND c.column_name = req.column_name";

        return ExecuteBooleanScalar(conn, sql);
    }

    public static bool HasTextureData(NpgsqlConnection conn)
    {
        const string sql = @"
SELECT EXISTS (
    SELECT 1
    FROM citydb.surface_data_mapping sdm
    JOIN citydb.surface_data sd ON sd.id = sdm.surface_data_id
    JOIN citydb.tex_image ti ON ti.id = sd.tex_image_id
    WHERE sdm.texture_mapping IS NOT NULL
      AND ti.image_data IS NOT NULL
    LIMIT 1
)";

        return ExecuteBooleanScalar(conn, sql);
    }

    public static bool HasColumn(NpgsqlConnection conn, string tableName, string columnName)
    {
        var schemaAndTable = GetSchemaAndTable(tableName);
        if (schemaAndTable == null) {
            return false;
        }

        const string sql = @"
SELECT EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = @table
      AND column_name = @column
)";

        return ExecuteBooleanScalar(conn, sql, cmd => {
            cmd.Parameters.AddWithValue("schema", schemaAndTable.Value.Schema);
            cmd.Parameters.AddWithValue("table", schemaAndTable.Value.Table);
            cmd.Parameters.AddWithValue("column", columnName);
        });
    }

    private static (string Schema, string Table)? GetSchemaAndTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName)) {
            return null;
        }

        var value = tableName.Trim();
        var parts = value.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) {
            return ("public", StripQuotes(parts[0]));
        }

        return (StripQuotes(parts[0]), StripQuotes(parts[1]));
    }

    private static string StripQuotes(string value)
    {
        return value.Trim().Trim('"');
    }

    private static bool ExecuteBooleanScalar(NpgsqlConnection conn, string sql, Action<NpgsqlCommand> addParameters = null)
    {
        conn.Open();
        try {
            using var cmd = new NpgsqlCommand(sql, conn);
            addParameters?.Invoke(cmd);
            var value = cmd.ExecuteScalar();
            return value is bool result && result;
        }
        finally {
            conn.Close();
        }
    }
}
