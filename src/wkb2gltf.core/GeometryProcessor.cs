using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Triangulate;
using Wkx;

namespace Wkb2Gltf;

public static class GeometryProcessor
{
    private static Geometry GetGeometry(Geometry multiGeometry, int i)
    {
        switch (multiGeometry) {
            case MultiPolygon multiPolygon:
                return multiPolygon.Geometries[i];
            case MultiLineString multiLineString:
                return multiLineString.Geometries[i];
            case PolyhedralSurface polyhedralSurface:
                return polyhedralSurface.Geometries[i];
            default:
                throw new NotSupportedException($"Geometry type {multiGeometry.GeometryType} is not supported");
        }
    }

    private static int Count (Geometry multiGeometry)
    {
       switch (multiGeometry)
        {
            case MultiPolygon multiPolygon:
                return multiPolygon.Geometries.Count;
            case MultiLineString multiLineString:
                return multiLineString.Geometries.Count;
            case PolyhedralSurface polyhedralSurface:
                return polyhedralSurface.Geometries.Count;
            default:
                throw new NotSupportedException($"Geometry type {multiGeometry.GeometryType} is not supported");
        }
    }

    public static List<Triangle> GetTriangles(Geometry geometry, int batchId, double[] translation = null, double[] scale = null, ShaderColors shadercolors = null, float? radius = null, string textureMapping = "", string geometryProperties = "", byte[] textureImageData = null, string textureMimeType = "", List<GeometryTexture> textures = null)
    {
        var r = radius.HasValue ? radius.Value : (float)1.0f;

        var textureSets = GetTextureSets(textureMapping, textureImageData, textureMimeType, textures);
        if (CanUseTextureMapping(geometry, textureSets)) {
            var texturedTriangles = GetTexturedTriangles(geometry, batchId, translation, scale, geometryProperties, textureSets);
            if (texturedTriangles.Count > 0) {
                return texturedTriangles;
            }
        }

        if (geometry is MultiPolygon || geometry is MultiLineString || geometry is PolyhedralSurface)
        {
            if(shadercolors != null)
            {
                var numberOfGeometries = Count(geometry);

                if (numberOfGeometries == shadercolors.Count()) {
                    return GetTrianglesForMultiGeometries(geometry, batchId, translation, scale, shadercolors, r, numberOfGeometries);
                }
            }
        }

        List<Polygon> geometries;

        switch (geometry)
        {
            case LineString lineString:
                geometries = GetTrianglesFromLines(lineString, r, translation, scale);
                break;
            case MultiLineString multiLineString:
                geometries = GetTrianglesFromLines(multiLineString, r, translation, scale);
                break;
            case Polygon:
            case MultiPolygon:
            case PolyhedralSurface:
                geometries = GetTrianglesFromPolygons(geometry, translation);
                break;
            case Tin tin:
                var nmp = new PolyhedralSurface(tin.Geometries.Select(t => new Polygon(t.ExteriorRing.Points)));
                geometries = GetTrianglesFromPolygons(nmp, translation);
                break;
            default:
                throw new NotSupportedException($"Geometry type {geometry.GeometryType} is not supported");
        }

        var result = GetTriangles(batchId, shadercolors, geometries);

        return result;
    }

    private static List<Triangle> GetTrianglesForMultiGeometries(Geometry geometry, int batchId, double[] translation, double[] scale, ShaderColors shadercolors, float? radius, int numberOfGeometries)
    {
        var result1 = new List<Triangle>();
        // Do special treatment
        for (var i = 0; i < numberOfGeometries; i++) {
            var geom = GetGeometry(geometry, i);
            var shader = shadercolors.ToShader(i);
            var shaderColor = ShaderColors.ToShaderColors(shader);

            var geometryRecord = new GeometryRecord(batchId) {
                Geometry = geom,
                Shader = shaderColor,
                Radius = radius
            };
            result1.AddRange(geometryRecord.GetTriangles(translation, scale));
        }
        return result1;
    }

    private static List<GeometryTexture> GetTextureSets(string textureMapping, byte[] textureImageData, string textureMimeType, List<GeometryTexture> textures)
    {
        var textureSets = new List<GeometryTexture>();
        if (textures != null && textures.Count > 0) {
            textureSets.AddRange(textures.Where(texture => texture != null && texture.IsValid()));
        }

        if (textureSets.Count == 0 && !string.IsNullOrWhiteSpace(textureMapping) && textureImageData != null && textureImageData.Length > 0) {
            textureSets.Add(new GeometryTexture() {
                TextureMapping = textureMapping,
                TextureImageData = textureImageData,
                TextureMimeType = textureMimeType
            });
        }

        return textureSets;
    }

    private static bool CanUseTextureMapping(Geometry geometry, List<GeometryTexture> textureSets)
    {
        return (geometry is Polygon || geometry is MultiPolygon || geometry is PolyhedralSurface)
            && textureSets.Any(texture => texture.IsValid());
    }

    private static List<Triangle> GetTexturedTriangles(Geometry geometry, int batchId, double[] translation, double[] scale, string geometryProperties, List<GeometryTexture> textureSets)
    {
        var parsedTextureSets = textureSets
            .Where(texture => texture.IsValid())
            .Select(texture => new ParsedTextureSet {
                Texture = texture,
                TextureMappings = ParseTextureMappings(texture.TextureMapping)
            })
            .Where(texture => texture.TextureMappings.Count > 0)
            .ToList();

        if (parsedTextureSets.Count == 0) {
            return [];
        }

        var objectIdsByGeometryIndex = ParseGeometryObjectIds(geometryProperties);
        var mappingObjectIds = parsedTextureSets
            .SelectMany(texture => texture.TextureMappings.Keys)
            .Distinct()
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        var sourcePolygons = GetGeometries(geometry);
        var relativePolygons = GetRelativePolygons(sourcePolygons, translation, scale);
        var allTriangles = new List<Triangle>();

        for (var i = 0; i < relativePolygons.Count; i++) {
            var relativePolygon = relativePolygons[i];
            var trianglesForPolygon = GetTrianglesForPolygon(relativePolygon, batchId);

            var objectId = ResolveObjectId(objectIdsByGeometryIndex, mappingObjectIds, i);
            var isTextured = false;

            if (!string.IsNullOrEmpty(objectId)) {
                foreach (var textureSet in parsedTextureSets) {
                    if (!textureSet.TextureMappings.TryGetValue(objectId, out var polygonTextureCoordinates)) {
                        continue;
                    }

                    var lookup = GetTextureCoordinateLookup(relativePolygon, polygonTextureCoordinates);
                    if (lookup.Count == 0) {
                        continue;
                    }

                    ApplyTexture(trianglesForPolygon, lookup, textureSet.Texture);
                    isTextured = true;
                    break;
                }
            }

            if (!isTextured) {
                foreach (var textureSet in parsedTextureSets) {
                    foreach (var polygonTextureCoordinates in textureSet.TextureMappings.Values) {
                        var lookup = GetTextureCoordinateLookup(relativePolygon, polygonTextureCoordinates);
                        if (lookup.Count == 0) {
                            continue;
                        }

                        ApplyTexture(trianglesForPolygon, lookup, textureSet.Texture);
                        isTextured = true;
                        break;
                    }

                    if (isTextured) {
                        break;
                    }
                }
            }

            allTriangles.AddRange(trianglesForPolygon);
        }

        return allTriangles;
    }

    private static void ApplyTexture(List<Triangle> trianglesForPolygon, Dictionary<string, Vector2> lookup, GeometryTexture texture)
    {
        foreach (var triangle in trianglesForPolygon) {
            if (!TryGetTextureCoordinates(triangle, lookup, out var textureCoordinates)) {
                continue;
            }

            triangle.TextureCoordinates = textureCoordinates;
            triangle.TextureImageData = texture.TextureImageData;
            triangle.TextureMimeType = texture.TextureMimeType;
        }
    }

    private static string ResolveObjectId(Dictionary<int, string> objectIdsByGeometryIndex, List<string> mappingObjectIds, int geometryIndex)
    {
        if (objectIdsByGeometryIndex.TryGetValue(geometryIndex, out var objectId)) {
            return objectId;
        }

        if (objectIdsByGeometryIndex.Count == 1) {
            return objectIdsByGeometryIndex.First().Value;
        }

        if (mappingObjectIds.Count == 1) {
            return mappingObjectIds[0];
        }

        if (objectIdsByGeometryIndex.Count == 0 && geometryIndex >= 0 && geometryIndex < mappingObjectIds.Count) {
            return mappingObjectIds[geometryIndex];
        }

        return string.Empty;
    }

    private class ParsedTextureSet
    {
        public GeometryTexture Texture { get; set; }

        public Dictionary<string, List<List<Vector2>>> TextureMappings { get; set; } = [];
    }

    private static List<Triangle> GetTrianglesForPolygon(Polygon polygon, int batchId)
    {
        if (polygon.ExteriorRing.Points.Count == 4 && polygon.InteriorRings.Count == 0) {
            return [GetTriangle(polygon, batchId)];
        }

        var multipolygon = new MultiPolygon(new List<Polygon> { polygon });
        var triangulated = Triangulator.Triangulate(multipolygon);
        var triangles = ((MultiPolygon)triangulated).Geometries;
        return GetTriangles(batchId, null, triangles);
    }

    private static Dictionary<string, Vector2> GetTextureCoordinateLookup(Polygon polygon, List<List<Vector2>> polygonTextureCoordinates)
    {
        var lookup = new Dictionary<string, Vector2>();
        var rings = new List<LinearRing>() { polygon.ExteriorRing };
        rings.AddRange(polygon.InteriorRings);

        if (rings.Count != polygonTextureCoordinates.Count) {
            return lookup;
        }

        for (var ringIndex = 0; ringIndex < rings.Count; ringIndex++) {
            var ring = rings[ringIndex];
            var coordinates = polygonTextureCoordinates[ringIndex];
            if (ring.Points.Count != coordinates.Count) {
                return new Dictionary<string, Vector2>();
            }

            for (var i = 0; i < ring.Points.Count; i++) {
                var point = ring.Points[i];
                var coordinate = coordinates[i];
                lookup[ToPointKey(point)] = new Vector2(coordinate.X, 1 - coordinate.Y);
            }
        }

        return lookup;
    }

    private static bool TryGetTextureCoordinates(Triangle triangle, Dictionary<string, Vector2> lookup, out (Vector2, Vector2, Vector2) textureCoordinates)
    {
        var p0 = triangle.GetP0();
        var p1 = triangle.GetP1();
        var p2 = triangle.GetP2();

        var hasP0 = lookup.TryGetValue(ToPointKey(p0), out var t0);
        var hasP1 = lookup.TryGetValue(ToPointKey(p1), out var t1);
        var hasP2 = lookup.TryGetValue(ToPointKey(p2), out var t2);

        if (hasP0 && hasP1 && hasP2) {
            textureCoordinates = (t0, t1, t2);
            return true;
        }

        textureCoordinates = default;
        return false;
    }

    private static Dictionary<int, string> ParseGeometryObjectIds(string geometryProperties)
    {
        var result = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(geometryProperties)) {
            return result;
        }

        var geometryPropertiesJson = JObject.Parse(geometryProperties);
        var rootObjectId = geometryPropertiesJson["objectId"]?.Value<string>();
        if (!string.IsNullOrWhiteSpace(rootObjectId)) {
            result[0] = rootObjectId;
        }

        if (geometryPropertiesJson["children"] is JArray children) {
            var childIndex = 0;
            foreach (var child in children.OfType<JObject>()) {
                var objectId = child["objectId"]?.Value<string>();
                if (string.IsNullOrWhiteSpace(objectId)) {
                    childIndex++;
                    continue;
                }

                var geometryIndex = child["geometryIndex"]?.Value<int?>() ?? childIndex;
                result[geometryIndex] = objectId;
                childIndex++;
            }
        }

        return result;
    }

    private static Dictionary<string, List<List<Vector2>>> ParseTextureMappings(string textureMapping)
    {
        var result = new Dictionary<string, List<List<Vector2>>>();
        var mappingJson = JObject.Parse(textureMapping);
        foreach (var property in mappingJson.Properties()) {
            if (property.Value is not JArray ringMappings) {
                continue;
            }

            var rings = new List<List<Vector2>>();
            foreach (var ringMapping in ringMappings) {
                if (ringMapping is not JArray ringCoordinates) {
                    continue;
                }

                var coordinates = new List<Vector2>();
                foreach (var coordinate in ringCoordinates) {
                    if (coordinate is not JArray textureCoordinate || textureCoordinate.Count < 2) {
                        continue;
                    }

                    var s = textureCoordinate[0].Value<float?>();
                    var t = textureCoordinate[1].Value<float?>();
                    if (!s.HasValue || !t.HasValue) {
                        continue;
                    }

                    coordinates.Add(new Vector2(s.Value, t.Value));
                }

                rings.Add(coordinates);
            }

            result[property.Name] = rings;
        }
        return result;
    }

    private static string ToPointKey(Point point)
    {
        var x = Convert.ToDouble(point.X).ToString("R", CultureInfo.InvariantCulture);
        var y = Convert.ToDouble(point.Y).ToString("R", CultureInfo.InvariantCulture);
        var z = Convert.ToDouble(point.Z).ToString("R", CultureInfo.InvariantCulture);
        return $"{x}|{y}|{z}";
    }


    private static List<Polygon> GetTrianglesFromLines(MultiLineString line, float radius, double[] translation = null, double[] scale = null, int? tabularSegments = 64, int? radialSegments = 8)
    {
        var relativeLine = GetRelativeLine(line, translation, scale);
        var triangles = Triangulator.Triangulate(relativeLine, radius, radialSegments: radialSegments);
        return triangles.Geometries;
    }

    private static List<Polygon> GetTrianglesFromLines(LineString line, float radius, double[] translation = null, double[] scale = null, int? tabularSegments = 64, int? radialSegments = 8)
    {
        var relativeLine = GetRelativeLine(line, translation, scale);
        var triangles = Triangulator.Triangulate(relativeLine, radius, radialSegments: radialSegments);
        return triangles.Geometries;
    }


    private static List<Polygon> GetTrianglesFromPolygons(Geometry geometry, double[] translation = null, double[] scale = null)
    {
        var geometries = GetGeometries(geometry);

        var isTriangulated = IsTriangulated(geometries);

        var relativePolygons = GetRelativePolygons(geometries, translation, scale);
        var m1 = new MultiPolygon(relativePolygons);

        if (!isTriangulated) {
            var triangles = Triangulator.Triangulate(m1);
            geometries = ((MultiPolygon)triangles).Geometries;
        }
        else {
            geometries = m1.Geometries;
        }

        return geometries;
    }

    private static List<Polygon> GetGeometries(Geometry geometry)
    {
        switch (geometry)
        {
            case Polygon polygon:
                return new List<Polygon>() { polygon };
            case MultiPolygon multiPolygon:
                return multiPolygon.Geometries;
            case PolyhedralSurface surface:
                return surface.Geometries;
            default:
                throw new NotSupportedException($"Geometry type {geometry.GeometryType} is not supported");
        }
    }

    private static MultiLineString GetRelativeLine(MultiLineString multiline, double[] translation = null, double[] scale = null)
    {
        var result = new MultiLineString();
        foreach (var line in multiline.Geometries) {
            var relativeLine = GetRelativeLine(line, translation, scale);
            result.Geometries.Add(relativeLine);
        }
        return result;
    }

    private static LineString GetRelativeLine(LineString line, double[] translation = null, double[] scale = null)
    {
        var result = new LineString();
        foreach (var pnt in line.Points) {
            var relativePoint = ToRelativePoint(pnt, translation, scale);
            result.Points.Add(relativePoint);
        }
        return result;
    }

    private static List<Polygon> GetRelativePolygons(List<Polygon> geometries, double[] translation = null, double[] scale = null)
    {
        var relativePolygons = new List<Polygon>();

        foreach (var geometry in geometries) {
            var exteriorRing = new LinearRing();
            var interiorRings = new List<LinearRing>();
            foreach (var point in geometry.ExteriorRing.Points) {
                var relativePoint = ToRelativePoint(point, translation, scale);
                exteriorRing.Points.Add(relativePoint);
            }

            foreach (var interiorRing in geometry.InteriorRings) {
                var relativeInteriorRing = new LinearRing();
                foreach (var point in interiorRing.Points) {
                    var relativePoint = ToRelativePoint(point, translation, scale);
                    relativeInteriorRing.Points.Add(relativePoint);
                }
                interiorRings.Add(relativeInteriorRing);
            }
            var relativePolygon = new Polygon(exteriorRing, interiorRings);
            relativePolygons.Add(relativePolygon);
        }

        return relativePolygons;
    }

    private static List<Triangle> GetTriangles(int batchId, ShaderColors shadercolors, List<Polygon> geometries)
    {
        var degenerated_triangles = 0;
        var allTriangles = new List<Triangle>();
        for (var i = 0; i < geometries.Count; i++) {
            var geometry = geometries[i];
            var triangle = GetTriangle(geometry, batchId);

            if (triangle != null && shadercolors != null) {
                shadercolors.Validate(geometries.Count);
                triangle.Shader = shadercolors.ToShader(i);
            }

            if (triangle != null) {
                allTriangles.Add(triangle);
            }
            else {
                degenerated_triangles++;
            }
        }

        return allTriangles;
    }

    public static Triangle GetTriangle(Polygon geometry, int batchId)
    {
        var pnts = geometry.ExteriorRing.Points;
        if (pnts.Count != 4) {
            throw new ArgumentOutOfRangeException($"Expected number of vertices in triangles: 4, actual: {pnts.Count}");
        }

        var triangle = new Triangle(pnts[0], pnts[1], pnts[2], batchId);

        return triangle;
    }

    private static Point ToRelativePoint(Point pnt, double[] translation = null, double[] scale = null)
    {
        Point res;
        if (translation != null) {
            res = new Point((double)pnt.X - translation[0], (double)pnt.Y - translation[1], pnt.Z - translation[2]);
        }
        else {
            res = pnt;
        }
        if (scale != null) {
            res = new Point((double)res.X * scale[0], (double)res.Y * scale[1], res.Z * scale[2]);
        }

        return res;
    }

    private static bool IsTriangulated(List<Polygon> polygons)
    {
        foreach (var polygon in polygons) {
            if (polygon.ExteriorRing.Points.Count != 4) {
                return false;
            }
        }
        return true;
    }
}
