using System.Data;
using Wkx;

namespace B3dm.Tileset;

public static class BoundingBoxRepository
{
    public static (BoundingBox bbox, double zmin, double zmax) GetBoundingBoxForTable(IDbConnection conn, string geometry_table, string geometry_column, bool keepProjection = false, string query = "")
    {
        var select = $"SELECT st_xmin(geom1),st_ymin(geom1), st_xmax(geom1), st_ymax(geom1), st_zmin(geom1), st_zmax(geom1) ";
        var geom = keepProjection?
            $"(select ST_3DExtent({geometry_column})":
            $"(select st_transform(ST_3DExtent({ geometry_column}), 4979)";
        var sqlBounds = $"{select} FROM {geom} as geom1 from {geometry_table} {query}) as t";
        var bbox3d = GetBounds(conn, sqlBounds);
        return bbox3d;
    }

    /// <summary>
    /// As GetBoundingBoxForTable, but holding every envelope centroid: for projected input the transformed extent can miss some.
    /// </summary>
    public static (BoundingBox bbox, double zmin, double zmax) GetBoundingBoxForQuadtree(IDbConnection conn, string geometry_table, string geometry_column, int source_epsg, bool keepProjection = false, string query = "")
    {
        if (keepProjection || source_epsg == 4326) {
            return GetBoundingBoxForTable(conn, geometry_table, geometry_column, keepProjection, query);
        }
        var select = "SELECT least(st_xmin(e), x0), least(st_ymin(e), y0), greatest(st_xmax(e), x1), greatest(st_ymax(e), y1), st_zmin(e), st_zmax(e) ";
        var rows = $"select {geometry_column}, st_transform(ST_Centroid(ST_Envelope({geometry_column})), 4326) as p from {geometry_table} {query}";
        var sqlBounds = $"{select} FROM (select st_transform(ST_3DExtent({geometry_column}), 4979) as e, " +
            $"min(ST_X(p)) as x0, min(ST_Y(p)) as y0, max(ST_X(p)) as x1, max(ST_Y(p)) as y1 from ({rows}) as r) as t";
        return GetBounds(conn, sqlBounds);
    }

    private static (BoundingBox, double, double) GetBounds(IDbConnection conn, string sql)
    {
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var reader = cmd.ExecuteReader();
        reader.Read();
        // increase the boundingbox a little to avoid missing geometries at the edges
        var delta = 0.000001;
        var xmin = reader.GetDouble(0)-delta;
        var ymin = reader.GetDouble(1)-delta;
        var xmax = reader.GetDouble(2)+delta;
        var ymax = reader.GetDouble(3)+delta;
        var zmin = reader.GetDouble(4);
        var zmax = reader.GetDouble(5);
        var bbox = new BoundingBox(xmin, ymin, xmax, ymax);
        reader.Close();
        conn.Close();
        return (bbox, zmin, zmax);
    }
}




