using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using B3dm.Tile;
using B3dm.Tileset;
using CommandLine;
using Newtonsoft.Json;
using Npgsql;
using Wkb2Gltf;
using Wkx;

namespace pg2b3dm
{
    class Program
    {
        static string password = string.Empty;

        static void Main(string[] args)
        {
            var epsg = 4978; // todo: make dynamic
            var version = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine($"tool: pg2b3dm {version}");

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

                Console.WriteLine($"Start processing....");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var output = o.Output;
                var outputTiles = output + "/tiles";
                if (!Directory.Exists(output)) {
                    Directory.CreateDirectory(output);
                }
                if (!Directory.Exists(outputTiles)) {
                    Directory.CreateDirectory(outputTiles);
                }

                var geometryTable = o.GeometryTable;
                var geometryColumn = o.GeometryColumn;
                var idcolumn = o.IdColumn;
                Console.WriteLine("Calculating bounding boxes...");
                var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                var bbox3d = BoundingBoxRepository.GetBoundingBox3D(conn, geometryTable, geometryColumn);
                var translation = bbox3d.GetCenter().ToVector();
                var zupBoxes = TileCutter.GetZupBoxes(conn, geometryTable, geometryColumn, idcolumn, translation);
                var boundingAllActualNew = BoundingBoxCalculator.GetBoundingAll(bbox3d, translation);
                var box = boundingAllActualNew.GetBox();
                Console.WriteLine("Writing tileset.json...");
                var tiles = TileCutter.GetTiles(conn, o.ExtentTile, geometryTable, geometryColumn, idcolumn, bbox3d, epsg);

                foreach(var t in tiles) {
                    var tileZupBoxes = new List<BoundingBox3D>();
                    foreach (var bb in zupBoxes) {
                        if (t.Ids.Contains(bb.Id)) {
                            tileZupBoxes.Add(bb);
                        }
                    }
                    var bvol = GetBoundingvolume(tileZupBoxes);
                    t.Boundingvolume = bvol;
                }

                WiteTilesetJson(translation, tiles, o.Output, box);
                Console.WriteLine($"Writing {tiles.Count} tiles...");
                WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, tiles, o.Output, o.RoofColorColumn, o.AttributesColumn);

                conn.Close();
                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine("Program finished.");
            });
        }


        public static Boundingvolume GetBoundingvolume(List<BoundingBox3D> bbs)
        {
            var bbox = BoundingBoxCalculator.GetBoundingBox(bbs);
            var boundingVolume = new Boundingvolume {
                box = bbox.GetBox()
            };
            return boundingVolume;
        }

        private static void WriteTiles(NpgsqlConnection conn, string geometryTable, string geometryColumn, string idcolumn, double[] translation, List<Tile> tiles, string outputPath, string colorColumn = "", string attributesColumn = "")
        {
            var counter = 0;
            foreach (var t in tiles) {
                counter++;
                var perc = Math.Round(((double)counter / tiles.Count) * 100, 2);
                Console.Write($"\rProgress: tile {counter} - {perc:F}%");

                var subset = t.Ids.ToArray();
                var geometries = BoundingBoxRepository.GetGeometrySubset(conn, geometryTable, geometryColumn, idcolumn, translation, subset, colorColumn, attributesColumn);

                var triangleCollection = GetTriangles(geometries);

                var bytes = GlbCreator.GetGlb(triangleCollection);
                var b3dm = new B3dm.Tile.B3dm(bytes);
                var featureTable = new FeatureTable {
                    BATCH_LENGTH = geometries.Count
                };
                b3dm.FeatureTableJson = JsonConvert.SerializeObject(featureTable);

                if (attributesColumn != string.Empty) {
                    var batchtable = new BatchTable();
                    var allattributes = new List<object>();
                    foreach (var geom in geometries) {
                        // only take the first now....
                        allattributes.Add(geom.Attributes[0]);
                    }

                    var item = new BatchTableItem {
                        Name = attributesColumn,
                        Values = allattributes.ToArray()
                    };
                    batchtable.BatchTableItems.Add(item);
                    var json = JsonConvert.SerializeObject(batchtable, new BatchTableJsonConverter(typeof(BatchTable)));
                    b3dm.BatchTableJson = json;
                }

                B3dmWriter.WriteB3dm($"{outputPath}/tiles/{counter}.b3dm", b3dm);

            }
        }


        private static void WiteTilesetJson(double[] translation, List<Tile> tiles, string outputPath, double[] box)
        {
            var tileset = TreeSerializer.ToTileset(tiles, translation, box);
            var s = JsonConvert.SerializeObject(tileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText($"{outputPath}/tileset.json", s);
        }


        public static TriangleCollection GetTriangles(List<GeometryRecord> geomrecords)
        {
            var triangleCollection = new TriangleCollection();
            foreach (var g in geomrecords) {
                var surface = (PolyhedralSurface)g.Geometry;
                var colors = g.HexColors;
                var triangles = Triangulator.GetTriangles(surface, colors, g.BatchId);
                triangleCollection.AddRange(triangles);
            }

            return triangleCollection;
        }

    }
}
