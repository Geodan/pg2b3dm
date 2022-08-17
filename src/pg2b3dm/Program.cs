using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using B3dm.Tileset;
using CommandLine;
using Npgsql;
using Wkx;

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
                    if(args.Contains("-e") || args.Contains("--extenttile")) {
                        Console.WriteLine("Warning: parameter -e --extenttile is ignored with implicit tiling");
                    }
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

                var att = !String.IsNullOrEmpty(o.AttributeColumns) ? o.AttributeColumns : "-";
                Console.WriteLine($"Attribute columns: {att}");

                var box = bbox3d.GetBox();

                if (!o.UseImplicitTiling) {
                    var outputTiles = $"{output}{Path.AltDirectorySeparatorChar}tiles";
                    if (!Directory.Exists(outputTiles)) {
                        Directory.CreateDirectory(outputTiles);
                    }

                    // do not use implicit tiling
                    var tiles = TileCutter.GetTiles(0, conn, o.ExtentTile, geometryTable, geometryColumn, bbox3d, sr, 0, lods, geometricErrors.Skip(1).ToArray(), lodcolumn, query);
                    Console.WriteLine();
                    var nrOfTiles = RecursiveTileCounter.CountTiles(tiles.tiles, 0);
                    Console.WriteLine($"Tiles with features: {nrOfTiles} ");
                    CalculateBoundingBoxes(translation, tiles.tiles, bbox3d.ZMin, bbox3d.ZMax);
                    Console.WriteLine("Writing tileset.json...");

                    var minx = ConvertToRadians(bbox_wgs84.XMin);
                    var miny = ConvertToRadians(bbox_wgs84.YMin);
                    var maxx = ConvertToRadians(bbox_wgs84.XMax);
                    var maxy = ConvertToRadians(bbox_wgs84.YMax);

                    var region = new double[] { minx, miny, maxx, maxy, heights.min, heights.max }; 
                    var json = TreeSerializer.ToJson(tiles.tiles, translation, region, geometricErrors[0], o.Refinement, version);
                    File.WriteAllText($"{o.Output}{Path.AltDirectorySeparatorChar}tileset.json", json);
                    WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, tiles.tiles, sr, o.Output, 0, nrOfTiles,
             o.ShadersColumn, o.AttributeColumns, o.LodColumn, o.Copyright);
                }
                else {
                    // use implictit tiling
                    var contentDirectory = $"{output}{Path.AltDirectorySeparatorChar}content";
                    var subtreesDirectory = $"{output}{Path.AltDirectorySeparatorChar}subtrees";

                    if (!Directory.Exists(contentDirectory)) {
                        Directory.CreateDirectory(contentDirectory);
                    }
                    if (!Directory.Exists(subtreesDirectory)) {
                        Directory.CreateDirectory(subtreesDirectory);
                    }

                    Console.WriteLine($"Maximum features per tile: " + o.ImplicitTilingMaxFeatures);
                    var bbox = new BoundingBox(bbox3d.XMin, bbox3d.YMin, bbox3d.XMax, bbox3d.YMax);
                    var tile = new subtree.Tile(0, 0, 0);
                    var tiles = ImplicitTiling.GenerateTiles(geometryTable, conn, sr, geometryColumn, idcolumn, bbox, o.ImplicitTilingMaxFeatures, tile, new List<subtree.Tile>(), query, translation, o.ShadersColumn, o.AttributeColumns, contentDirectory, o.Copyright);
                    Console.WriteLine();
                    Console.WriteLine("Tiles created: " + tiles.Count);
                    var mortonIndex = subtree.MortonIndex.GetMortonIndex(tiles);
                    var subtreebytes = ImplicitTiling.GetSubtreeBytes(mortonIndex);

                    var subtreeFile = $"{subtreesDirectory}{Path.AltDirectorySeparatorChar}0_0_0.subtree";
                    Console.WriteLine($"Writing {subtreeFile}...");
                    File.WriteAllBytes(subtreeFile, subtreebytes);

                    var subtreeLevels = tiles.Max(t => t.Z) + 1;
                    var tilesetjson = TreeSerializer.ToImplicitTileset(translation, box, geometricErrors[0], subtreeLevels, o.Refinement, version);
                    var file = $"{o.Output}{Path.AltDirectorySeparatorChar}tileset.json";
                    Console.WriteLine("SubtreeLevels: " + subtreeLevels);
                    Console.WriteLine("SubdivisionScheme: QUADTREE");
                    Console.WriteLine("Refine method: ADD");
                    Console.WriteLine($"Geometric errors: {geometricErrors[0]}, {geometricErrors[0]}");
                    Console.WriteLine($"Writing {file}...");
                    File.WriteAllText(file, tilesetjson);
                }

                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine($"Program finished {DateTime.Now}.");
            });
        }

        public static double[] Reverse(double[] translation)
        {
            var res = new double[] { translation[0] * -1, translation[1] * -1, translation[2] * -1 };
            return res;
        }

        private static void CalculateBoundingBoxes(double[] translation, List<Tile> tiles, double minZ, double maxZ)
        {
            foreach (var t in tiles) {

                var bb = t.BoundingBox;
                var bvol = new BoundingBox3D(bb.XMin, bb.YMin, minZ, bb.XMax, bb.YMax, maxZ);

                if (t.Children != null) {

                    CalculateBoundingBoxes(translation, t.Children, minZ, maxZ);
                }
                t.Boundingvolume = TileCutter.GetBoundingvolume(bvol);
            }
        }

        private static int WriteTiles(NpgsqlConnection conn, string geometryTable, string geometryColumn, string idcolumn, double[] translation, List<Tile> tiles, int epsg, string outputPath, int counter, int maxcount, string colorColumn = "", string attributesColumns = "", string lodColumn="", string copyright = "", string query="")
        {
            foreach (var t in tiles) {
                counter++;
                var perc = Math.Round(((double)counter / maxcount) * 100, 2);
                Console.Write($"\rcreating tiles: {counter}/{maxcount} - {perc:F}%");

                var geometries = BoundingBoxRepository.GetGeometrySubset(conn, geometryTable, geometryColumn, idcolumn, translation, t, epsg, colorColumn, attributesColumns, lodColumn, query);

                var bytes = B3dmWriter.ToB3dm(geometries, copyright);

                File.WriteAllBytes($"{outputPath}/tiles/{counter}.b3dm", bytes);

                if (t.Children != null) {
                    counter = WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, t.Children, epsg, outputPath, counter, maxcount, colorColumn, attributesColumns, lodColumn, query);
                }

            }
            return counter;
        }
        private static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }


    }
}
