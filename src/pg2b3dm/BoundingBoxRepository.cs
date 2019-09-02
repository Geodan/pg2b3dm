using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Npgsql;
using Wkb2Gltf;
using Wkx;

namespace pg2b3dm
{
    public static class BoundingBoxRepository
    {
        public static BoundingBox3D GetBoundingBox3D(NpgsqlConnection conn, string geometry_table, string geometry_column)
        {
            var cmd = new NpgsqlCommand($"SELECT st_xmin(geom1), st_ymin(geom1), st_zmin(geom1), st_xmax(geom1), st_ymax(geom1), st_zmax(geom1) FROM (select ST_3DExtent({geometry_column}) as geom1 from {geometry_table} where ST_GeometryType(geom) =  'ST_PolyhedralSurface') as t", conn);
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

        private static string GetGeometryTable(string geometry_table, string geometry_column, double[] translation, string colorColumn = "", string attributesColumn="")
        {
            var sqlSelect = $"select ST_RotateX(ST_Translate({geometry_column}, {translation[0].ToString(CultureInfo.InvariantCulture)}*-1,{translation[1].ToString(CultureInfo.InvariantCulture)}*-1 , {translation[2].ToString(CultureInfo.InvariantCulture)}*-1), -pi() / 2) as geom1, ST_Area(ST_Force2D(geom)) AS weight ";

            var optionalColumns = SqlBuilder.GetOptionalColumnsSql(colorColumn, attributesColumn);
            sqlSelect += $"{optionalColumns} ";
            var sqlFrom = $"FROM {geometry_table} ";
            var sqlWhere = $"where ST_GeometryType(geom) =  'ST_PolyhedralSurface' ORDER BY weight DESC";
            return sqlSelect+ sqlFrom + sqlWhere;
        }

        public static List<BoundingBox3D> GetAllBoundingBoxes(NpgsqlConnection conn, string geometry_table, string geometry_column, double[] translation)
        {
            var geometryTable = GetGeometryTable(geometry_table, geometry_column, translation);
            var sql = $"SELECT ST_XMIN(geom1),ST_YMIN(geom1),ST_ZMIN(geom1), ST_XMAX(geom1),ST_YMAX(geom1),ST_ZMAX(geom1) FROM ({geometryTable}) as t";
            var cmd = new NpgsqlCommand(sql, conn);
            var bboxes = new List<BoundingBox3D>();
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                var xmin = reader.GetDouble(0);
                var ymin = reader.GetDouble(1);
                var zmin = reader.GetDouble(2);
                var xmax = reader.GetDouble(3);
                var ymax = reader.GetDouble(4);
                var zmax = reader.GetDouble(5);
                var bbox = new BoundingBox3D(xmin,ymin,zmin,xmax,ymax,zmax);
                bboxes.Add(bbox);
            }
            reader.Close();
            return bboxes;
        }

        public static List<GeometryRecord> GetGeometrySubset(NpgsqlConnection conn, string geometry_table, string geometry_column, double[] translation, int[] row_numbers, string colorColumn = "", string attributesColumn = "")
        {
            var geometries = new List<GeometryRecord>();
            var new_row_numbers= Array.ConvertAll(row_numbers, x => x+1);
            var ids = string.Join(",", new_row_numbers);
            var geometryTable = GetGeometryTable(geometry_table, geometry_column, translation, colorColumn, attributesColumn);
            var sqlselect = $"select row_number, ST_AsBinary(geom1)";
            if (colorColumn != String.Empty) {
                sqlselect = $"{sqlselect}, {colorColumn} ";
            }
            if (attributesColumn != String.Empty) {
                sqlselect = $"{sqlselect}, {attributesColumn} ";
            }

            var sqlFrom = $"from(SELECT row_number() over(), geom1 FROM({geometryTable}) as t) as p ";

            var optionalcolumns = SqlBuilder.GetOptionalColumnsSql(colorColumn, attributesColumn);

            if (colorColumn != String.Empty) {
                sqlFrom = $"from(SELECT row_number() over(), geom1 {optionalcolumns} FROM({geometryTable}) as t) as p ";
            }
            var sqlWhere = $"where row_number in ({ids})";

            var sql = sqlselect + sqlFrom + sqlWhere;

            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            var attributesColumnId=int.MinValue;
            if (attributesColumn != String.Empty) {
                attributesColumnId = FindField(reader, attributesColumn);
            }
            var batchId = 0;
            while (reader.Read()) {
                var rownumber = reader.GetInt32(0);
                var stream = reader.GetStream(1);

                var g = Geometry.Deserialize<WkbSerializer>(stream);
                var geometryRecord = new GeometryRecord (batchId) { RowNumber = rownumber, Geometry = g };

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
            var res=new string[0];
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
