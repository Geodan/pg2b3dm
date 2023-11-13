using System.Data;
using Wkx;

namespace B3dm.Tileset;

public static class BoundingBoxRepository
{
    public static BoundingBox GetBoundingBoxForTable(IDbConnection conn, string geometry_table, string geometry_column)
    {
        var sqlBounds = $"SELECT st_xmin(geom1),st_ymin(geom1), st_xmax(geom1), st_ymax(geom1) FROM (select ST_Transform(ST_3DExtent({geometry_column}), 4326) as geom1 from {geometry_table}) as t";
        var bbox3d = GetBounds(conn, sqlBounds);
        return bbox3d;
    }

    private static BoundingBox GetBounds(IDbConnection conn, string sql)
    {
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var reader = cmd.ExecuteReader();
        reader.Read();
        // increase the boundingbox a little to avoid missing geometries at the edges
        var delta = 0.001;
        var xmin = reader.GetDouble(0)-delta;
        var ymin = reader.GetDouble(1)-delta;
        var xmax = reader.GetDouble(2)+delta;
        var ymax = reader.GetDouble(3)+delta;
        var bbox = new BoundingBox(xmin, ymin, xmax, ymax);
        reader.Close();
        conn.Close();
        return bbox;
    }
}




