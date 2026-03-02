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
    public static double[] GetGeometriesBoundingBox(NpgsqlConnection conn, string geometry_table, string geometry_column, int epsg, Tile t, HashSet<string> tileHashes, string query = "", bool keepProjection = false)
    {
        var sqlSelect = keepProjection?
            $"select st_Asbinary(st_3dextent({geometry_column})) ":
            $"select st_Asbinary(st_3dextent(st_transform({geometry_column}, 4979))) ";

        var sqlWhere = $" MD5(ST_AsBinary({geometry_column})::text) = ANY(@hashes)";
        var sql = $"{sqlSelect} from {geometry_table} where {sqlWhere}";

        conn.Open();
        try {
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("hashes", tileHashes.ToArray());
            using var reader = cmd.ExecuteReader();
            reader.Read();
            var stream = reader.GetStream(0);
            var geometry = Geometry.Deserialize<WkbSerializer>(stream);
            var result = BBox3D.GetBoundingBoxPoints(geometry);

            return result;
        }
        finally {
            conn.Close();
        }
    }

    public static List<GeometryRecord> GetGeometrySubset(NpgsqlConnection conn, string geometry_table, string geometry_column, double[] bbox, int source_epsg, int target_srs, string shaderColumn = "", string attributesColumns = "", string query = "", string radiusColumn = "", HashSet<string> excludeHashes = null, int? maxFeatures = null, SortBy sortBy = SortBy.AREA, bool keepProjection = false, string idColumn = "", bool includeTextures = false)
    {
        var sqlselect = GetSqlSelect(geometry_column, shaderColumn, attributesColumns, radiusColumn, target_srs, idColumn);
        var sqlFrom = "FROM " + geometry_table;
        var points = GetPoints(bbox);

        var sqlWhere = GetWhere(geometry_column, points.fromPoint, points.toPoint, query, source_epsg);
        
        // Add hash exclusion filter using parameterized query
        if (excludeHashes != null && excludeHashes.Count > 0) {
            sqlWhere += $" AND MD5(ST_AsBinary({geometry_column})::text) != ALL(@excludeHashes)";
        }
        
        var sqlOrderBy = GetOrderBy(geometry_column, sortBy);
        var sqlLimit = maxFeatures.HasValue ? $" LIMIT {maxFeatures.Value}" : "";
        var sql = sqlselect + sqlFrom + " where " + sqlWhere + sqlOrderBy + sqlLimit;

        conn.Open();
        List<GeometryRecord> geometries;
        try {
            using var cmd = new NpgsqlCommand(sql, conn);
            if (excludeHashes != null && excludeHashes.Count > 0) {
                cmd.Parameters.AddWithValue("excludeHashes", excludeHashes.ToArray());
            }

            geometries = GetGeometries(cmd, shaderColumn, attributesColumns, radiusColumn, idColumn);        
        }
        finally {
            conn.Close();
        }
        if (includeTextures) {
            EnrichWithTextures(conn, geometries);
        }
        return geometries;
    }

    public static string GetWhere(string geometry_column, Point from, Point to, string query, int source_epsg)
    {
        var fromX = from.X.Value.ToString(CultureInfo.InvariantCulture);
        var fromY = from.Y.Value.ToString(CultureInfo.InvariantCulture);
        var toX = to.X.Value.ToString(CultureInfo.InvariantCulture);
        var toY = to.Y.Value.ToString(CultureInfo.InvariantCulture);

        var hasZ = from.Z.HasValue && to.Z.HasValue;
        var where = "";

        if (!hasZ) {
            where = $"ST_Centroid(ST_Envelope({geometry_column})) && ST_MakeEnvelope({fromX}, {fromY}, {toX}, {toY}, {source_epsg}) {query}";
        }
        else {
            var fromBox = $"st_setsrid(ST_MakePoint({fromX}, {fromY}, {from.Z.Value.ToString(CultureInfo.InvariantCulture)}), {source_epsg})";
            var toBox = $"st_setsrid(ST_MakePoint({toX}, {toY}, {to.Z.Value.ToString(CultureInfo.InvariantCulture)}), {source_epsg})";

            var geom = $"st_setsrid(st_makepoint((st_xmin({geometry_column}) + st_xmax({geometry_column}))/2,(st_ymin({geometry_column}) + st_ymax({geometry_column}))/2, (st_zmin({geometry_column}) + st_zmax({geometry_column}))/2), {source_epsg})";
            where = $"ST_3DIntersects({geom}, ST_3DMakeBox({fromBox}, {toBox})) {query}";
        }

        return where;
    }

    public static string GetSqlSelect(string geometry_column, string shaderColumn, string attributesColumns, string radiusColumn, int target_srs, string idColumn = "")
    {
        var g = GetGeometryColumn(geometry_column, target_srs);
        var sqlselect = $"SELECT ST_AsBinary({g})";
        if (idColumn != String.Empty) {
            sqlselect = $"{sqlselect}, {idColumn} ";
        }
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

    public static string GetOrderBy(string geometry_column, SortBy sortBy)
    {
        if (sortBy == SortBy.VOLUME) {
            return $" ORDER BY (ST_XMax({geometry_column}) - ST_XMin({geometry_column})) *(ST_YMax({geometry_column}) - ST_YMin({geometry_column})) *(ST_ZMax({geometry_column}) - ST_ZMin({geometry_column})) DESC";
        }
        return $" ORDER BY ST_Area(ST_Envelope({geometry_column})) DESC";
    }

    public static List<GeometryRecord> GetGeometries(NpgsqlCommand cmd, string shaderColumn, string attributesColumns, string radiusColumn, string idColumn = "", string geometry_column = "")
    {
        var geometries = new List<GeometryRecord>();
        var reader = cmd.ExecuteReader();
        var attributesColumnIds = new Dictionary<string, int>();
        var shadersColumnId = int.MinValue;
        var radiusColumnId = int.MinValue;
        var hashColumnId = int.MinValue;
        var idColumnId = int.MinValue;

        if (attributesColumns != String.Empty) {
            var attributesColumnsList = attributesColumns.Split(',').ToList();
            attributesColumnIds = FindFields(reader, attributesColumnsList);
        }
        if (idColumn != String.Empty) {
            var fld = FindField(reader, idColumn);
            if (fld.HasValue) {
                idColumnId = fld.Value;
            }
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
            if (idColumn != string.Empty && idColumnId >= 0) {
                var id = reader.GetFieldValue<object>(idColumnId);
                geometryRecord.SourceId = Convert.ToInt64(id);
            }

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
        return geometries;
    }

    private static void EnrichWithTextures(NpgsqlConnection conn, List<GeometryRecord> geometries)
    {
        var sourceIds = geometries
            .Where(g => g.SourceId.HasValue)
            .Select(g => g.SourceId!.Value)
            .Distinct()
            .ToArray();

        if (sourceIds.Length == 0) {
            return;
        }

        var geometriesById = geometries
            .Where(g => g.SourceId.HasValue)
            .GroupBy(g => g.SourceId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        const string sql = @"
SELECT g.id,
       g.geometry_properties::text AS geometry_properties,
       sdm.texture_mapping::text AS texture_mapping,
       ti.mime_type,
       ti.image_data
FROM citydb.geometry_data g
JOIN citydb.surface_data_mapping sdm
  ON sdm.geometry_data_id = g.id
JOIN citydb.surface_data sd
  ON sd.id = sdm.surface_data_id
JOIN citydb.tex_image ti
  ON ti.id = sd.tex_image_id
WHERE g.id = ANY(@ids)
  AND sdm.texture_mapping IS NOT NULL
  AND ti.image_data IS NOT NULL
ORDER BY g.id, sdm.surface_data_id";

        conn.Open();
        var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("ids", sourceIds);
        var reader = cmd.ExecuteReader();

        while (reader.Read()) {
            var sourceId = Convert.ToInt64(reader.GetFieldValue<object>(0));
            if (!geometriesById.TryGetValue(sourceId, out var geometryRecord)) {
                continue;
            }

            if (string.IsNullOrWhiteSpace(geometryRecord.GeometryProperties)) {
                geometryRecord.GeometryProperties = reader.IsDBNull(1) ? String.Empty : reader.GetString(1);
            }

            var textureMapping = reader.IsDBNull(2) ? String.Empty : reader.GetString(2);
            var textureMimeType = reader.IsDBNull(3) ? String.Empty : reader.GetString(3);
            var textureImageData = reader.GetFieldValue<byte[]>(4);

            var texture = new GeometryTexture() {
                TextureMapping = textureMapping,
                TextureMimeType = textureMimeType,
                TextureImageData = textureImageData
            };

            if (texture.IsValid()) {
                geometryRecord.Textures.Add(texture);
            }

            // keep legacy fields for backward compatibility with older call paths
            if (string.IsNullOrWhiteSpace(geometryRecord.TextureMapping) && geometryRecord.TextureImageData.Length == 0) {
                geometryRecord.TextureMapping = textureMapping;
                geometryRecord.TextureMimeType = textureMimeType;
                geometryRecord.TextureImageData = textureImageData;
            }
        }

        reader.Close();
        conn.Close();
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
