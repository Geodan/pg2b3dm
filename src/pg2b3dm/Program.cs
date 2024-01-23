using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using B3dm.Tileset;
using CommandLine;
using Npgsql;
using Newtonsoft.Json;
using subtree;
using B3dm.Tileset.Extensions;
using Wkx;
using SharpGLTF.Schema2;

namespace pg2b3dm;

class Program
{
    static string password = string.Empty;
    static bool skipCreateTiles = false; // could be useful for debugging purposes
    private static AppMode appMode;
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
            var createGltf = (bool)o.CreateGltf;

            var query = o.Query;

            var conn = new NpgsqlConnection(connectionString);

            var source_epsg = SpatialReferenceRepository.GetSpatialReference(conn, table, geometryColumn);

            if (source_epsg == 4978) {
                Console.WriteLine("----------------------------------------------------------------------------");
                Console.WriteLine("WARNING: Input geometries in ECEF (epsg:4978) are not supported in version >= 2.0.0");
                Console.WriteLine("Fix: Use local coordinate systems or EPSG:4326 in input datasource.");
                Console.WriteLine("----------------------------------------------------------------------------");
            }

            appMode = AppMode.Cesium;
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

            Console.WriteLine($"Query bounding box of {table}.{geometryColumn}...");
            var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, table, geometryColumn);
            var bbox = bbox_wgs84.bbox;

            Console.WriteLine($"Bounding box for {table}.{geometryColumn} (in WGS84): " +
                $"{Math.Round(bbox.XMin, 8)}, {Math.Round(bbox.YMin, 8)}, " +
                $"{Math.Round(bbox.XMax, 8)}, {Math.Round(bbox.YMax, 8)}");

            var zmin = bbox_wgs84.zmin;
            var zmax = bbox_wgs84.zmax;
            // int to_epsg = 4326;

            Console.WriteLine($"Height values: [{Math.Round(zmin, 2)} m - {Math.Round(zmax, 2)} m]");
            Console.WriteLine($"Default color: {defaultColor}");
            Console.WriteLine($"Default metallic roughness: {defaultMetallicRoughness}");
            Console.WriteLine($"Doublesided: {doubleSided}");
            Console.WriteLine($"Create glTF tiles: {createGltf}");

            var att = !string.IsNullOrEmpty(o.AttributeColumns) ? o.AttributeColumns : "-";
            Console.WriteLine($"Attribute columns: {att}");

            var contentDirectory = $"{output}{Path.AltDirectorySeparatorChar}content";

            if (!Directory.Exists(contentDirectory)) {
                Directory.CreateDirectory(contentDirectory);
            }
            var center_wgs84 = bbox.GetCenter();
            Console.WriteLine($"Center (wgs84): {center_wgs84.X}, {center_wgs84.Y}");

            // cesium specific
            if (appMode == AppMode.Cesium) {
                Tiles3DExtensions.RegisterExtensions();

                Console.WriteLine("Starting Cesium mode...");

                var translation = Translation.GetTranslation(center_wgs84);
                Console.WriteLine($"Translation ECEF: {String.Join(',', translation)}");

                var lodcolumn = o.LodColumn;
                var addOutlines = (bool)o.AddOutlines;
                var geometricErrors = Array.ConvertAll(o.GeometricErrors.Split(','), double.Parse);
                var useImplicitTiling = (bool)o.UseImplicitTiling;
                if (useImplicitTiling) {
                    if (!String.IsNullOrEmpty(lodcolumn)) {
                        Console.WriteLine("Warning: parameter -l --lodcolumn is ignored with implicit tiling");
                        lodcolumn = String.Empty;
                    }
                }
                Console.WriteLine($"Lod column: {lodcolumn}");
                Console.WriteLine($"Geometric errors: {String.Join(',', geometricErrors)}");
                Console.WriteLine($"Refinement: {o.Refinement}");

                var lods = (lodcolumn != string.Empty ? LodsRepository.GetLods(conn, table, lodcolumn, query) : new List<int> { 0 });
                if ((geometricErrors.Length != lods.Count + 1) && lodcolumn == string.Empty) {
                    Console.WriteLine($"Lod levels from database column {lodcolumn}: [{String.Join(',', lods)}]");
                    Console.WriteLine($"Geometric errors: {o.GeometricErrors}");

                    Console.WriteLine("Error: parameter -g --geometricerrors is wrongly specified...");
                    Console.WriteLine("end of program...");
                    Environment.Exit(0);
                }
                if (lodcolumn != String.Empty) {
                    Console.WriteLine($"Lod levels: {String.Join(',', lods)}");

                    if (lods.Count >= geometricErrors.Length) {
                        Console.WriteLine($"Calculating geometric errors starting from {geometricErrors[0]}");
                        geometricErrors = GeometricErrorCalculator.GetGeometricErrors(geometricErrors[0], lods);
                        Console.WriteLine($"Calculated geometric errors (for {lods.Count} levels): {String.Join(',', geometricErrors)}");
                    }
                };

                if (!useImplicitTiling) {
                    Console.WriteLine("Geometric errors used: " + String.Join(',', geometricErrors));
                }
                else {
                    Console.WriteLine("Geometric error used for implicit tiling: " + geometricErrors[0]);
                }
                Console.WriteLine($"Add outlines: {addOutlines}");
                Console.WriteLine($"Use 3D Tiles 1.1 implicit tiling: {o.UseImplicitTiling}");

                var rootBoundingVolumeRegion = bbox.ToRadians().ToRegion(zmin, zmax);

                var subtreesDirectory = $"{output}{Path.AltDirectorySeparatorChar}subtrees";

                Console.WriteLine($"Maximum features per tile: " + o.MaxFeaturesPerTile);

                var tile = new Tile(0, 0, 0);
                tile.BoundingBox = bbox.ToArray();
                Console.WriteLine($"Start generating tiles...");
                var quadtreeTiler = new QuadtreeTiler(conn, table, source_epsg, geometryColumn, o.MaxFeaturesPerTile, query, translation, o.ShadersColumn, o.AttributeColumns, lodcolumn, contentDirectory, lods, o.Copyright, skipCreateTiles);
                var tiles = quadtreeTiler.GenerateTiles(bbox, tile, new List<Tile>(), lodcolumn != string.Empty ? lods.First() : 0, addOutlines, defaultColor, defaultMetallicRoughness, doubleSided, createGltf);
                Console.WriteLine();
                Console.WriteLine("Tiles created: " + tiles.Count(tile => tile.Available));

                if (useImplicitTiling) {
                    if (!Directory.Exists(subtreesDirectory)) {
                        Directory.CreateDirectory(subtreesDirectory);
                    }

                    var subtreeFiles = SubtreeCreator.GenerateSubtreefiles(tiles);
                    Console.WriteLine($"Writing {subtreeFiles.Count} subtree files...");
                    foreach (var s in subtreeFiles) {
                        var t = s.Key;
                        var subtreefile = $"{subtreesDirectory}{Path.AltDirectorySeparatorChar}{t.Z}_{t.X}_{t.Y}.subtree";
                        File.WriteAllBytes(subtreefile, s.Value);
                    }

                    var subtreeLevels = subtreeFiles.Count > 1 ? ((Tile)subtreeFiles.ElementAt(1).Key).Z : 2;
                    var availableLevels = tiles.Max(t => t.Z) + 1;
                    Console.WriteLine("Available Levels: " + availableLevels);
                    Console.WriteLine("Subtree Levels: " + subtreeLevels);
                    var tilesetjson = TreeSerializer.ToImplicitTileset(translation, rootBoundingVolumeRegion, geometricErrors[0], availableLevels, subtreeLevels, version, createGltf);
                    var file = $"{o.Output}{Path.AltDirectorySeparatorChar}tileset.json";
                    var json = JsonConvert.SerializeObject(tilesetjson, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    Console.WriteLine("SubdivisionScheme: QUADTREE");
                    Console.WriteLine($"Writing {file}...");
                    File.WriteAllText(file, json);

                }
                else {
                    var refine = o.Refinement;
                    var json = TreeSerializer.ToJson(tiles, translation, rootBoundingVolumeRegion, geometricErrors, zmin, zmax, version, refine);
                    File.WriteAllText($"{o.Output}{Path.AltDirectorySeparatorChar}tileset.json", json);
                }
                // end cesium specific code

            }
            else {
                // mapbox specific code

                Console.WriteLine("Starting Experimental MapBox v3 mode...");
                Console.WriteLine("Mapbox mode v3 is not available in this version, progrma will exit...");
                Environment.Exit(0);

                var min_zoom = o.MinZoom;
                var max_zoom = o.MaxZoom;

                if (min_zoom > max_zoom) {
                    Console.WriteLine("Error: min zoom level is higher than max zoom level");
                    Environment.Exit(0);
                }

                for (var level = min_zoom; level <= max_zoom; level++) {
                    var tiles = Tiles.Tools.Tilebelt.GetTilesOnLevel(new double[] { bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax }, level);

                    Console.WriteLine($"Creating tiles for level {level}: {tiles.Count()}");

                    foreach (var t in tiles) {
                        var bounds = t.Bounds();

                        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, table, geometryColumn, new Point(bounds[0], bounds[1]), new Point(bounds[2], bounds[3]), query);

                        if (numberOfFeatures > 0) {
                            var center = t.Center();
                            var centerTileTranslation = new Point(center[0], center[1], 0); ;

                            var geometries = GeometryRepository.GetGeometrySubset(conn, table, geometryColumn, bounds, source_epsg, o.ShadersColumn, o.AttributeColumns, query);
                            var bytes = TileWriter.ToTile(geometries, new double[] {0,0,0}, o.Copyright, false, defaultColor, defaultMetallicRoughness);
                            File.WriteAllBytes($@"{contentDirectory}{Path.AltDirectorySeparatorChar}{t.Z}-{t.X}-{t.Y}.b3dm", bytes);
                            Console.Write(".");
                        }
                    }
                }
                // end mapbox specific code

            }

            stopWatch.Stop();

            Console.WriteLine();
            var timeSpan = stopWatch.Elapsed;
            Console.WriteLine("Time: {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            Console.WriteLine($"Program finished {DateTime.Now.ToLocalTime().ToString("s")}.");
        });
    }
}
