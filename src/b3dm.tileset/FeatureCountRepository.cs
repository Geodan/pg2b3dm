using System.Globalization;
using Npgsql;
using Wkx;

namespace B3dm.Tileset;

public static class FeatureCountRepository
{
    public static int CountFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, int epsg, string query)
    {
        var fromX = from.X.Value.ToString(CultureInfo.InvariantCulture);
        var fromY = from.Y.Value.ToString(CultureInfo.InvariantCulture);
        var toX = to.X.Value.ToString(CultureInfo.InvariantCulture);
        var toY = to.Y.Value.ToString(CultureInfo.InvariantCulture);

        var sql = $"select count({geometry_column}) from {geometry_table} where ST_Intersects(ST_Centroid(ST_Envelope({geometry_column})), ST_Transform(ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, 4326), {epsg})) {query}";
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
