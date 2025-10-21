using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using B3dm.Tileset;
using CommandLine;
using Npgsql;
using subtree;
using B3dm.Tileset.Extensions;
using SharpGLTF.Schema2;
using B3dm.Tileset.settings;

namespace pg2b3dm;

class Program
{
    static string password = string.Empty;
    static void Main(string[] args)
    {
        var version = Assembly.GetEntryAssembly().GetName().Version;
        Console.WriteLine($"Tool: pg2b3dm {version}");
        Console.WriteLine("Options: " + string.Join(" ", args));
        Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
            o.User = string.IsNullOrEmpty(o.User) ? Environment.UserName : o.User;
            o.Database = string.IsNullOrEmpty(o.Database) ? Environment.UserName : o.Database;

            var connectionString = $"Host={o.Host};Username={o.User};Database={o.Database};Port={o.Port};CommandTimeOut=0";
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


            Console.WriteLine($"Input table: {o.GeometryTable}");
            if (o.Query != String.Empty) {
                Console.WriteLine($"Query:  {o.Query ?? "-"}");
            }
            Console.WriteLine($"Input geometry column: {o.GeometryColumn}");

            var defaultColor = o.DefaultColor;
            var defaultMetallicRoughness = o.DefaultMetallicRoughness;
            var doubleSided = (bool)o.DoubleSided;
            var defaultAlphaMode = o.DefaultAlphaMode;
            var createGltf = (bool)o.CreateGltf;
            var outputDirectory = o.Output;
            var copyright = o.Copyright;
            var tilesetVersion = o.TilesetVersion;
            var keepProjection = (bool)o.KeepProjection;
            var subdivisionScheme = o.subdivisionScheme;
            var geometricError = o.GeometricError;
            var geometricErrorFactor = o.GeometricErrorFactor;

            var inputTable = new InputTable();
            inputTable.TableName = o.GeometryTable;
            inputTable.GeometryColumn = o.GeometryColumn;
            inputTable.Query = o.Query;
            inputTable.RadiusColumn = o.RadiusColumn;
            inputTable.ShadersColumn = o.ShadersColumn;
            inputTable.AttributeColumns = o.AttributeColumns;
            inputTable.LodColumn = o.LodColumn;

            var conn = new NpgsqlConnection(connectionString);

            var source_epsg = SpatialReferenceRepository.GetSpatialReference(conn, inputTable.TableName, inputTable.GeometryColumn, inputTable.Query);
            if (source_epsg == 4978) {
                Console.WriteLine("----------------------------------------------------------------------------");
                Console.WriteLine("WARNING: Input geometries in ECEF (epsg:4978) are not supported in version >= 2.0.0");
                Console.WriteLine("Fix: Use local coordinate systems or EPSG:4326 in input datasource.");
                Console.WriteLine("----------------------------------------------------------------------------");
            }

            Console.WriteLine($"Spatial reference of {inputTable.TableName}.{inputTable.GeometryColumn}: {source_epsg}");
            inputTable.EPSGCode = source_epsg;

            // Check spatialIndex
            var hasSpatialIndex = SpatialIndexChecker.HasSpatialIndex(conn, inputTable.TableName, inputTable.GeometryColumn);
            if (!hasSpatialIndex) {
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine($"WARNING: No spatial index detected on {inputTable.TableName}.{inputTable.GeometryColumn}");
                Console.WriteLine("Fix: add a spatial index, for example: ");
                Console.WriteLine($"'CREATE INDEX ON {inputTable.TableName} USING gist(st_centroid(st_envelope({inputTable.GeometryColumn})))'");
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine();
            }
            else {
                Console.WriteLine($"Spatial index detected on {inputTable.TableName}.{inputTable.GeometryColumn}");
            }
            
            var skipCreateTiles = (bool)o.SkipCreateTiles;
            Console.WriteLine("Skip create tiles: " + skipCreateTiles);

            Console.WriteLine($"Query bounding box of {inputTable.TableName}.{inputTable.GeometryColumn}...");
            var where = (inputTable.Query != string.Empty ? $" where {inputTable.Query}" : String.Empty);

            var bbox_table = BoundingBoxRepository.GetBoundingBoxForTable(conn, inputTable.TableName, inputTable.GeometryColumn, keepProjection , where);
            var bbox = bbox_table.bbox;

            var proj = keepProjection? $"EPSG:{source_epsg}": $"EPSG:4326 (WGS84)";
            Console.WriteLine($"Bounding box for {inputTable.TableName}.{inputTable.GeometryColumn} ({proj}): " +
                $"{Math.Round(bbox.XMin, 8)}, {Math.Round(bbox.YMin, 8)}, " +
                $"{Math.Round(bbox.XMax, 8)}, {Math.Round(bbox.YMax, 8)}");

            var zmin = bbox_table.zmin;
            var zmax = bbox_table.zmax;
            var maxFeaturesPerTile = o.MaxFeaturesPerTile;

            Console.WriteLine($"Height values: [{Math.Round(zmin, 2)} m - {Math.Round(zmax, 2)} m]");
            Console.WriteLine($"Default color: {defaultColor}");
            Console.WriteLine($"Default metallic roughness: {defaultMetallicRoughness}");
            Console.WriteLine($"Doublesided: {doubleSided}");
            Console.WriteLine($"Default AlphaMode: {defaultAlphaMode}");
            Console.WriteLine($"Create glTF tiles: {createGltf}");

            var att = !string.IsNullOrEmpty(o.AttributeColumns) ? o.AttributeColumns : "-";
            Console.WriteLine($"Attribute columns: {att}");

            var center = bbox.GetCenter();
            Console.WriteLine($"Center ({proj}): {center.X}, {center.Y}");

            Tiles3DExtensions.RegisterExtensions();

            var translation =  keepProjection?
                new double[] { (double)center.X, (double)center.Y, 0 } :
                Translation.ToEcef(center);
            Console.WriteLine($"Translation: {String.Join(',', translation)}");

            var lodcolumn = o.LodColumn;
            var addOutlines = (bool)o.AddOutlines;
            var useImplicitTiling = (bool)o.UseImplicitTiling;
            var refinement = o.Refinement;
            if (useImplicitTiling) {
                if (!String.IsNullOrEmpty(lodcolumn)) {
                    Console.WriteLine("Warning: parameter -l --lodcolumn is ignored with implicit tiling");
                    lodcolumn = String.Empty;
                }
            }
            // if useImpliciting is false and createGlb is false, the set use10 to true
            var use10 = !useImplicitTiling && !createGltf;
            Console.WriteLine("3D Tiles version: " + (use10 ? "1.0" : "1.1"));
            Console.WriteLine($"Lod column: {lodcolumn}");
            Console.WriteLine($"Radius column: {o.RadiusColumn}");
            Console.WriteLine($"Geometric error: {o.GeometricError}");
            Console.WriteLine($"Geometric error factor: {o.GeometricErrorFactor}");
            Console.WriteLine($"Refinement: {refinement}");
            Console.WriteLine($"Keep projection: {keepProjection}");
            Console.WriteLine($"Subdivision scheme: {subdivisionScheme}");

            if(keepProjection && !useImplicitTiling) {
                Console.WriteLine("Warning: keepProjection is only supported with implicit tiling.");
                Console.WriteLine("Program will exit now.");
                return;
            }

            var lods = (lodcolumn != string.Empty ? LodsRepository.GetLods(conn, inputTable.TableName, lodcolumn, inputTable.Query) : new List<int> { 0 });
            if (lodcolumn != String.Empty) {
                Console.WriteLine($"Lod levels: {String.Join(',', lods)}");
            };

            Console.WriteLine($"Add outlines: {addOutlines}");
            Console.WriteLine($"Use 3D Tiles 1.1 implicit tiling: {o.UseImplicitTiling}");
            if(!tilesetVersion.Equals(string.Empty)) {
                Console.WriteLine($"Tileset version: {tilesetVersion}");
            }

            var rootBoundingVolumeRegion = 
                keepProjection?
                    bbox.ToRegion(zmin, zmax) : 
                    bbox.ToRadians().ToRegion(zmin, zmax);

            Console.WriteLine($"Maximum features per tile: " + maxFeaturesPerTile);

            var crs = keepProjection ? $"EPSG:{source_epsg}" : "";
            Console.WriteLine($"Start generating tiles...");

            var stylingSettings = new StylingSettings() {
                DefaultColor = defaultColor,
                DefaultMetallicRoughness = defaultMetallicRoughness,
                DefaultAlphaMode = defaultAlphaMode,
                DoubleSided = doubleSided,
                AddOutlines = addOutlines,
            };

            var outputFolder = o.Output;
            if (!Directory.Exists(outputFolder)) {
                Directory.CreateDirectory(outputFolder);
            }

            var subtreesDirectory = $"{outputFolder}{Path.AltDirectorySeparatorChar}subtrees";
            if (!Directory.Exists(subtreesDirectory)) {
                Directory.CreateDirectory(subtreesDirectory);
            }

            var contentDirectory = $"{outputFolder}{Path.AltDirectorySeparatorChar}content";

            if (!Directory.Exists(contentDirectory)) {
                Directory.CreateDirectory(contentDirectory);
            }

            var outputSettings = new OutputSettings() {
                OutputFolder = outputDirectory,
                ContentFolder = contentDirectory,
                SubtreesFolder = subtreesDirectory,
            };
            var tilesetSettings = new TilesetSettings();
            tilesetSettings.OutputSettings = outputSettings;
            tilesetSettings.Version = version;
            tilesetSettings.Copyright = copyright;
            tilesetSettings.TilesetVersion = tilesetVersion;
            tilesetSettings.SubdivisionScheme = subdivisionScheme;
            tilesetSettings.GeometricError = geometricError;
            tilesetSettings.GeometricErrorFactor = geometricErrorFactor;
            tilesetSettings.Translation = translation;
            tilesetSettings.Refinement = refinement;
            tilesetSettings.RootBoundingVolumeRegion = rootBoundingVolumeRegion;

            if (subdivisionScheme == SubdivisionScheme.QUADTREE) {
                QuadtreeTile(inputTable, stylingSettings, tilesetSettings, createGltf, 
                    keepProjection,conn, skipCreateTiles, bbox, maxFeaturesPerTile, 
                    useImplicitTiling, use10, 
                    lods, crs);
            }

            Console.WriteLine();

            stopWatch.Stop();

            var timeSpan = stopWatch.Elapsed;
            Console.WriteLine("Time: {0}h {1}m {2}s {3}ms", Math.Floor(timeSpan.TotalHours), timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            Console.WriteLine($"Program finished {DateTime.Now.ToLocalTime().ToString("s")}.");
        });
    }

    private static void QuadtreeTile(InputTable inputTable, StylingSettings stylingSettings, TilesetSettings tilesetSettings, bool createGltf, bool keepProjection, NpgsqlConnection conn, bool skipCreateTiles, Wkx.BoundingBox bbox, int maxFeaturesPerTile, bool useImplicitTiling, bool use10, List<int> lods, string crs)
    {
        var tile = new Tile(0, 0, 0);
        tile.BoundingBox = bbox.ToArray();
        var outputSettings = tilesetSettings.OutputSettings;

        var quadtreeTiler = new QuadtreeTiler(conn, inputTable, stylingSettings, maxFeaturesPerTile, tilesetSettings.Translation, outputSettings.ContentFolder, lods, tilesetSettings.Copyright, skipCreateTiles);
        var tiles = quadtreeTiler.GenerateTiles(bbox, tile, new List<Tile>(), inputTable.LodColumn != string.Empty ? lods.First() : 0, createGltf, keepProjection);
        Console.WriteLine();
        Console.WriteLine("Tiles created: " + tiles.Count(tile => tile.Available));

        if (tiles.Count(tile => tile.Available) > 0) {
            if (useImplicitTiling) {
                CesiumTiler.CreateImplicitTileset(tilesetSettings.Version, createGltf, outputSettings.OutputFolder, tilesetSettings.Translation, 
                    tilesetSettings.GeometricError, tilesetSettings.RootBoundingVolumeRegion, outputSettings.SubtreesFolder, tiles, tilesetSettings.TilesetVersion, crs, keepProjection, tilesetSettings.SubdivisionScheme, tilesetSettings.Refinement);
            }
            else {
                CesiumTiler.CreateExplicitTilesetsJson(tilesetSettings.Version, outputSettings.OutputFolder, tilesetSettings.Translation, 
                    tilesetSettings.GeometricError, tilesetSettings.GeometricErrorFactor, 
                    tilesetSettings.Refinement, use10, tilesetSettings.RootBoundingVolumeRegion, 
                    tile, tiles, tilesetSettings.TilesetVersion, crs);
            }
        }
    }
}
