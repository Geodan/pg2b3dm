using System.Data;

namespace B3dm.Tileset;

public static class SpatialReferenceRepository
{
    public static int GetSpatialReference(IDbConnection conn, string geometry_table, string geometry_column, string query = "")
    {
        var q = query == "" ? "" : $"WHERE {query}";
        var sql = $"SELECT ST_SRID({geometry_column}) from {geometry_table} {q} limit 1";
        var sr = DatabaseReader.ReadScalar(conn, sql);
        return sr;
    }
}
