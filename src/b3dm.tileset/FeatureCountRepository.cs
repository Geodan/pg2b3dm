using System.Globalization;
using Npgsql;
using Wkx;

namespace B3dm.Tileset;

public static class FeatureCountRepository
{
    public static int CountFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg)
    {
        var fromX = from.X.Value.ToString(CultureInfo.InvariantCulture);
        var fromY = from.Y.Value.ToString(CultureInfo.InvariantCulture);
        var toX = to.X.Value.ToString(CultureInfo.InvariantCulture);
        var toY = to.Y.Value.ToString(CultureInfo.InvariantCulture);

        var sql = $"SELECT COUNT({geometry_column}) FROM {geometry_table} WHERE ST_Centroid(ST_Envelope({geometry_column})) && st_transform(ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, 4326), {source_epsg}) {query}";
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
