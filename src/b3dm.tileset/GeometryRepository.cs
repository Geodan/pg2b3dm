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
        var sqlWhere = GetWhere(geometry_column, epsg, t.BoundingBox, query, keepProjection);
        var sql = $"{sqlSelect} from {geometry_table} {sqlWhere}";

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

    public static List<GeometryRecord> GetGeometrySubset(NpgsqlConnection conn, string geometry_table, string geometry_column, double[] bbox, int source_epsg, int target_srs, string shaderColumn = "", string attributesColumns = "", string query = "", string radiusColumn = "", bool keepProjection = false)
    {
        var sqlselect = GetSqlSelect(geometry_column, shaderColumn, attributesColumns, radiusColumn, target_srs);
        var sqlFrom = "FROM " + geometry_table;

        var sqlWhere = GetWhere(geometry_column, source_epsg, bbox, query, keepProjection);
        var sql = sqlselect + sqlFrom + sqlWhere;

        var geometries = GetGeometries(conn, shaderColumn, attributesColumns, sql, radiusColumn);
        return geometries;
    }

    private static (string xmin, string ymin, string xmax, string ymax) GetTileBoundingBox(double[] bbox)
    {
        var xmin = bbox[0].ToString(CultureInfo.InvariantCulture);
        var ymin = bbox[1].ToString(CultureInfo.InvariantCulture);
        var xmax = bbox[2].ToString(CultureInfo.InvariantCulture);
        var ymax = bbox[3].ToString(CultureInfo.InvariantCulture);
        return(xmin, ymin, xmax, ymax);
    }

    public static string GetWhere(string geometry_column, int source_epsg, double[] bbox, string query = "", bool keepProjection = false)
    {
        var hasZ = bbox.Length == 6;
        var b = GetTileBoundingBox(bbox);

        if (!hasZ) {
            var poly = keepProjection ?
                $"ST_MakeEnvelope({b.xmin}, {b.ymin}, {b.xmax}, {b.ymax}, {source_epsg}) " :
                $"st_transform(ST_MakeEnvelope({b.xmin}, {b.ymin}, {b.xmax}, {b.ymax}, 4326), {source_epsg}) ";


            return $" WHERE ST_Intersects(ST_Centroid(ST_Envelope({geometry_column})), {poly}) {query}";
        }
        else {
            return $" WHERE ST_3DIntersects(ST_Centroid(ST_Envelope({geometry_column})), " +
                $"ST_3DMakeBox(" +
                $"st_transform(st_setsrid(ST_MakePoint({b.xmin}, {b.ymin}, {bbox[4].ToString(CultureInfo.InvariantCulture)}), 4326), {source_epsg}), " +
                $"st_transform(st_setsrid(ST_MakePoint({b.xmax}, {b.ymax}, {bbox[5].ToString(CultureInfo.InvariantCulture)}), 4326), {source_epsg}))) {query}";
        }
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

        return sqlselect;
    }

    public static string GetGeometryColumn(string geometry_column, int target_srs)
    {
        return $"st_transform({geometry_column}, {target_srs})";
    }

    public static List<GeometryRecord> GetGeometries(NpgsqlConnection conn, string shaderColumn, string attributesColumns, string sql, string radiusColumn)
    {
        var geometries = new List<GeometryRecord>();
        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        var attributesColumnIds = new Dictionary<string, int>();
        var shadersColumnId = int.MinValue;
        var radiusColumnId = int.MinValue;

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
}