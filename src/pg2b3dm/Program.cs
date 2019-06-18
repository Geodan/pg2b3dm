using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        static int counter = 1;

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
                    connectionString = $"Host={o.Host};Username={o.User};Password={password};Database={o.Database};Port={o.Port}";
                    Console.Write($"Password for user {o.User}: ");
                    password = PasswordAsker.GetPassword();
                    Console.WriteLine();
                }

                Console.WriteLine($"Start processing....");

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

                Console.WriteLine($"Writing {Counter.Instance.Count} tiles...");
                var material = MaterialMaker.CreateMaterial("Material_house", 139 / 255f, 69 / 255f, 19 / 255f, 1.0f);
                WriteTiles(connectionString, geometryTable, geometryColumn, translation, tree, material);

                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine("Program finished.");
            });
        }

        private static void WriteTiles(string connectionString, string geometryTable, string geometryColumn, double[] translation, B3dm.Tileset.Node node, Material material)
        {
            if (node.Features.Count > 0) {
                counter++;
                var subset = (from f in node.Features select (f.Id)).ToArray();
                var geometries = BoundingBoxRepository.GetGeometrySubset(connectionString, geometryTable, geometryColumn, translation, subset);
                WriteB3dm(geometries, node.Id, translation, material);
            }
            // and write children too
            foreach (var subnode in node.Children) {
               var perc = Math.Round(((double)counter / Counter.Instance.Count) * 100,2);
                Console.Write($"\rProgress: tile {counter} - {perc.ToString("F")}%");
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
