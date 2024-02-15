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
    public static double[] GetGeometriesBoundingBox(NpgsqlConnection conn, string geometry_table, string geometry_column, int epsg, Tile t, string query = "")
    {
        var sqlSelect = $"select st_Asbinary(st_extent(st_transform({geometry_column}, 4326))) ";
        var b = GetTileBoundingBox(t.BoundingBox);
        var sqlWhere = GetWhere(geometry_column, epsg, b.xmin, b.ymin, b.xmax, b.ymax, query);
        var sql = $"{sqlSelect} from {geometry_table} {sqlWhere}";

        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        reader.Read();
        var stream = reader.GetStream(0);
        var polygon = (Polygon)Geometry.Deserialize<WkbSerializer>(stream);
        var points = polygon.ExteriorRing.Points;
        var result = new double[] { (double)points[0].X, (double)points[0].Y, (double)points[2].X, (double)points[2].Y };

        reader.Close();
        conn.Close();

        return result;
    }

    public static List<GeometryRecord> GetGeometrySubset(NpgsqlConnection conn, string geometry_table, string geometry_column, double[] bbox, int source_epsg, string shaderColumn = "", string attributesColumns = "", string query = "", string radiusColumn = "")
    {
        var sqlselect = GetSqlSelect(geometry_column, shaderColumn, attributesColumns, radiusColumn);
        var sqlFrom = "FROM " + geometry_table;

        var b = GetTileBoundingBox(bbox);

        var sqlWhere = GetWhere(geometry_column, source_epsg, b.xmin, b.ymin, b.xmax, b.ymax, query);

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

    public static string GetWhere(string geometry_column, int source_epsg, string xmin, string ymin, string xmax, string ymax, string query = "")
    {
        return $" WHERE  ST_Intersects(" +
            $"ST_Centroid(ST_Envelope({geometry_column})), " +
            $"st_transform(ST_MakeEnvelope({xmin}, {ymin}, {xmax}, {ymax}, 4326), {source_epsg}) " +
            $") {query}";
    }

    public static string GetSqlSelect(string geometry_column, string shaderColumn, string attributesColumns, string radiusColumn)
    {
        var g = GetGeometryColumn(geometry_column);
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

    public static string GetGeometryColumn(string geometry_column)
    {
        return $"st_transform({geometry_column}, 4978)";
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
                geometryRecord.Radius = (float)reader.GetValue(radiusColumnId);
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