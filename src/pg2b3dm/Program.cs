using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using B3dm.Tileset;
using CommandLine;
using Npgsql;
using subtree;
using B3dm.Tileset.Extensions;
using SharpGLTF.Schema2;
using B3dm.Tileset.settings;
using System.IO;

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

            // Octree checks 
            if(o.subdivisionScheme == SubdivisionScheme.OCTREE) {
                if (o.UseImplicitTiling == false) {
                    Console.WriteLine("Warning: Octree subdivision scheme is only supported with implicit tiling.");
                    Console.WriteLine("Program will exit now.");
                    return;
                }
                if(o.LodColumn != String.Empty) {
                    Console.WriteLine("Warning: parameter -l --lodcolumn is ignored with octree subdivision scheme");
                    o.LodColumn = String.Empty;
                }
            }

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

            var bbox_table = BoundingBoxRepository.GetBoundingBoxForTable(conn, inputTable.TableName, inputTable.GeometryColumn, keepProjection, where);
            var bbox = bbox_table.bbox;
            var zmin = bbox_table.zmin;
            var zmax = bbox_table.zmax;

            var proj = keepProjection ? $"EPSG:{source_epsg}" : $"EPSG:4326 (WGS84)";
            Console.WriteLine($"Bounding box for {inputTable.TableName}.{inputTable.GeometryColumn} ({proj}): " +
                $"{Math.Round(bbox.XMin, 8)}, {Math.Round(bbox.YMin, 8)}, " +
                $"{Math.Round(bbox.XMax, 8)}, {Math.Round(bbox.YMax, 8)}");

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

            var translation = keepProjection ?
                [(double)center.X, (double)center.Y, 0] :
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
            Console.WriteLine($"Lod column: {lodcolumn}");
            Console.WriteLine($"Radius column: {o.RadiusColumn}");
            Console.WriteLine($"Geometric error: {o.GeometricError}");
            Console.WriteLine($"Geometric error factor: {o.GeometricErrorFactor}");
            Console.WriteLine($"Refinement: {refinement}");
            Console.WriteLine($"Keep projection: {keepProjection}");
            Console.WriteLine($"Subdivision scheme: {subdivisionScheme}");

            if (keepProjection && !useImplicitTiling) {
                Console.WriteLine("Warning: keepProjection is only supported with implicit tiling.");
                Console.WriteLine("Program will exit now.");
                return;
            }

            var lods = (lodcolumn != string.Empty ? LodsRepository.GetLods(conn, inputTable.TableName, lodcolumn, inputTable.Query) : new List<int> { 0 });
            if (lodcolumn != String.Empty) {
                Console.WriteLine($"Lod levels: {String.Join(',', lods)}");
            }
            ;

            Console.WriteLine($"Add outlines: {addOutlines}");
            Console.WriteLine($"Use 3D Tiles 1.1 implicit tiling: {o.UseImplicitTiling}");
            if (!tilesetVersion.Equals(string.Empty)) {
                Console.WriteLine($"Tileset version: {tilesetVersion}");
            }

            var rootBoundingVolumeRegion =
                keepProjection ?
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
            var outputSettings = OutputDirectoryCreator.GetFolders(outputFolder);
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
            tilesetSettings.Crs = crs;


            var tilingSettings = new TilingSettings();
            tilingSettings.BoundingBox = bbox;
            tilingSettings.CreateGltf = createGltf;
            tilingSettings.KeepProjection = keepProjection;
            tilingSettings.SkipCreateTiles = skipCreateTiles;
            tilingSettings.MaxFeaturesPerTile = maxFeaturesPerTile;
            tilingSettings.Lods = lods;

            if (subdivisionScheme == SubdivisionScheme.QUADTREE) {
                QuadtreeTile(conn, inputTable, stylingSettings, tilesetSettings, tilingSettings);
            }
            else {
                var boundingBox3D = new BoundingBox3D() { XMin = bbox.XMin, YMin = bbox.YMin, ZMin = zmin, XMax = bbox.XMax, YMax = bbox.YMax, ZMax = zmax };
                OctreeTile(conn, boundingBox3D, inputTable, stylingSettings, tilesetSettings, tilingSettings);
            }

            Console.WriteLine();

            stopWatch.Stop();

            var timeSpan = stopWatch.Elapsed;
            Console.WriteLine("Time: {0}h {1}m {2}s {3}ms", Math.Floor(timeSpan.TotalHours), timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            Console.WriteLine($"Program finished {DateTime.Now.ToLocalTime().ToString("s")}.");
        });
    }

    private static void OctreeTile(NpgsqlConnection conn, BoundingBox3D bbox3D, InputTable inputTable, StylingSettings stylingSettings, TilesetSettings tilesetSettings, TilingSettings tilingSettings)
    {
        var rootTile3D = new Tile3D(0, 0, 0, 0);

        var octreeTiler = new OctreeTiler(conn, inputTable, tilingSettings, stylingSettings, tilesetSettings);
        var tiles3D = octreeTiler.GenerateTiles3D(bbox3D, 0, rootTile3D, new List<Tile3D>());
        var mortonIndices = MortonIndex.GetMortonIndices3D(tiles3D);
        var subtreebytes = SubtreeWriter.ToBytes(mortonIndices.tileAvailability, mortonIndices.contentAvailability);

        // todo: Write more subtree files
        File.WriteAllBytes($"{tilesetSettings.OutputSettings.SubtreesFolder}/0_0_0_0.subtree", subtreebytes);
        var maxAvailableLevel = tiles3D.Max(p => p.Level);

        tilesetSettings.SubtreeLevels = maxAvailableLevel + 1;

        // todo add explicit tileset option
        CesiumTiler.CreateImplicitTileset(tilesetSettings, tilingSettings.CreateGltf, tilingSettings.KeepProjection);
    }

    private static void QuadtreeTile(NpgsqlConnection conn, InputTable inputTable, StylingSettings stylingSettings, TilesetSettings tilesetSettings, TilingSettings tilingSettings)
    {
        var tile = new Tile(0, 0, 0);
        var bbox = tilingSettings.BoundingBox;
        tile.BoundingBox = bbox.ToArray();
        var outputSettings = tilesetSettings.OutputSettings;

        var quadtreeTiler = new QuadtreeTiler(conn, inputTable, stylingSettings, tilingSettings.MaxFeaturesPerTile, tilesetSettings.Translation, outputSettings.ContentFolder, tilingSettings.Lods, tilesetSettings.Copyright, tilingSettings.SkipCreateTiles);
        var tiles = quadtreeTiler.GenerateTiles(bbox, tile, new List<Tile>(), inputTable.LodColumn != string.Empty ? tilingSettings.Lods.First() : 0, tilingSettings.CreateGltf, tilingSettings.KeepProjection);
        Console.WriteLine();
        Console.WriteLine("Tiles created: " + tiles.Count(tile => tile.Available));

        if (tiles.Count(tile => tile.Available) > 0) {
            if (tilingSettings.UseImplicitTiling) {
                var subtreeLevels = CesiumTiler.CreateSubtreeFiles(outputSettings, tiles);
                tilesetSettings.SubtreeLevels = subtreeLevels;
                CesiumTiler.CreateImplicitTileset(tilesetSettings, tilingSettings.CreateGltf, tilingSettings.KeepProjection);
            }
            else {
                CesiumTiler.CreateExplicitTilesetsJson(tilesetSettings.Version, outputSettings.OutputFolder, tilesetSettings.Translation, 
                    tilesetSettings.GeometricError, tilesetSettings.GeometricErrorFactor, 
                    tilesetSettings.Refinement, tilesetSettings.RootBoundingVolumeRegion, 
                    tile, tiles, tilesetSettings.TilesetVersion, tilesetSettings.Crs);
            }
        }
    }
}
