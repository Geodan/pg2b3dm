using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using B3dm.Tile;
using B3dm.Tileset;
using CommandLine;
using Newtonsoft.Json;
using Npgsql;
using Wkb2Gltf;

namespace pg2b3dm
{
    class Program
    {
        static string password = string.Empty;
        static int counter = 1;

        static void Main(string[] args)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine($"tool: pg2b3dm {version}");

            // 4978

            Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
                o.User = string.IsNullOrEmpty(o.User) ? Environment.UserName : o.User;
                o.Database = string.IsNullOrEmpty(o.Database) ? Environment.UserName : o.Database;

                var connectionString = $"Host={o.Host};Username={o.User};Database={o.Database};Port={o.Port}";
                var istrusted = TrustedConnectionChecker.HasTrustedConnection(connectionString);

                if (!istrusted) {
                    connectionString = $"Host={o.Host};Username={o.User};Password={password};Database={o.Database};Port={o.Port}";
                    Console.Write($"Password for user {o.User}: ");
                    password = PasswordAsker.GetPassword();
                    Console.WriteLine();
                }

                Console.WriteLine($"Start processing....");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                // Do in try catch as may not have acces rights.
                string output = o.Output;
                string outputTiles = output + "/tiles";
                if (!Directory.Exists(output)) {
                    Directory.CreateDirectory(output);
                }
                if (!Directory.Exists(outputTiles)) {
                    Directory.CreateDirectory(outputTiles);
                }

                var geometryTable = o.GeometryTable;
                var geometryColumn = o.GeometryColumn;
                Console.WriteLine("Calculating bounding boxes...");
                var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                var bbox3d = BoundingBoxRepository.GetBoundingBox3D(conn, geometryTable, geometryColumn);

                var translation = bbox3d.GetCenter().ToVector();
                var zupBoxes = GetZupBoxes(conn, geometryTable, geometryColumn, translation);
                var tree = TileCutter.ConstructTree(zupBoxes);

                Console.WriteLine("Writing tileset.json...");
                WiteTilesetJson(translation, tree, o.Output);

                Console.WriteLine($"Writing {Counter.Instance.Count} tiles...");
                WriteTiles(conn, geometryTable, geometryColumn, translation, tree, o.Output, o.RoofColorColumn);
                conn.Close();
                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine("Program finished.");
            });
        }
        private static void WriteTiles(NpgsqlConnection conn, string geometryTable, string geometryColumn, double[] translation, Node node, string outputPath, string colorColumn = "")
        {
            if (node.Features.Count > 0) {
                counter++;
                var subset = (from f in node.Features select (f.Id)).ToArray();
                var geometries = BoundingBoxRepository.GetGeometrySubset(conn, geometryTable, geometryColumn, translation, subset, colorColumn);

                var triangleCollection = Triangulator.GetTriangles(geometries);

                var bytes = GlbCreator.GetGlb(triangleCollection);
                var b3dm = new B3dm.Tile.B3dm(bytes);
                B3dmWriter.WriteB3dm($"{outputPath}/tiles/{node.Id}.b3dm", b3dm);

            }
            // and write children too
            foreach (var subnode in node.Children) {
               var perc = Math.Round(((double)counter / Counter.Instance.Count) * 100,2);
                Console.Write($"\rProgress: tile {counter} - {perc.ToString("F")}%");
                WriteTiles(conn, geometryTable, geometryColumn, translation, subnode, outputPath, colorColumn);
            }
        }

        private static void WiteTilesetJson(double[] translation, Node tree, string outputPath)
        {
            var tileset = TreeSerializer.ToTileset(tree, translation);
            var s = JsonConvert.SerializeObject(tileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText($"{outputPath}/tileset.json", s);
        }

        private static List<BoundingBox3D> GetZupBoxes(NpgsqlConnection conn, string GeometryTable, string GeometryColumn, double[] translation)
        {
            var bboxes = BoundingBoxRepository.GetAllBoundingBoxes(conn, GeometryTable, GeometryColumn, translation);
            var zupBoxes = new List<BoundingBox3D>();
            foreach (var bbox in bboxes) {
                var zupBox = bbox.TransformYToZ();
                zupBoxes.Add(zupBox);
            }

            return zupBoxes;
        }

    }
}
