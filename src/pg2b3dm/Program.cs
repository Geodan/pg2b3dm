using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using B3dm.Tileset;
using CommandLine;
using Npgsql;
using subtree;
using B3dm.Tileset.Extensions;
using SharpGLTF.Schema2;

namespace pg2b3dm;

class Program
{
    static string password = string.Empty;
    static void Main(string[] args)
    {
        var version = Assembly.GetEntryAssembly().GetName().Version;
        Console.WriteLine($"Tool: pg2b3dm {version}");
        Console.WriteLine("Options: " + string.Join(" ", args));
        Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
            o.User = string.IsNullOrEmpty(o.User) ? Environment.UserName : o.User;
            o.Database = string.IsNullOrEmpty(o.Database) ? Environment.UserName : o.Database;

            var connectionString = $"Host={o.Host};Username={o.User};Database={o.Database};Port={o.Port};CommandTimeOut=0";
            var istrusted = TrustedConnectionChecker.HasTrustedConnection(connectionString);
            if (!istrusted) {
                Console.Write($"Password for user {o.User}: ");
                password = PasswordAsker.GetPassword();
                connectionString += $";password={password}";
                Console.WriteLine();
            }

            Console.WriteLine($"Start processing {DateTime.Now.ToLocalTime().ToString("s")}....");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var output = o.Output;
            if (!Directory.Exists(output)) {
                Directory.CreateDirectory(output);
            }

            Console.WriteLine($"Input table: {o.GeometryTable}");
            if (o.Query != String.Empty) {
                Console.WriteLine($"Query:  {o.Query ?? "-"}");
            }
            Console.WriteLine($"Input geometry column: {o.GeometryColumn}");

            var table = o.GeometryTable;
            var geometryColumn = o.GeometryColumn;
            var defaultColor = o.DefaultColor;
            var defaultMetallicRoughness = o.DefaultMetallicRoughness;
            var doubleSided = (bool)o.DoubleSided;
            var defaultAlphaMode = o.DefaultAlphaMode;
            var createGltf = (bool)o.CreateGltf;
            var outputDirectory = o.Output;
            var zoom = o.Zoom;
            var shadersColumn = o.ShadersColumn;
            var attributeColumns = o.AttributeColumns;
            var copyright = o.Copyright;
            var tilesetVersion = o.TilesetVersion;
            var keepProjection = (bool)o.KeepProjection;

            var query = o.Query;

            var conn = new NpgsqlConnection(connectionString);

            var source_epsg = SpatialReferenceRepository.GetSpatialReference(conn, table, geometryColumn, query);

            if (source_epsg == 4978) {
                Console.WriteLine("----------------------------------------------------------------------------");
                Console.WriteLine("WARNING: Input geometries in ECEF (epsg:4978) are not supported in version >= 2.0.0");
                Console.WriteLine("Fix: Use local coordinate systems or EPSG:4326 in input datasource.");
                Console.WriteLine("----------------------------------------------------------------------------");
            }

            Console.WriteLine("App mode: " + o.AppMode);
            Console.WriteLine($"Spatial reference of {table}.{geometryColumn}: {source_epsg}");

            // Check spatialIndex
            var hasSpatialIndex = SpatialIndexChecker.HasSpatialIndex(conn, table, geometryColumn);
            if (!hasSpatialIndex) {
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine($"WARNING: No spatial index detected on {table}.{geometryColumn}");
                Console.WriteLine("Fix: add a spatial index, for example: ");
                Console.WriteLine($"'CREATE INDEX ON {table} USING gist(st_centroid(st_envelope({geometryColumn})))'");
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine();
            }
            else {
                Console.WriteLine($"Spatial index detected on {table}.{geometryColumn}");
            }
            
            var skipCreateTiles = o.SkipCreateTiles;
            Console.WriteLine("Skip create tiles: " + skipCreateTiles);

            Console.WriteLine($"Query bounding box of {table}.{geometryColumn}...");
            var where = (query != string.Empty ? $" where {query}" : String.Empty);

            var bbox_table = BoundingBoxRepository.GetBoundingBoxForTable(conn, table, geometryColumn, keepProjection , where);
            var bbox = bbox_table.bbox;

            var proj = keepProjection? $"EPSG:{source_epsg}": $"EPSG:4326 (WGS84)";
            Console.WriteLine($"Bounding box for {table}.{geometryColumn} ({proj}): " +
                $"{Math.Round(bbox.XMin, 8)}, {Math.Round(bbox.YMin, 8)}, " +
                $"{Math.Round(bbox.XMax, 8)}, {Math.Round(bbox.YMax, 8)}");

            var zmin = bbox_table.zmin;
            var zmax = bbox_table.zmax;

            Console.WriteLine($"Height values: [{Math.Round(zmin, 2)} m - {Math.Round(zmax, 2)} m]");
            Console.WriteLine($"Default color: {defaultColor}");
            Console.WriteLine($"Default metallic roughness: {defaultMetallicRoughness}");
            Console.WriteLine($"Doublesided: {doubleSided}");
            Console.WriteLine($"Default AlphaMode: {defaultAlphaMode}");
            Console.WriteLine($"Create glTF tiles: {createGltf}");

            var att = !string.IsNullOrEmpty(o.AttributeColumns) ? o.AttributeColumns : "-";
            Console.WriteLine($"Attribute columns: {att}");

            var contentDirectory = $"{output}{Path.AltDirectorySeparatorChar}content";

            if (!Directory.Exists(contentDirectory)) {
                Directory.CreateDirectory(contentDirectory);
            }
            var center = bbox.GetCenter();
            Console.WriteLine($"Center ({proj}): {center.X}, {center.Y}");

            Tiles3DExtensions.RegisterExtensions();

            // cesium specific
            if (o.AppMode == AppMode.Cesium) {

                Console.WriteLine("Starting Cesium mode...");

                var translation =  keepProjection?
                    new double[] { (double)center.X, (double)center.Y, 0 } :
                    Translation.ToEcef(center);
                Console.WriteLine($"Translation: {String.Join(',', translation)}");

                var lodcolumn = o.LodColumn;
                var addOutlines = (bool)o.AddOutlines;
                var useImplicitTiling = (bool)o.UseImplicitTiling;
                var refinement = o.Refinement;
                if (useImplicitTiling) {
                    if (!String.IsNullOrEmpty(lodcolumn)) {
                        Console.WriteLine("Warning: parameter -l --lodcolumn is ignored with implicit tiling");
                        lodcolumn = String.Empty;
                    }
                }
                // if useImpliciting is false and createGlb is false, the set use10 to true
                var use10 = !useImplicitTiling && !createGltf;
                Console.WriteLine("3D Tiles version: " + (use10 ? "1.0" : "1.1"));
                Console.WriteLine($"Lod column: {lodcolumn}");
                Console.WriteLine($"Radius column: {o.RadiusColumn}");
                Console.WriteLine($"Geometric error: {o.GeometricError}");
                Console.WriteLine($"Geometric error factor: {o.GeometricErrorFactor}");
                Console.WriteLine($"Refinement: {refinement}");
                Console.WriteLine($"Keep projection: {keepProjection}");

                var lods = (lodcolumn != string.Empty ? LodsRepository.GetLods(conn, table, lodcolumn, query) : new List<int> { 0 });
                if (lodcolumn != String.Empty) {
                    Console.WriteLine($"Lod levels: {String.Join(',', lods)}");
                };

                Console.WriteLine($"Add outlines: {addOutlines}");
                Console.WriteLine($"Use 3D Tiles 1.1 implicit tiling: {o.UseImplicitTiling}");
                if(!tilesetVersion.Equals(string.Empty)) {
                    Console.WriteLine($"Tileset version: {tilesetVersion}");
                }

                var rootBoundingVolumeRegion = 
                    keepProjection?
                        bbox.ToRegion(zmin, zmax) : 
                        bbox.ToRadians().ToRegion(zmin, zmax);

                var subtreesDirectory = $"{output}{Path.AltDirectorySeparatorChar}subtrees";

                Console.WriteLine($"Maximum features per tile: " + o.MaxFeaturesPerTile);

                var tile = new Tile(0, 0, 0);
                tile.BoundingBox = bbox.ToArray();
                Console.WriteLine($"Start generating tiles...");
                var quadtreeTiler = new QuadtreeTiler(conn, table, source_epsg, geometryColumn, o.MaxFeaturesPerTile, query, translation, o.ShadersColumn, o.AttributeColumns, lodcolumn, contentDirectory, lods, o.Copyright, skipCreateTiles, o.RadiusColumn);
                var tiles = quadtreeTiler.GenerateTiles(bbox, tile, new List<Tile>(), lodcolumn != string.Empty ? lods.First() : 0, addOutlines, defaultColor, defaultMetallicRoughness, doubleSided, defaultAlphaMode, createGltf, keepProjection);
                Console.WriteLine();
                Console.WriteLine("Tiles created: " + tiles.Count(tile => tile.Available));

                var crs = keepProjection ?
                    $"EPSG:{source_epsg}" :
                    "";

                if (tiles.Count(tile => tile.Available) > 0) {
                    if (useImplicitTiling) {
                        CesiumTiler.CreateImplicitTileset(version, createGltf, outputDirectory, translation, o.GeometricError, rootBoundingVolumeRegion, subtreesDirectory, tiles, tilesetVersion, crs, keepProjection);
                    }
                    else {
                        CesiumTiler.CreateExplicitTilesetsJson(version, outputDirectory, translation, o.GeometricError, o.GeometricErrorFactor, refinement, use10, rootBoundingVolumeRegion, tile, tiles, tilesetVersion, crs, keepProjection);
                    }
                }
                Console.WriteLine();

                // end cesium specific code

            }
            else {
                MapboxTiler.CreateMapboxTiles(table, geometryColumn, defaultColor, defaultMetallicRoughness, createGltf, zoom, shadersColumn, attributeColumns, copyright, query, conn, source_epsg, bbox, contentDirectory);
            }

            stopWatch.Stop();

            var timeSpan = stopWatch.Elapsed;
            Console.WriteLine("Time: {0}h {1}m {2}s {3}ms", Math.Floor(timeSpan.TotalHours), timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            Console.WriteLine($"Program finished {DateTime.Now.ToLocalTime().ToString("s")}.");
        });
    }
}
