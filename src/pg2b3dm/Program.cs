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
                var geometricErrors = Array.ConvertAll(o.GeometricErrors.Split(','), double.Parse); ;

                var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                var lods = (lodcolumn != string.Empty ? GetLods(conn, geometryTable, lodcolumn) : new List<int> { 0 });
                if(geometricErrors.Length > lods.Count + 1) {
                    Console.WriteLine("Error: parameter -g --geometric is wrongly specified: " + o.GeometricErrors);
                }
                if (lodcolumn != String.Empty){
                    Console.WriteLine($"Detected {lods.Count} lod levels: [{String.Join(',', lods)}]");

                    if (lods.Count >= geometricErrors.Length) {
                        geometricErrors = GeometricErrorCalculator.GetGeometricErrors(geometricErrors[0], lods);
                    }
                };
                Console.WriteLine("Geometric errors used: " + String.Join(',', geometricErrors));
                Console.WriteLine($"Calculating dataset translation for {geometryTable}...");
                var bbox3d = BoundingBoxRepository.GetBoundingBox3DForTable(conn, geometryTable, geometryColumn);
                var translation = bbox3d.GetCenter().ToVector();
                Console.WriteLine($"Translation: [{string.Join(',', translation) }]");
                var boundingboxAllFeatures = BoundingBoxCalculator.RotateTranslateTransform(bbox3d, translation);
                var box = boundingboxAllFeatures.GetBox();
                var sr = SpatialReferenceRepository.GetSpatialReference(conn, geometryTable, geometryColumn);
                Console.WriteLine($"Spatial reference: {sr}");
                var tiles = TileCutter.GetTiles(conn, o.ExtentTile, geometryTable, geometryColumn, bbox3d, sr, lods, geometricErrors.Skip(1).ToArray(), lodcolumn);
                Console.WriteLine();
                var nrOfTiles = RecursiveTileCounter.CountTiles(tiles, 0);
                Console.WriteLine($"Tiles with features: {nrOfTiles} ");
                CalculateBoundingBoxes(translation, tiles, boundingboxAllFeatures.ZMin, boundingboxAllFeatures.ZMax);
                Console.WriteLine("Writing tileset.json...");
                WiteTilesetJson(translation, tiles, o.Output, box, geometricErrors[0]);
                WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, tiles, sr, o.Output, 0, nrOfTiles, o.RoofColorColumn, o.AttributesColumn, o.LodColumn);

                conn.Close();
                stopWatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds / 1000} seconds");
                Console.WriteLine("Program finished.");
            });
        }



        private static void CalculateBoundingBoxes(double[] translation, List<Tile> tiles, double minZ, double maxZ)
        {
            foreach (var t in tiles) {

                var bb = t.BoundingBox;
                var bvol = new BoundingBox3D(bb.XMin, bb.YMin, minZ, bb.XMax, bb.YMax, maxZ);
                var bvolRotated = BoundingBoxCalculator.RotateTranslateTransform(bvol, translation);

                if (t.Children != null) {

                    CalculateBoundingBoxes(translation, t.Children, minZ, maxZ);
                }
                t.Boundingvolume = TileCutter.GetBoundingvolume(bvolRotated);
            }
        }


        private static List<int> GetLods(NpgsqlConnection conn, string geometryTable, string lodcolumn)
        {
            var res = new List<int>();
            var sql = $"select distinct({lodcolumn}) from {geometryTable} order by {lodcolumn}";

            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                var id = reader.GetInt32(0);
                res.Add(id);
            }

            reader.Close();
            return res;
        }


        private static int WriteTiles(NpgsqlConnection conn, string geometryTable, string geometryColumn, string idcolumn, double[] translation, List<Tile> tiles, int epsg, string outputPath, int counter, int maxcount, string colorColumn = "", string attributesColumn = "", string lodColumn="")
        {
            foreach (var t in tiles) {
                counter++;
                var perc = Math.Round(((double)counter / maxcount) * 100, 2);
                Console.Write($"\rCreating tile phase: tile {counter}/{maxcount} - {perc:F}%");

                var geometries = BoundingBoxRepository.GetGeometrySubset(conn, geometryTable, geometryColumn, idcolumn, translation, t, epsg, colorColumn, attributesColumn, lodColumn);

                var triangleCollection = GetTriangles(geometries);

                var b3dm = GetB3dm(attributesColumn, geometries, triangleCollection);

                B3dmWriter.WriteB3dm($"{outputPath}/tiles/{counter}.b3dm", b3dm);

                if (t.Children != null) {
                    counter = WriteTiles(conn, geometryTable, geometryColumn, idcolumn, translation, t.Children, epsg, outputPath, counter, maxcount, colorColumn, attributesColumn, lodColumn);
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

        private static void WiteTilesetJson(double[] translation, List<Tile> tiles, string outputPath, double[] box, double maxGeometricError)
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
