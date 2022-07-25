using System.Data;

namespace B3dm.Tileset
{
    public static class SpatialReferenceRepository
    {
        public static int GetSpatialReference(IDbConnection conn, string geometry_table, string geometry_column)
        {
            var sql = $"SELECT ST_SRID({geometry_column}) from {geometry_table} limit 1";
            var sr = DatabaseReader.ReadScalar(conn, sql);
            return sr;
        }
    }
}
