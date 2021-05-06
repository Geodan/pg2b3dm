﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Newtonsoft.Json;
using Npgsql;
using Wkb2Gltf;
using Wkx;

namespace B3dm.Tileset
{
    public static class BoundingBoxRepository
    {
        public static bool HasFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, Point from, Point to, int epsg, string lodQuery, string query = "")
        {
            var where = (query != string.Empty ? $"and {query}" : String.Empty);

            var sql = $"select exists(select {geometry_column} from {geometry_table} where ST_Intersects(ST_Centroid(ST_Envelope({geometry_column})), ST_MakeEnvelope({from.X}, {from.Y}, {to.X}, {to.Y}, {epsg})) and ST_GeometryType({geometry_column}) =  'ST_PolyhedralSurface' {lodQuery} {where})";
            conn.Open();
            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            reader.Read();
            var exists = reader.GetBoolean(0);
            reader.Close();
            conn.Close();
            return exists;
        }

        public static BoundingBox3D GetBoundingBox3DForTable(NpgsqlConnection conn, string geometry_table, string geometry_column, string query = "")
        {
            var where = (query != string.Empty ? $"and {query}" : String.Empty);

            conn.Open();
            var sql = $"SELECT st_xmin(geom1), st_ymin(geom1), st_zmin(geom1), st_xmax(geom1), st_ymax(geom1), st_zmax(geom1) FROM (select ST_3DExtent({geometry_column}) as geom1 from {geometry_table} where ST_GeometryType({geometry_column}) =  'ST_PolyhedralSurface' {where})  as t";
            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            reader.Read();
            var xmin = reader.GetDouble(0);
            var ymin = reader.GetDouble(1);
            var zmin = reader.GetDouble(2);
            var xmax = reader.GetDouble(3);
            var ymax = reader.GetDouble(4);
            var zmax = reader.GetDouble(5);
            reader.Close();
            conn.Close();
            return new BoundingBox3D() { XMin = xmin, YMin = ymin, ZMin = zmin, XMax = xmax, YMax = ymax, ZMax = zmax };
        }

        private static string GetGeometryColumn(string geometry_column, double[] translation)
        {
            return $"ST_RotateX(ST_Translate({ geometry_column}, { translation[0].ToString(CultureInfo.InvariantCulture)}*-1,{ translation[1].ToString(CultureInfo.InvariantCulture)}*-1 , { translation[2].ToString(CultureInfo.InvariantCulture)}*-1), -pi() / 2)";
        }

        public static List<GeometryRecord> GetGeometrySubset(NpgsqlConnection conn, string geometry_table, string geometry_column, string idcolumn, double[] translation, Tile t, int epsg, string shaderColumn = "", string attributesColumn = "", string lodColumn = "", string query = "")
        {
            var geometries = new List<GeometryRecord>();
            var g = GetGeometryColumn(geometry_column, translation);
            var sqlselect = $"SELECT {idcolumn}, ST_AsBinary({g})";
            if (shaderColumn != String.Empty) {
                sqlselect = $"{sqlselect}, {shaderColumn} ";
            }
            if (attributesColumn != String.Empty) {
                sqlselect = $"{sqlselect}, {attributesColumn} ";
            }

            var sqlFrom = "FROM " + geometry_table;

            var where = (query != string.Empty ? $"and {query}" : String.Empty);

            var lodQuery = LodQuery.GetLodQuery(lodColumn, t.Lod);
            var sqlWhere = $" WHERE ST_Intersects(ST_Centroid(ST_Envelope({ geometry_column})), ST_MakeEnvelope({ t.BoundingBox.XMin}, { t.BoundingBox.YMin}, { t.BoundingBox.XMax}, { t.BoundingBox.YMax}, { epsg})) and ST_GeometryType({ geometry_column}) = 'ST_PolyhedralSurface' { lodQuery} {where}";

            var sql = sqlselect + sqlFrom + sqlWhere;

            conn.Open();
            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            var attributesColumnId = int.MinValue;
            var shadersColumnId = int.MinValue;

            if (attributesColumn != String.Empty) {
                attributesColumnId = FindField(reader, attributesColumn);
            }
            if (shaderColumn != String.Empty) {
                shadersColumnId = FindField(reader, shaderColumn);
            }
            var batchId = 0;
            while (reader.Read()) {
                var id = reader.GetString(0);
                var stream = reader.GetStream(1);

                var geom = Geometry.Deserialize<WkbSerializer>(stream);
                var geometryRecord = new GeometryRecord(batchId) { Id = id, Geometry = geom };

                if (shaderColumn != String.Empty) {
                    var json = GetJson(reader, shadersColumnId);
                    geometryRecord.Shader = JsonConvert.DeserializeObject<ShaderColors>(json);
                }
                if (attributesColumn != String.Empty) {
                    geometryRecord.Attributes = GetColumnValuesAsList(reader, attributesColumnId);
                }

                geometries.Add(geometryRecord);
                batchId++;
            }

            reader.Close();
            conn.Close();
            return geometries;
        }

        private static string GetJson(NpgsqlDataReader reader, int columnId)
        {
            var json = reader.GetString(columnId);
            return json;
        }

        private static string[] GetColumnValuesAsList(NpgsqlDataReader reader, int columnId)
        {
            return reader.GetFieldValue<string[]>(columnId);
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
            return 0;
        }
    }
}
