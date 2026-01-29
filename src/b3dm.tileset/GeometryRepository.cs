using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;
using subtree;
using Wkb2Gltf;
using Wkx;

namespace B3dm.Tileset;

public static class GeometryRepository
{
    /// <summary>
    /// Returns double array with 6 bounding box coordinates, xmin, ymin, xmax, ymax, zmin, zmax
    /// </summary>
    public static double[] GetGeometriesBoundingBox(NpgsqlConnection conn, string geometry_table, string geometry_column, int epsg, Tile t, string query = "", bool keepProjection = false)
    {
        var sqlSelect = keepProjection?
            $"select st_Asbinary(st_3dextent({geometry_column})) ":
            $"select st_Asbinary(st_3dextent(st_transform({geometry_column}, 4979))) ";
        var sqlWhere = GetWhere(geometry_column, new Point(t.BoundingBox[0], t.BoundingBox[1]), new Point(t.BoundingBox[2], t.BoundingBox[3]), query, epsg, keepProjection);
        var sql = $"{sqlSelect} from {geometry_table} where {sqlWhere}";

        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        reader.Read();
        var stream = reader.GetStream(0);
        var geometry = Geometry.Deserialize<WkbSerializer>(stream);
        var result = BBox3D.GetBoundingBoxPoints(geometry);

        reader.Close();
        conn.Close();

        return result;
    }

    public static List<GeometryRecord> GetGeometrySubset(NpgsqlConnection conn, string geometry_table, string geometry_column, double[] bbox, int source_epsg, int target_srs, string shaderColumn = "", string attributesColumns = "", string query = "", string radiusColumn = "", bool keepProjection = false, HashSet<string> excludeHashes = null, int? maxFeatures = null)
    {
        var sqlselect = GetSqlSelect(geometry_column, shaderColumn, attributesColumns, radiusColumn, target_srs);
        var sqlFrom = "FROM " + geometry_table;

        // todo: fix unit test when there is no z
        var points = GetPoints(bbox);

        var sqlWhere = GetWhere(geometry_column, points.fromPoint, points.toPoint, query, source_epsg, keepProjection, excludeHashes);
        var sqlOrderBy = GetOrderBy(geometry_column);
        var sqlLimit = maxFeatures.HasValue ? $" LIMIT {maxFeatures.Value}" : "";
        var sql = sqlselect + sqlFrom + " where " + sqlWhere + sqlOrderBy + sqlLimit;

        var geometries = GetGeometries(conn, shaderColumn, attributesColumns, sql, radiusColumn, geometry_column);
        return geometries;
    }

    public static string GetWhere(string geometry_column, Point from, Point to, string query, int source_epsg, bool keepProjection, HashSet<string> excludeHashes = null)
    {
        var fromX = from.X.Value.ToString(CultureInfo.InvariantCulture);
        var fromY = from.Y.Value.ToString(CultureInfo.InvariantCulture);
        var toX = to.X.Value.ToString(CultureInfo.InvariantCulture);
        var toY = to.Y.Value.ToString(CultureInfo.InvariantCulture);

        var hasZ = from.Z.HasValue && to.Z.HasValue;
        var where = "";

        if (!hasZ) {
            where = keepProjection ?
                $"ST_Centroid(ST_Envelope({geometry_column})) && ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, {source_epsg}) {query}" :
                $"ST_Centroid(ST_Envelope({geometry_column})) && st_transform(ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, 4326), {source_epsg}) {query}";
        }
        else {
            var fromBox = keepProjection ?
                $"st_setsrid(ST_MakePoint({fromX}, {fromY}, {from.Z.Value.ToString(CultureInfo.InvariantCulture)}), {source_epsg})" :
                 $"st_setsrid(ST_MakePoint({fromX}, {fromY}, {from.Z.Value.ToString(CultureInfo.InvariantCulture)}), 4979)";
            var toBox = keepProjection ?
                $"st_setsrid(ST_MakePoint({toX}, {toY}, {to.Z.Value.ToString(CultureInfo.InvariantCulture)}), {source_epsg})" :
                $"st_setsrid(ST_MakePoint({toX}, {toY}, {to.Z.Value.ToString(CultureInfo.InvariantCulture)}), 4979)";

            var geom = $"st_setsrid(st_makepoint((st_xmin({geometry_column}) + st_xmax({geometry_column}))/2,(st_ymin({geometry_column}) + st_ymax({geometry_column}))/2, (st_zmin({geometry_column}) + st_zmax({geometry_column}))/2), {source_epsg})";
            where = keepProjection ?
                $"ST_3DIntersects({geom}, ST_3DMakeBox({fromBox}, {toBox})) {query}" :
                $"ST_3DIntersects({geom}, st_transform(ST_3DMakeBox({fromBox}, {toBox}), {source_epsg})) {query}";
        }

        // Add hash exclusion filter
        if (excludeHashes != null && excludeHashes.Count > 0) {
            var hashList = string.Join(",", excludeHashes.Select(h => $"'{h}'"));
            where += $" AND MD5(ST_AsBinary({geometry_column})::text) NOT IN ({hashList})";
        }

        return where;
    }

    public static string GetSqlSelect(string geometry_column, string shaderColumn, string attributesColumns, string radiusColumn, int target_srs)
    {
        var g = GetGeometryColumn(geometry_column, target_srs);
        var sqlselect = $"SELECT ST_AsBinary({g})";
        if (shaderColumn != String.Empty) {
            sqlselect = $"{sqlselect}, {shaderColumn} ";
        }
        if (attributesColumns != String.Empty) {
            sqlselect = $"{sqlselect}, {attributesColumns} ";
        }
        if (radiusColumn != String.Empty) {
            sqlselect = $"{sqlselect}, {radiusColumn} ";
        }
        // Add MD5 hash of geometry
        sqlselect = $"{sqlselect}, MD5(ST_AsBinary({geometry_column})::text) as geom_hash ";

        return sqlselect;
    }

    public static string GetGeometryColumn(string geometry_column, int target_srs)
    {
        return $"st_transform({geometry_column}, {target_srs})";
    }

    public static string GetOrderBy(string geometry_column)
    {
        return $" ORDER BY ST_Area(ST_Envelope({geometry_column})) DESC";
    }

    public static List<GeometryRecord> GetGeometries(NpgsqlConnection conn, string shaderColumn, string attributesColumns, string sql, string radiusColumn, string geometry_column = "")
    {
        var geometries = new List<GeometryRecord>();
        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        var attributesColumnIds = new Dictionary<string, int>();
        var shadersColumnId = int.MinValue;
        var radiusColumnId = int.MinValue;
        var hashColumnId = int.MinValue;

        if (attributesColumns != String.Empty) {
            var attributesColumnsList = attributesColumns.Split(',').ToList();
            attributesColumnIds = FindFields(reader, attributesColumnsList);
        }
        if (shaderColumn != String.Empty) {
            var fld = FindField(reader, shaderColumn);
            if (fld.HasValue) {
                shadersColumnId = FindField(reader, shaderColumn).Value;
            }
        }
        if (radiusColumn != String.Empty) {
            var fld = FindField(reader, radiusColumn);
            if (fld.HasValue) {
                radiusColumnId = FindField(reader, radiusColumn).Value;
            }
        }
        // Find hash column
        var hashFld = FindField(reader, "geom_hash");
        if (hashFld.HasValue) {
            hashColumnId = hashFld.Value;
        }

        var batchId = 0;
        while (reader.Read()) {
            var stream = reader.GetStream(0);

            var geom = Geometry.Deserialize<WkbSerializer>(stream);
            var geometryRecord = new GeometryRecord(batchId) { Geometry = geom };

            if (shaderColumn != string.Empty) {
                var json = GetJson(reader, shadersColumnId);
                geometryRecord.Shader = JsonConvert.DeserializeObject<ShaderColors>(json);
            }
            if (attributesColumns != string.Empty) {
                var attributes = GetColumnValuesAsList(reader, attributesColumnIds);
                geometryRecord.Attributes = attributes;
            }
            if (radiusColumn != string.Empty) {

                var radius = reader.GetFieldValue<object>(radiusColumnId);
                geometryRecord.Radius = Convert.ToSingle(radius);
            }
            if (hashColumnId != int.MinValue) {
                geometryRecord.Hash = reader.GetString(hashColumnId);
            }

            geometries.Add(geometryRecord);
            batchId++;
        }

        reader.Close();
        conn.Close();
        return geometries;
    }

    private static int? FindField(NpgsqlDataReader reader, string fieldName)
    {
        var schema = reader.GetSchemaTable();
        var rows = schema.Columns["ColumnName"].Table.Rows;
        foreach (var row in rows) {
            var columnName = (string)((DataRow)row).ItemArray[0];
            if (columnName == fieldName) {
                return (int)((DataRow)row).ItemArray[1];
            }
        }
        return null;
    }

    private static Dictionary<string, int> FindFields(NpgsqlDataReader reader, List<string> fieldNames)
    {
        var res = new Dictionary<string, int>();
        foreach (var field in fieldNames) {
            var fieldId = FindField(reader, field.Trim());
            if (fieldId.HasValue) {
                res.Add(field, fieldId.Value);
            }
        }
        return res;
    }

    private static string GetJson(NpgsqlDataReader reader, int columnId)
    {
        var json = reader.GetString(columnId);
        return json;
    }

    private static Dictionary<string, object> GetColumnValuesAsList(NpgsqlDataReader reader, Dictionary<string, int> columnIds)
    {
        var attributes = new Dictionary<string, object>();
        foreach (var colId in columnIds) {
            var attr = reader.GetFieldValue<object>(colId.Value);
            attributes.Add(colId.Key, attr);
        }
        return attributes;
    }

    private static (Point fromPoint, Point toPoint) GetPoints(double[] bbox)
    {
        Point fromPoint;
        Point toPoint;

        if (bbox.Length == 4) {
            fromPoint = new Point(bbox[0], bbox[1]);
            toPoint = new Point(bbox[2], bbox[3]);
        }
        else {
            fromPoint = new Point(bbox[0], bbox[1], bbox[4]);
            toPoint = new Point(bbox[2], bbox[3], bbox[5]);
        }
        return (fromPoint, toPoint);
    }

}