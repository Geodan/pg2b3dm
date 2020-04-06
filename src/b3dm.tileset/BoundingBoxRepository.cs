using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Npgsql;
using Wkx;

namespace B3dm.Tileset
{
    public static class BoundingBoxRepository
    {
        public static List<string> GetFeaturesInBox(NpgsqlConnection conn, string geometry_table, string geometry_column, string idcolumn, Point from, Point to, int epsg, string lodQuery)
        {
            var res = new List<string>();
            var sql = $"select {idcolumn} from {geometry_table} where ST_Intersects(ST_Centroid(ST_Envelope({geometry_column})), ST_MakeEnvelope({from.X}, {from.Y}, {to.X}, {to.Y}, {epsg})) and ST_GeometryType({geometry_column}) =  'ST_PolyhedralSurface' {lodQuery}";
            var cmd = new NpgsqlCommand(sql, conn);
            var reader= cmd.ExecuteReader();
            while (reader.Read()) {
                var id = reader.GetString(0);
                res.Add(id);
            }

            reader.Close();
            return res;
        }

        public static BoundingBox3D GetBoundingBox3DForTable(NpgsqlConnection conn, string geometry_table, string geometry_column)
        {
            var sql = $"SELECT st_xmin(geom1), st_ymin(geom1), st_zmin(geom1), st_xmax(geom1), st_ymax(geom1), st_zmax(geom1) FROM (select ST_3DExtent({geometry_column}) as geom1 from {geometry_table} where ST_GeometryType({geometry_column}) =  'ST_PolyhedralSurface') as t";
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
            return new BoundingBox3D() { XMin = xmin, YMin = ymin, ZMin = zmin, XMax = xmax, YMax = ymax, ZMax = zmax };
        }

        private  static string GetGeometryColumn(string geometry_column, double[] translation)
        {
            return $"ST_RotateX(ST_Translate({ geometry_column}, { translation[0].ToString(CultureInfo.InvariantCulture)}*-1,{ translation[1].ToString(CultureInfo.InvariantCulture)}*-1 , { translation[2].ToString(CultureInfo.InvariantCulture)}*-1), -pi() / 2)";
        }
        
        public static BoundingBox3D Get3DExtent(NpgsqlConnection conn, string GeometryTable, string geometryColumn, string idcolumn, string[] ids)
        {
            var res = new List<string>();
            foreach (var id in ids) {
                res.Add("'" + id + "'");
            }

            var ids1 = string.Join(",", res);

            var sql = $"select st_xmin(t.geom), st_ymin(t.geom), st_zmin(t.geom), st_xmax(t.geom), st_ymax(t.geom), st_zmax(t.geom) from (select st_3dextent({geometryColumn}) as geom from {GeometryTable} where {idcolumn} in ({ids1})) as t";
            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            reader.Read();
            var xmin = reader.GetDouble(0);
            var ymin = reader.GetDouble(1);
            var zmin = reader.GetDouble(2);
            var xmax = reader.GetDouble(3);
            var ymax = reader.GetDouble(4);
            var zmax = reader.GetDouble(5);
            var bbox3d = new BoundingBox3D(xmin, ymin, zmin, xmax, ymax, zmax);
            reader.Close();
            return bbox3d;
        }

        public static List<GeometryRecord> GetGeometrySubset(NpgsqlConnection conn, string geometry_table, string geometry_column, string idcolumn, double[] translation, string[] ids, string colorColumn = "", string attributesColumn = "")
        {
            var geometries = new List<GeometryRecord>();
            var g = GetGeometryColumn(geometry_column, translation);
            var sqlselect = $"SELECT {idcolumn}, ST_AsBinary({g})";
            if (colorColumn != String.Empty) {
                sqlselect = $"{sqlselect}, {colorColumn} ";
            }
            if (attributesColumn != String.Empty) {
                sqlselect = $"{sqlselect}, {attributesColumn} ";
            }

            var sqlFrom = $"FROM {geometry_table} ";

            var res = new List<string>();
            foreach(var id in ids) {
                res.Add("'" + id + "'");
            }

            var ids1 = string.Join(",", res);
            var sqlWhere = $"WHERE {idcolumn} in ({ids1})";

            var sql = sqlselect + sqlFrom + sqlWhere;

            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            var attributesColumnId=int.MinValue;
            if (attributesColumn != String.Empty) {
                attributesColumnId = FindField(reader, attributesColumn);
            }
            var batchId = 0;
            while (reader.Read()) {
                var id = reader.GetString(0);
                var stream = reader.GetStream(1);

                var geom = Geometry.Deserialize<WkbSerializer>(stream);
                var geometryRecord = new GeometryRecord (batchId) { Id = id, Geometry = geom };

                if (colorColumn != String.Empty) {
                    geometryRecord.HexColors = GetColumnValuesAsList(reader, 2);
                }
                if (attributesColumn != String.Empty) {
                    geometryRecord.Attributes= GetColumnValuesAsList(reader, attributesColumnId);
                }

                geometries.Add(geometryRecord);
                batchId++;
            }

            reader.Close();
             return geometries;
        }

        private static string[] GetColumnValuesAsList(NpgsqlDataReader reader, int columnId)
        {
            string[] res;
            if (reader.GetFieldType(columnId).Name == "String") {
                var attribute = reader.GetString(columnId);
                res = new string[1] { attribute };
            }
            else {
                res = reader.GetFieldValue<string[]>(columnId);
            }
            return res;
        }

        private static int FindField(NpgsqlDataReader reader, string fieldName)
        {
            var schema = reader.GetSchemaTable();
            var rows = schema.Columns["ColumnName"].Table.Rows;
            foreach(var row in rows) {
                var columnName= (string)((DataRow)row).ItemArray[0];
                if (columnName == fieldName) {
                    return (int)((DataRow)row).ItemArray[1];
                }
            }
            return 0;
        }
    }
}
