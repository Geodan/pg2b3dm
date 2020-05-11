using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using B3dm.Tile;
using B3dm.Tileset;
using CommandLine;
using Npgsql;
using Wkb2Gltf;

namespace pg2b3dm
{
    class Program
    {
        static string password = string.Empty

        static void Main(string[] args)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine($"tool: pg2b3dm {version}");

            Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
                o.User = string.IsNullOrEmpty(o.User) ? Environment.UserName : o.User;
                o.Database = string.IsNullOrEmpty(o.Database) ? Environment.UserName : o.Database;

                var connectionString = $"Host={o.Host};Username={o.User};Database={o.Database};Port={o.Port}";
                var istrusted = TrustedConnectionChecker.HasTrustedConnection(connectionString);

                if (!istrusted) {
                    Console.Write($"password for user {o.User}: ");
                    password = PasswordAsker.GetPassword();
                    connectionString += $";password={password}";
                    Console.WriteLine();
                }

                Console.WriteLine($"start processing....");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var output = o.Output;
                var outputTiles = $"{output}{Path.DirectorySeparatorChar}tiles";
                if (!Directory.Exists(output)) {
                    Directory.CreateDirectory(output);
                }
                if (!Directory.Exists(outputTiles)) {
                    Directory.CreateDirectory(outputTiles);
                }

                Console.WriteLine($"input table:  {o.GeometryTable}");
                Console.WriteLine($"input geometry column:  {o.GeometryColumn}");

                Console.WriteLine($"output directory:  {outputTiles}");

                var geometryTable = o.GeometryTable;
                var geometryColumn = o.GeometryColumn;
                var idcolumn = o.IdColumn;
                var lodcolumn = o.LodColumn;
                var geometricErrors = Array.ConvertAll(o.GeometricErrors.Split(','), double.Parse); ;

                var conn = new NpgsqlConnection(connectionString);

                var lods = (lodcolumn != string.Empty ? LodsRepository.GetLods(conn, geometryTable, lodcolumn) : new List<int> { 0 });
                if((geometricErrors.Length != lods.Count + 1) && lodcolumn==string.Empty) {
                    Console.WriteLine($"lod levels: [{ String.Join(',', lods)}]");
                    Console.WriteLine($"geometric errors: {o.GeometricErrors}");

                    Console.WriteLine("error: parameter -g --geometricerrors is wrongly specified...");
                    Console.WriteLine("end of program...");
                    Environment.Exit(0);
                }
                if (lodcolumn != String.Empty){
                    Console.WriteLine($"lod levels: {String.Join(',', lods)}");

                    if (lods.Count >= geometricErrors.Length) {
                        Console.WriteLine($"calculating geometric errors starting from {geometricErrors[0]}");
                        geometricErrors = GeometricErrorCalculator.GetGeometricErrors(geometricErrors[0], lods);
                    }
                };
                Console.WriteLine("geometric errors: " + String.Join(',', geometricErrors));

                var bbox3d = BoundingBoxRepository.GetBoundingBox3DForTable(conn, geometryTable, geometryColumn);
                // Console.WriteLine($"3D Boundingbox {geometryTable}.{geometryColumn}: [{bbox3d.XMin}, {bbox3d.YMin}, {bbox3d.ZMin},{bbox3d.XMax},{bbox3d.YMax}, {bbox3d.ZMax}]");
                var translation = bbox3d.GetCenter().ToVector();
               //  Console.WriteLine($"translation {geometryTable}.{geometryColumn}: [{string.Join(',', translation) }]");
                var boundingboxAllFeatures = BoundingBoxCalculator.TranslateRotateX(bbox3d, Reverse(translation), Math.PI / 2);
                var box = boundingboxAllFeatures.GetBox();
                var sr = SpatialReferenceRepository.GetSpatialReference(conn, geometryTable, geometryColumn);
                Console.WriteLine($"spatial reference: {sr}");
                var tiles = TileCutter.GetTiles(0, conn, o.ExtentTile, geometryTable, geometryColumn, bbox3d, sr, 0, lods, geometricErrors.Skip(1).ToArray(), lodcolumn);
                Console.WriteLine();
                var nrOfTiles = RecursiveTileCounter.CountTiles(tiles.tiles, 0);
                Console.WriteLine($"tiles with features: {nrOfTiles} ");
                CalculateBoundingBoxes(translation, tiles.tiles, bbox3d.ZMin, bbox3d.ZMax);
                Console.WriteLine("writing tileset.json...");
                var json = TreeSerializer.ToJson(tiles.tiles, translation, box, geometricErrors[0], o.Refinement);
                File.WriteAllText($"{o.Output}/tileset.json", json);

                WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, tiles.tiles, sr, o.Output, 0, nrOfTiles, o.RoofColorColumn, o.AttributesColumn, o.LodColumn);

                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine("program finished.");
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
                var bvolRotated = BoundingBoxCalculator.TranslateRotateX(bvol, Reverse(translation), Math.PI / 2);

                if (t.Children != null) {

                    CalculateBoundingBoxes(translation, t.Children, minZ, maxZ);
                }
                t.Boundingvolume = TileCutter.GetBoundingvolume(bvolRotated);
            }
        }

        private static int WriteTiles(NpgsqlConnection conn, string geometryTable, string geometryColumn, string idcolumn, double[] translation, List<Tile> tiles, int epsg, string outputPath, int counter, int maxcount, string colorColumn = "", string attributesColumn = "", string lodColumn="")
        {
            foreach (var t in tiles) {
                counter++;
                var perc = Math.Round(((double)counter / maxcount) * 100, 2);
                Console.Write($"\rcreating tiles: {counter}/{maxcount} - {perc:F}%");

                var geometries = BoundingBoxRepository.GetGeometrySubset(conn, geometryTable, geometryColumn, idcolumn, translation, t, epsg, colorColumn, attributesColumn, lodColumn);

                var triangleCollection = GetTriangles(geometries);

                var attributes = GetAttributes(geometries);

                var b3dm = B3dmCreator.GetB3dm(attributesColumn, attributes, triangleCollection);

                B3dmWriter.WriteB3dm($"{outputPath}/tiles/{counter}.b3dm", b3dm);

                if (t.Children != null) {
                    counter = WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, t.Children, epsg, outputPath, counter, maxcount, colorColumn, attributesColumn, lodColumn);
                }

            }
            return counter;
        }

        private static List<object> GetAttributes(List<GeometryRecord> geometries)
        {
            var allattributes = new List<object>();
            foreach (var geom in geometries) {
                // only take the first now....
                allattributes.Add(geom.Attributes[0]);
            }
            return allattributes;
        }

        public static List<Wkb2Gltf.Triangle> GetTriangles(List<GeometryRecord> geomrecords)
        {
            var triangleCollection = new List<Wkb2Gltf.Triangle>();
            foreach (var g in geomrecords) {
                var triangles = g.GetTriangles();
                triangleCollection.AddRange(triangles);
            }

            return triangleCollection;
        }

    }
}
