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

namespace pg2b3dm
{
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

                var connectionString = $"Host={o.Host};Username={o.User};Database={o.Database};Port={o.Port}";
                var istrusted = TrustedConnectionChecker.HasTrustedConnection(connectionString);
                if (!istrusted) {
                    Console.Write($"Password for user {o.User}: ");
                    password = PasswordAsker.GetPassword();
                    connectionString += $";password={password}";
                    Console.WriteLine();
                }

                Console.WriteLine($"Start processing {DateTime.Now}....");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var output = o.Output;
                if (!Directory.Exists(output)) {
                    Directory.CreateDirectory(output);
                }

                Console.WriteLine($"Input table: {o.GeometryTable}");
                if (o.Query != String.Empty) {
                    Console.WriteLine($"Query:  {o.Query??"-"}");
                }
                Console.WriteLine($"input geometry column: {o.GeometryColumn}");

                var geometryTable = o.GeometryTable;
                var geometryColumn = o.GeometryColumn;
                var idcolumn = o.IdColumn;
                var lodcolumn = o.LodColumn;
                var query = o.Query;
                var geometricErrors = Array.ConvertAll(o.GeometricErrors.Split(','), double.Parse);

                if (o.UseImplicitTiling) {
                    if (args.Contains("-l") || args.Contains("--lodcolumn")) {
                        Console.WriteLine("Warning: parameter -l --lodcolumn is ignored with implicit tiling");
                    }
                }

                var conn = new NpgsqlConnection(connectionString);

                var lods = (lodcolumn != string.Empty ? LodsRepository.GetLods(conn, geometryTable, lodcolumn,query) : new List<int> { 0 });
                if((geometricErrors.Length != lods.Count + 1) && lodcolumn==string.Empty) {
                    Console.WriteLine($"Lod levels: [{ String.Join(',', lods)}]");
                    Console.WriteLine($"Geometric errors: {o.GeometricErrors}");

                    Console.WriteLine("Error: parameter -g --geometricerrors is wrongly specified...");
                    Console.WriteLine("end of program...");
                    Environment.Exit(0);
                }
                if (lodcolumn != String.Empty){
                    Console.WriteLine($"Lod levels: {String.Join(',', lods)}");

                    if (lods.Count >= geometricErrors.Length) {
                        Console.WriteLine($"Calculating geometric errors starting from {geometricErrors[0]}");
                        geometricErrors = GeometricErrorCalculator.GetGeometricErrors(geometricErrors[0], lods);
                    }
                };

                if (!o.UseImplicitTiling) {
                    Console.WriteLine("Geometric errors: " + String.Join(',', geometricErrors));
                }
                else {
                    Console.WriteLine("Geometric error: " + geometricErrors[0]);
                }

                var sr = SpatialReferenceRepository.GetSpatialReference(conn, geometryTable, geometryColumn);
                Console.WriteLine($"Spatial reference: {sr}");
                Console.WriteLine($"Query bounding box for table {geometryTable}...");
                var bbox3d = BoundingBoxRepository.GetBoundingBox3DForTable(conn, geometryTable, geometryColumn, query);
                var bbox_wgs84 = BoundingBoxRepository.ToWgs84(conn, bbox3d, sr);
                Console.WriteLine($"Bounding box for table (WGS84): {Math.Round(bbox_wgs84.XMin,4)}, {Math.Round(bbox_wgs84.YMin,4)}, {Math.Round(bbox_wgs84.XMax,4)}, {Math.Round(bbox_wgs84.YMax,4)}");
                Console.WriteLine($"Query heights for table {geometryTable}...");
                var heights = BoundingBoxRepository.GetHeight(conn, geometryTable, geometryColumn, query);
                Console.WriteLine($"Heights for table: [{heights.min}, {heights.max}] m");
                var translation = bbox3d.GetCenter().ToVector();
                Console.WriteLine($"Use 3D Tiles 1.1 implicit tiling: {o.UseImplicitTiling}");
                var bbox_3857 = bbox_wgs84.ToSpherical();
                
                var att = !string.IsNullOrEmpty(o.AttributeColumns) ? o.AttributeColumns : "-";
                Console.WriteLine($"Attribute columns: {att}");

                var rootBoundingVolumeRegion = bbox_wgs84.ToRadians().ToRegion(heights.min, heights.max);
                
                var contentDirectory = $"{output}{Path.AltDirectorySeparatorChar}content";
                var subtreesDirectory = $"{output}{Path.AltDirectorySeparatorChar}subtrees";

                if (!Directory.Exists(contentDirectory)) {
                    Directory.CreateDirectory(contentDirectory);
                }
                if (!Directory.Exists(subtreesDirectory)) {
                    Directory.CreateDirectory(subtreesDirectory);
                }

                Console.WriteLine($"Maximum features per tile: " + o.ImplicitTilingMaxFeatures);

                var tile = new Tile(0, 0, 0);
                tile.BoundingBox = bbox_3857;
                var tiles = ImplicitTiling.GenerateTiles(geometryTable, conn, sr, geometryColumn, idcolumn, bbox_3857, o.ImplicitTilingMaxFeatures, tile, new List<Tile>(), query, translation, o.ShadersColumn, o.AttributeColumns, contentDirectory, o.Copyright);
                Console.WriteLine();
                Console.WriteLine("Tiles created: " + tiles.Count);

                var json = TreeSerializer.ToJson(tiles, translation, rootBoundingVolumeRegion, geometricErrors, o.Refinement, heights.min, heights.max, version);
                File.WriteAllText($"{o.Output}{Path.AltDirectorySeparatorChar}tileset.json", json);

                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine($"Program finished {DateTime.Now}.");
            });
        }
    }
}
