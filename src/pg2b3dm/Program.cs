using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using B3dm.Tile;
using B3dm.Tileset;
using CommandLine;
using glTFLoader;
using glTFLoader.Schema;
using Newtonsoft.Json;
using Wkb2Gltf;
using Wkx;

namespace pg2b3dm
{
    class Program
    {
        static string password = string.Empty;

        static void Main(string[] args)
        {
            Console.WriteLine("tool: pg2b3dm");

            Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
                o.User = string.IsNullOrEmpty(o.User)? Environment.UserName :o.User;
                o.Database = string.IsNullOrEmpty(o.Database) ? Environment.UserName : o.Database;

                Console.Write($"Password for user {o.User}: ");
                ConsoleKeyInfo keyInfo;

                do {
                    keyInfo = Console.ReadKey(true);
                    // Skip if Backspace or Enter is Pressed
                    if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter) {
                        password += keyInfo.KeyChar;
                        Console.Write("*");
                    }
                    else {
                        if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0) {
                            // Remove last charcter if Backspace is Pressed
                            password = password.Substring(0, (password.Length - 1));
                            Console.Write("\b \b");
                        }
                    }
                }
                // Stops Getting Password Once Enter is Pressed
                while (keyInfo.Key != ConsoleKey.Enter);

                Console.WriteLine();
                Console.WriteLine($"Start processing....");

                var connectionString = $"Host={o.Host};Username={o.User};Password={password};Database={o.Database};Port={o.Port}";
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                if (!Directory.Exists("./output")) {
                    Directory.CreateDirectory("./output");
                }
                if (!Directory.Exists("./output/tiles")) {
                    Directory.CreateDirectory("./output/tiles");
                }

                var geometryTable = o.GeometryTable;
                var geometryColumn = o.GeometryColumn;
                Console.WriteLine("Calculating bounding boxes...");
                var bbox3d = BoundingBoxRepository.GetBoundingBox3D(connectionString, geometryTable, geometryColumn);
                var translation = bbox3d.GetCenter().ToVector();
                var zupBoxes = GetZupBoxes(connectionString, geometryTable, geometryColumn, translation);
                var tree = TileCutter.ConstructTree(zupBoxes);

                Console.WriteLine("Writing tileset.json...");
                WiteTilesetJson(translation, tree);

                Console.WriteLine("Writing tiles...");
                var material = MaterialMaker.CreateMaterial("Material_house", 139 / 255f, 69 / 255f, 19 / 255f, 1.0f);
                WriteTiles(connectionString, geometryTable, geometryColumn, translation, tree, material);

                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine("Program finished. Press any key to continue...");
                Console.ReadKey();
            });
        }

        private static void WriteTiles(string connectionString, string geometryTable, string geometryColumn, double[] translation, B3dm.Tileset.Node node, Material material)
        {
            if (node.Features.Count > 0) {
                var subset = (from f in node.Features select (f.Id)).ToArray();
                var geometries = BoundingBoxRepository.GetGeometrySubset(connectionString, geometryTable, geometryColumn, translation, subset);
                WriteB3dm(geometries, node.Id, translation, material);
            }
            // and write children too
            foreach (var subnode in node.Children) {
                Console.Write(".");
                WriteTiles(connectionString, geometryTable, geometryColumn, translation, subnode, material);
            }
        }

        private static void WiteTilesetJson(double[] translation, B3dm.Tileset.Node tree)
        {
            var tileset = TreeSerializer.ToTileset(tree, translation);
            var s = JsonConvert.SerializeObject(tileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText("./output/tileset.json", s);
        }

        private static List<BoundingBox3D> GetZupBoxes(string connectionString, string GeometryTable, string GeometryColumn, double[] translation)
        {
            var bboxes = BoundingBoxRepository.GetAllBoundingBoxes(connectionString, GeometryTable, GeometryColumn, translation);
            var zupBoxes = new List<BoundingBox3D>();
            foreach (var bbox in bboxes) {
                var zupBox = bbox.TransformYToZ();
                zupBoxes.Add(zupBox);
            }

            return zupBoxes;
        }

        private static void WriteB3dm(List<GeometryRecord> geomrecords, int tile_id, double[] translation, Material material)
        {
            var triangleCollection = new TriangleCollection();
            foreach(var g in geomrecords) {
                var surface = (PolyhedralSurface)g.Geometry;
                var triangles = Triangulator.GetTriangles(surface);
                triangleCollection.AddRange(triangles);
            }

            var bb = GetBoundingBox3D(geomrecords);
            var gltfArray = Gltf2Loader.GetGltfArray(triangleCollection, bb);
            var gltfall = Gltf2Loader.ToGltf(gltfArray, translation, material);
            var ms = new MemoryStream();
            gltfall.Gltf.SaveBinaryModel(gltfall.Body, ms);
            var glb = ms.ToArray();
            var b3dm = new B3dm.Tile.B3dm(glb);
            B3dmWriter.WriteB3dm($"./output/tiles/{tile_id}.b3dm", b3dm);
        }

        private static BoundingBox3D GetBoundingBox3D(List<GeometryRecord> records)
        {
            var bboxes = new List<BoundingBox3D>();
            foreach (var record in records) {
                var surface = (PolyhedralSurface)record.Geometry;
                var bbox = surface.GetBoundingBox3D();
                bboxes.Add(bbox);
            }
            var combinedBoundingBox = BoundingBoxCalculator.GetBoundingBox(bboxes);

            return combinedBoundingBox;
        }
    }
}
