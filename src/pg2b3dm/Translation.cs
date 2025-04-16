using System;
using Npgsql;
using NpgsqlTypes;
using Wkb2Gltf;
using Wkx;

namespace pg2b3dm;
public static class Translation
{
    public static double[] GetTranslation(Point center_wgs84)
    {
        double[] translation;
        var v3 = SpatialConverter.GeodeticToEcef((double)center_wgs84.X, (double)center_wgs84.Y, 0);
        translation = [v3.X, v3.Y, v3.Z];

        return translation;
    }

    public static double[] GetTranslation(NpgsqlConnection conn, Point center_wgs84, int source_epsg)
    {
        string sql = @"
            SELECT ST_X(transformed_geom), ST_Y(transformed_geom)
            FROM (
                SELECT ST_Transform(ST_SetSRID(ST_MakePoint(@lon, @lat), 4326), @epsg) AS transformed_geom
            ) AS subquery;
        ";

        var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("lon", NpgsqlDbType.Double, center_wgs84.X);
        cmd.Parameters.AddWithValue("lat", NpgsqlDbType.Double, center_wgs84.Y);
        cmd.Parameters.AddWithValue("epsg", NpgsqlDbType.Integer, source_epsg);
        conn.Open();
        var reader = cmd.ExecuteReader();
        reader.Read();
        var x = reader.GetDouble(0);
        var y = reader.GetDouble(1);
        conn.Close();
        return [x, y, 0];
    }
}
