using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using B3dm.Tileset;
using CommandLine;
using Npgsql;
using B3dm.Tileset.extensions;
using Newtonsoft.Json;
using Tile = B3dm.Tileset.Tile;
using Humanizer;
using subtree;

namespace pg2b3dm;

class Program
{
    static string password = string.Empty;

    static void Main(string[] args)
    {
        var version = Assembly.GetEntryAssembly().GetName().Version;
        Console.WriteLine($"Tool: pg2b3dm {version}");

        Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
            o.User = string.IsNullOrEmpty(o.User) ? Environment.UserName : o.User;
            o.Database = string.IsNullOrEmpty(o.Database) ? Environment.UserName : o.Database;

            var connectionString = $"Host={o.Host};Username={o.User};Database={o.Database};Port={o.Port};CommandTimeOut={o.SqlCommandTimeout}";
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

            var geometryTable = o.GeometryTable;
            var geometryColumn = o.GeometryColumn;
            var lodcolumn = o.LodColumn;
            var query = o.Query;
            var geometricErrors = Array.ConvertAll(o.GeometricErrors.Split(','), double.Parse);
            var useImplicitTiling = (bool)o.UseImplicitTiling;
            if (useImplicitTiling) {
                if (args.Contains("-l") || args.Contains("--lodcolumn")) {
                    Console.WriteLine("Warning: parameter -l --lodcolumn is ignored with implicit tiling");
                    lodcolumn = String.Empty;
                }
            }
            Console.WriteLine($"Lod column: {lodcolumn}");
            Console.WriteLine($"Geometric errors: {String.Join(',',geometricErrors)}");

            var conn = new NpgsqlConnection(connectionString);

            var lods = (lodcolumn != string.Empty ? LodsRepository.GetLods(conn, geometryTable, lodcolumn, query) : new List<int> { 0 });
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
                    Console.WriteLine($"Calculated geometric errors (for {lods.Count} levels): {geometricErrors}");
                }
            };

            if (!useImplicitTiling) {
                Console.WriteLine("Geometric errors used: " + String.Join(',', geometricErrors));
            }
            else {
                Console.WriteLine("Geometric error used for implicit tiling: " + geometricErrors[0]);
            }

            var sr = SpatialReferenceRepository.GetSpatialReference(conn, geometryTable, geometryColumn);
            Console.WriteLine($"Spatial reference of {geometryTable}.{geometryColumn}: {sr}");
            Console.WriteLine($"Query bounding box of {geometryTable}.{geometryColumn}...");
            var bbox_wgs84 = BoundingBoxRepository.GetBoundingBoxForTable(conn, geometryTable, geometryColumn, query);
            Console.WriteLine($"Bounding box for {geometryTable}.{geometryColumn} (in WGS84): {Math.Round(bbox_wgs84.XMin, 4)}, {Math.Round(bbox_wgs84.YMin, 4)}, {Math.Round(bbox_wgs84.XMax, 4)}, {Math.Round(bbox_wgs84.YMax, 4)}");

            var heightsArray = o.BoundingVolumeHeights.Split(',');
            (double min, double max) heights = (double.Parse(heightsArray[0]), double.Parse(heightsArray[1]));

            Console.WriteLine($"Heights for bounding volume: [{heights.min} m, {heights.max} m] ");
            var center_wgs84 = bbox_wgs84.GetCenter();

            double[] translation;
            if (sr == 4978) {
                var v3 = SpatialConverter.GeodeticToEcef((double)center_wgs84.X, (double)center_wgs84.Y, 0);
                translation = new double[] { v3.X, v3.Y, v3.Z };
            }
            else {
                translation = SphericalMercator.ToSphericalMercatorFromWgs84((double)center_wgs84.X, (double)center_wgs84.Y);
            }
            Console.WriteLine($"Translation: {String.Join(',', translation)}");

            Console.WriteLine($"Use 3D Tiles 1.1 implicit tiling: {o.UseImplicitTiling}");
            var att = !string.IsNullOrEmpty(o.AttributeColumns) ? o.AttributeColumns : "-";
            Console.WriteLine($"Attribute columns: {att}");

            var rootBoundingVolumeRegion = bbox_wgs84.ToRadians().ToRegion(heights.min, heights.max);

            var contentDirectory = $"{output}{Path.AltDirectorySeparatorChar}content";
            var subtreesDirectory = $"{output}{Path.AltDirectorySeparatorChar}subtrees";

            if (!Directory.Exists(contentDirectory)) {
                Directory.CreateDirectory(contentDirectory);
            }

            Console.WriteLine($"Maximum features per tile: " + o.MaxFeaturesPerTile);

            var tile = new Tile(0, 0, 0);
            tile.BoundingBox = bbox_wgs84;
            Console.WriteLine($"Start generating tiles...");
            var quadtreeTiler = new QuadtreeTiler(conn, geometryTable, sr, geometryColumn, o.MaxFeaturesPerTile, query, translation, o.ShadersColumn, o.AttributeColumns, lodcolumn, contentDirectory, lods, o.Copyright);
            var tiles = quadtreeTiler.GenerateTiles(bbox_wgs84, tile, new List<Tile>(), lodcolumn != string.Empty ? lods.First():0);
            Console.WriteLine();
            Console.WriteLine("Tiles created: " + tiles.Count(tile => tile.Available));

            if (useImplicitTiling) {
                if (!Directory.Exists(subtreesDirectory)) {
                    Directory.CreateDirectory(subtreesDirectory);
                }

                var subtreeFile = $"{subtreesDirectory}{Path.AltDirectorySeparatorChar}0_0_0.subtree";
                Console.WriteLine($"Writing {subtreeFile}...");
                var subtreebytes = SubtreeCreator.GenerateSubtreefile(tiles);
                File.WriteAllBytes(subtreeFile, subtreebytes);
                var availableLevels = tiles.Max(t => t.Z) + 1;
                var subtreeLevels = 1; // hardcoded for now
                var tilesetjson = TreeSerializer.ToImplicitTileset(translation, rootBoundingVolumeRegion, geometricErrors[0], availableLevels, subtreeLevels, version);
                var file = $"{o.Output}{Path.AltDirectorySeparatorChar}tileset.json";
                Console.WriteLine("Available Levels: " + availableLevels);
                Console.WriteLine("Subtree Levels: " + subtreeLevels);
                Console.WriteLine("SubdivisionScheme: QUADTREE");
                Console.WriteLine("Refine method: ADD");
                Console.WriteLine($"Writing {file}...");

                var json = JsonConvert.SerializeObject(tilesetjson, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                File.WriteAllText(file, json);
            }
            else {
                var refine = lodcolumn != String.Empty ? "REPLACE" : "ADD";
                var json = TreeSerializer.ToJson(tiles, translation, rootBoundingVolumeRegion, geometricErrors, heights.min, heights.max, version);
                File.WriteAllText($"{o.Output}{Path.AltDirectorySeparatorChar}tileset.json", json);
            }

            stopWatch.Stop();
            Console.WriteLine();
            Console.WriteLine($"Elapsed: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).Humanize()}");
            Console.WriteLine($"Program finished {DateTime.Now.ToLocalTime().ToString("s")}.");
        });
    }
}
