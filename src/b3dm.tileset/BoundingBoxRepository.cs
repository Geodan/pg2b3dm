﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;
using Wkb2Gltf;
using Wkx;

namespace B3dm.Tileset
{
    public static class BoundingBoxRepository
    {
        public static int CountFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, int epsg, string query)
        {
            var fromX = from.X.Value.ToString(CultureInfo.InvariantCulture);
            var fromY = from.Y.Value.ToString(CultureInfo.InvariantCulture);
            var toX = to.X.Value.ToString(CultureInfo.InvariantCulture);
            var toY = to.Y.Value.ToString(CultureInfo.InvariantCulture);

            var sql = $"select count({geometry_column}) from {geometry_table} where ST_Intersects(ST_Centroid(ST_Envelope({geometry_column})), ST_Transform(ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, 3857), {epsg})) {query}";
            conn.Open();
            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            reader.Read();
            var count = reader.GetInt32(0);
            reader.Close();
            conn.Close();
            return count;
        }

        public static BoundingBox3D GetBoundingBox3DForTable(IDbConnection conn, string geometry_table, string geometry_column, string query = "")
        {
            var where = GetWhere(query);
            var sqlBounds = $"SELECT st_xmin(geom1),st_ymin(geom1), st_zmin(geom1), st_xmax(geom1), st_ymax(geom1), st_zmax(geom1) FROM (select ST_3DExtent({geometry_column}) as geom1 from {geometry_table} {where}) as t";
            var bbox3d = GetBounds(conn, sqlBounds);
            return bbox3d;
        }

        private static string GetWhere(string query)
        {
            return (query != string.Empty ? $" where {query}" : String.Empty);
        }

        private static BoundingBox3D GetBounds(IDbConnection conn, string sql)
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();
            reader.Read();
            var xmin = reader.GetDouble(0);
            var ymin = reader.GetDouble(1);
            var zmin = reader.GetDouble(2);
            var xmax = reader.GetDouble(3);
            var ymax = reader.GetDouble(4);
            var zmax = reader.GetDouble(5);
            var bbox3d = new BoundingBox3D() { XMin = xmin, YMin = ymin, ZMin = zmin, ZMax = zmax, XMax = xmax, YMax = ymax };
            reader.Close();
            conn.Close();
            return bbox3d;
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
                $"st_transform(ST_MakeEnvelope({xmin}, {ymin}, {xmax}, {ymax}, 3857), {epsg}) " +
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
                var fieldId = FindField(reader, shaderColumn);
                if (fieldId != -1) {
                    shadersColumnId = FindField(reader, shaderColumn);
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

        private static int FindField(NpgsqlDataReader reader, string fieldName)
        {
            var schema = reader.GetSchemaTable();
            var rows = schema.Columns["ColumnName"].Table.Rows;
            foreach (var row in rows) {
                var columnName = (string)((DataRow)row).ItemArray[0];
                if (columnName == fieldName) {
                    return (int)((DataRow)row).ItemArray[1];
                }
            }
            return -1;
        }

        private static Dictionary<string, int> FindFields(NpgsqlDataReader reader, List<string> fieldNames)
        {
            var res = new Dictionary<string, int>();
            foreach (var field in fieldNames) {
                var fieldId = FindField(reader, field.Trim());
                if (fieldId != -1) {
                    res.Add(field, fieldId);
                }
            }
            return res;
        }

    }
}




