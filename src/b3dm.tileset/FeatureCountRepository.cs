using System.Globalization;
using Npgsql;
using Wkx;

namespace B3dm.Tileset;

public static class FeatureCountRepository
{
    public static int CountFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection = false)
    {
        var hasZ = from.Z.HasValue && to.Z.HasValue;
        var fromX = from.X.Value.ToString(CultureInfo.InvariantCulture);
        var fromY = from.Y.Value.ToString(CultureInfo.InvariantCulture);
        var toX = to.X.Value.ToString(CultureInfo.InvariantCulture);
        var toY = to.Y.Value.ToString(CultureInfo.InvariantCulture);

        var select = $"COUNT({geometry_column})";
        var where = "";


        if (!hasZ) {
            where = keepProjection ?
                $"ST_Centroid(ST_Envelope({geometry_column})) && ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, {source_epsg}) {query}" :
                $"ST_Centroid(ST_Envelope({geometry_column})) && st_transform(ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, 4326), {source_epsg}) {query}";
        }
        else {
            where = $"ST_3DIntersects(ST_Centroid(ST_Envelope({geometry_column})), " +
                $"ST_3DMakeBox(" +
                $"st_transform(st_setsrid(ST_MakePoint({fromX}, {fromY}, {from.Z.Value.ToString(CultureInfo.InvariantCulture)}), 4326), {source_epsg}), " +
                $"st_transform(st_setsrid(ST_MakePoint({toX}, {toY}, {to.Z.Value.ToString(CultureInfo.InvariantCulture)}), 4326), {source_epsg})" +
                $")" +
                $") {query}";
        }

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
