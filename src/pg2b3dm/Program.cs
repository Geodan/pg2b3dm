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
                var outputTiles = $"{output}{Path.DirectorySeparatorChar}tiles";
                if (!Directory.Exists(output)) {
                    Directory.CreateDirectory(output);
                }
                if (!Directory.Exists(outputTiles)) {
                    Directory.CreateDirectory(outputTiles);
                }

                Console.WriteLine($"Input table:  {o.GeometryTable}");
                Console.WriteLine($"Output directory:  {outputTiles}");

                var geometryTable = o.GeometryTable;
                var geometryColumn = o.GeometryColumn;
                var idcolumn = o.IdColumn;
                var lodcolumn = o.LodColumn;
                var maxGeometricError = o.GeometricError;

                var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                var lods = (lodcolumn != string.Empty ? GetLods(conn, geometryTable, lodcolumn) : new List<int> { 0 });

                if (lodcolumn != String.Empty){
                    Console.WriteLine($"Detected {lods.Count} lod levels: [{String.Join(',', lods)}]");
                };

                Console.WriteLine($"Calculating dataset translation for {geometryTable}...");
                var bbox3d = BoundingBoxRepository.GetBoundingBox3D(conn, geometryTable, geometryColumn);
                var translation = bbox3d.GetCenter().ToVector();
                Console.WriteLine($"Translation: [{string.Join(',', translation) }]");
                var boundingAllActualNew = BoundingBoxCalculator.GetBoundingAll(bbox3d, translation);
                var box = boundingAllActualNew.GetBox();
                var sr = SpatialReferenceRepository.GetSpatialReference(conn, geometryTable, geometryColumn);
                Console.WriteLine($"Spatial reference: {sr}");
                Console.WriteLine("Calculating features per tile...");
                var geometricErrors = GeometricErrorCalculator.GetGeometricErrors(maxGeometricError, lods);
                var tiles = TileCutter.GetTiles(conn, o.ExtentTile, geometryTable, geometryColumn, idcolumn, bbox3d, sr, lods, geometricErrors, lodcolumn);
                var nrOfTiles = RecursiveTileCounter.CountTiles(tiles, 0);
                Console.WriteLine($"Number of tiles: {nrOfTiles} ");
                Console.WriteLine("Calculating boundingbox per tile...");
                CalculateBoundingBoxes(geometryTable, geometryColumn, idcolumn, conn, translation, tiles, 0, nrOfTiles);
                Console.WriteLine();
                Console.WriteLine("Writing tileset.json...");
                WiteTilesetJson(translation, tiles, o.Output, box, maxGeometricError);
                Console.WriteLine($"Writing {nrOfTiles} tiles...");
                WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, tiles, o.Output, 0, nrOfTiles, o.RoofColorColumn, o.AttributesColumn);

                conn.Close();
                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine("Program finished.");
            });
        }

        private static int CalculateBoundingBoxes(string geometryTable, string geometryColumn, string idcolumn, NpgsqlConnection conn, double[] translation, List<Tile> tiles, int counter, int totalcount)
        {
            foreach (var t in tiles) {
                counter++;

                var perc = Math.Round(((double)counter / totalcount) * 100, 2);
                Console.Write($"\rProgress: tile {counter} - {perc:F}%");

                var bvol = TileCutter.GetTileBoundingBoxNew(conn, geometryTable, geometryColumn, idcolumn, translation, t.Ids.ToArray());

                if (t.Child != null) {

                    counter= CalculateBoundingBoxes(geometryTable, geometryColumn, idcolumn, conn, translation, new List<Tile> { t.Child }, counter, totalcount);
                }
                t.Boundingvolume = bvol;
            }
            return counter;
        }

        private static List<int> GetLods(NpgsqlConnection conn, string geometryTable, string lodcolumn)
        {
            var res = new List<int>();
            var sql = $"select distinct({lodcolumn}) from {geometryTable}";

            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                var id = reader.GetInt32(0);
                res.Add(id);
            }

            reader.Close();
            return res;
        }



        public static Boundingvolume GetBoundingvolume(List<BoundingBox3D> bbs)
        {
            var bbox = BoundingBoxCalculator.GetBoundingBox(bbs);
            var boundingVolume = new Boundingvolume {
                box = bbox.GetBox()
            };
            return boundingVolume;
        }

        private static int WriteTiles(NpgsqlConnection conn, string geometryTable, string geometryColumn, string idcolumn, double[] translation, List<Tile> tiles, string outputPath, int counter, int maxcount, string colorColumn = "", string attributesColumn = "")
        {
            foreach (var t in tiles) {
                counter++;
                var perc = Math.Round(((double)counter / maxcount) * 100, 2);
                Console.Write($"\rProgress: tile {counter} - {perc:F}%");

                var subset = t.Ids.ToArray();
                var geometries = BoundingBoxRepository.GetGeometrySubset(conn, geometryTable, geometryColumn, idcolumn, translation, subset, colorColumn, attributesColumn);

                var triangleCollection = GetTriangles(geometries);

                var b3dm = GetB3dm(attributesColumn, geometries, triangleCollection);

                B3dmWriter.WriteB3dm($"{outputPath}/tiles/{counter}.b3dm", b3dm);

                if (t.Child != null) {
                    counter = WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, new List<Tile> { t.Child }, outputPath, counter, maxcount, colorColumn, attributesColumn);
                }

            }
            return counter;
        }

        private static B3dm.Tile.B3dm GetB3dm(string attributesColumn, List<GeometryRecord> geometries, TriangleCollection triangleCollection)
        {
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

            return b3dm;
        }

        private static void WiteTilesetJson(double[] translation, List<Tile> tiles, string outputPath, double[] box, int maxGeometricError)
        {
            var tileset = TreeSerializer.ToTileset(tiles, translation, box, maxGeometricError);
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
