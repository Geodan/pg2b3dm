using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;
using Wkb2Gltf;
using Wkx;

namespace B3dm.Tileset;

public static class BoundingBoxRepository
{
    public static BoundingBox GetBoundingBoxForTable(IDbConnection conn, string geometry_table, string geometry_column, string query = "")
    {
        var where = GetWhere(query);
        var sqlBounds = $"SELECT st_xmin(geom1),st_ymin(geom1), st_xmax(geom1), st_ymax(geom1) FROM (select ST_Transform(ST_3DExtent({geometry_column}), 4326) as geom1 from {geometry_table} {where}) as t";
        var bbox3d = GetBounds(conn, sqlBounds);
        return bbox3d;
    }

    private static string GetWhere(string query)
    {
        return (query != string.Empty ? $" where {query}" : String.Empty);
    }

    private static BoundingBox GetBounds(IDbConnection conn, string sql)
    {
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var reader = cmd.ExecuteReader();
        reader.Read();
        var xmin = reader.GetDouble(0);
        var ymin = reader.GetDouble(1);
        var xmax = reader.GetDouble(2);
        var ymax = reader.GetDouble(3);
        var bbox = new BoundingBox(xmin, ymin, xmax, ymax);
        reader.Close();
        conn.Close();
        return bbox;
    }

    private static string GetGeometryColumn(string geometry_column, double[] translation)
    {
        return $"ST_Translate({geometry_column}, {translation[0].ToString(CultureInfo.InvariantCulture)}*-1,{translation[1].ToString(CultureInfo.InvariantCulture)}*-1 , {translation[2].ToString(CultureInfo.InvariantCulture)}*-1)";
    }

    public static List<GeometryRecord> GetGeometrySubset(NpgsqlConnection conn, string geometry_table, string geometry_column, double[] translation, Tile t, int epsg, string shaderColumn = "", string attributesColumns = "", string lodColumn = "", string query = "")
    {
        var sqlselect = GetSqlSelect(geometry_column, translation, shaderColumn, attributesColumns);
        var sqlFrom = "FROM " + geometry_table;

        var lodQuery = LodQuery.GetLodQuery(lodColumn, t.Lod);
        var xmin = t.BoundingBox.XMin.ToString(CultureInfo.InvariantCulture);
        var ymin = t.BoundingBox.YMin.ToString(CultureInfo.InvariantCulture);
        var xmax = t.BoundingBox.XMax.ToString(CultureInfo.InvariantCulture);
        var ymax = t.BoundingBox.YMax.ToString(CultureInfo.InvariantCulture);

        var sqlWhere = GetWhere(geometry_column, epsg, xmin, ymin, xmax, ymax, lodQuery);
        var queryWhere = (query != string.Empty ? $" and {query}" : String.Empty);

        var sql = sqlselect + sqlFrom + sqlWhere + queryWhere;

        var geometries = GetGeometries(conn, shaderColumn, attributesColumns, sql);
        return geometries;
    }

    private static string GetWhere(string geometry_column, int epsg, string xmin, string ymin, string xmax, string ymax, string lodQuery = "")
    {
        return $" WHERE  ST_Intersects(" +
            $"ST_Centroid(ST_Envelope({geometry_column})), " +
            $"st_transform(ST_MakeEnvelope({xmin}, {ymin}, {xmax}, {ymax}, 4326), {epsg}) " +
            $") {lodQuery}";
    }

    private static List<GeometryRecord> GetGeometries(NpgsqlConnection conn, string shaderColumn, string attributesColumns, string sql)
    {
        var geometries = new List<GeometryRecord>();
        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        var reader = cmd.ExecuteReader();
        var attributesColumnIds = new Dictionary<string, int>();
        var shadersColumnId = int.MinValue;

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

            geometries.Add(geometryRecord);
            batchId++;
        }

        reader.Close();
        conn.Close();
        return geometries;
    }

    private static string GetSqlSelect(string geometry_column,double[] translation, string shaderColumn, string attributesColumns)
    {
        var g = GetGeometryColumn(geometry_column, translation);
        var sqlselect = $"SELECT ST_AsBinary({g})";
        if (shaderColumn != String.Empty) {
            sqlselect = $"{sqlselect}, {shaderColumn} ";
        }
        if (attributesColumns != String.Empty) {
            sqlselect = $"{sqlselect}, {attributesColumns} ";
        }

        return sqlselect;
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

}




