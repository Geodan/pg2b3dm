using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using B3dm.Tileset.settings;
using Npgsql;
using subtree;
using Wkx;

namespace pg2b3dm;

public class QuadtreeTiler
{
    private readonly NpgsqlConnection conn;
    private readonly int source_epsg;
    private readonly int maxFeaturesPerTile;
    private readonly double[] translation;
    private readonly string outputFolder;
    private readonly List<int> lods;
    private readonly string copyright;
    private readonly bool skipCreateTiles;
    private readonly StylingSettings stylingSettings;
    private InputTable inputTable;
    private readonly bool useImplicitTiling;

    public QuadtreeTiler(string connectionString, InputTable inputTable, StylingSettings stylingSettings, int maxFeaturesPerTile, double[] translation, string outputFolder, List<int> lods, string copyright = "", bool skipCreateTiles = false, bool useImplicitTiling = false)
    {
        this.conn = new NpgsqlConnection(connectionString);
        this.inputTable = inputTable;
        this.source_epsg = inputTable.EPSGCode;
        this.maxFeaturesPerTile = maxFeaturesPerTile;
        this.translation = translation;
        this.outputFolder = outputFolder;
        this.lods = lods;
        this.copyright = copyright;
        this.skipCreateTiles = skipCreateTiles;
        this.stylingSettings = stylingSettings;
        this.useImplicitTiling = useImplicitTiling;
    }

    public List<Tile> GenerateTiles(BoundingBox bbox, Tile tile, List<Tile> tiles, int lod = 0, bool createGltf = false, bool keepProjection = false, HashSet<string> processedGeometries = null)
    {
        if (processedGeometries == null) {
            processedGeometries = new HashSet<string>();
        }

        var where = inputTable.GetQueryClause();

        var lodquery = LodQuery.GetLodQuery(inputTable.LodColumn, lod);

        if (inputTable.LodColumn != String.Empty) {
            where += $" and {lodquery}";
        }

        var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, inputTable.TableName, inputTable.GeometryColumn, new Point(bbox.XMin, bbox.YMin), new Point(bbox.XMax, bbox.YMax), where, source_epsg, keepProjection, processedGeometries);

        if (numberOfFeatures == 0) {
            tile.Available = false;
            tiles.Add(tile);
        }
        else if (numberOfFeatures > maxFeaturesPerTile) {
            // First, get the largest geometries up to maxFeaturesPerTile for this level
            var localProcessedGeometries = CreateTileForLargestGeometries(bbox, tile, tiles, where, lod, createGltf, keepProjection, processedGeometries);

            var z = tile.Z + 1;

            // split in quadtree
            for (var x = 0; x < 2; x++) {
                for (var y = 0; y < 2; y++) {
                    var dx = (bbox.XMax - bbox.XMin) / 2;
                    var dy = (bbox.YMax - bbox.YMin) / 2;

                    var xstart = bbox.XMin + dx * x;
                    var ystart = bbox.YMin + dy * y;
                    var xend = xstart + dx;
                    var yend = ystart + dy;

                    var bboxQuad = new BoundingBox(xstart, ystart, xend, yend);
                    var new_tile = new Tile(z, tile.X * 2 + x, tile.Y * 2 + y);
                    new_tile.BoundingBox = bboxQuad.ToArray();

                    var filteredProcessedGeometries = GeometryRepository.FilterHashesByEnvelope(conn, inputTable.TableName, inputTable.GeometryColumn, bboxQuad, source_epsg, localProcessedGeometries, keepProjection);

                    GenerateTiles(bboxQuad, new_tile, tiles, lod, createGltf, keepProjection, filteredProcessedGeometries);
                }
            }
        }
        else {
            CreateTile(bbox, tile, tiles, where, lod, createGltf, keepProjection, processedGeometries);
        }

        return tiles;
    }

    private HashSet<string> CreateTileForLargestGeometries(BoundingBox bbox, Tile tile, List<Tile> tiles, string where, int lod, bool createGltf, bool keepProjection, HashSet<string> processedGeometries)
    {
        // clone processedIds to avoid modifying the original set in recursive calls
        var localProcessedGeometries = new HashSet<string>(processedGeometries);
        var tileHashes = new HashSet<string>();

        // Get the largest geometries (up to maxFeaturesPerTile) for this tile at this level
        tile.Available = false;
        tile.BoundingBox = bbox.ToArray();
        
        int target_srs = keepProjection ? source_epsg : 4978;

        var geometriesToProcess = GeometryRepository.GetGeometrySubset(conn, inputTable.TableName, inputTable.GeometryColumn, tile.BoundingBox, source_epsg, target_srs, inputTable.ShadersColumn, inputTable.AttributeColumns, where, inputTable.RadiusColumn, keepProjection, processedGeometries, maxFeaturesPerTile);

        if (geometriesToProcess.Count > 0) {
            
            // Collect hashes of processed geometries
            foreach (var geom in geometriesToProcess.Where(geom => !string.IsNullOrEmpty(geom.Hash))) {
                localProcessedGeometries.Add(geom.Hash);
                tileHashes.Add(geom.Hash);
            }

            var file = $"{tile.Z}_{tile.X}_{tile.Y}";
            if (inputTable.LodColumn != String.Empty) {
                file += $"_{lod}";
            }

            var ext = createGltf ? ".glb" : ".b3dm";
            file += ext;
            Console.Write($"\rCreating tile: {file}  ");
            tile.ContentUri = file;

            tile.Lod = lod;

            var outputPath = $"{outputFolder}{Path.AltDirectorySeparatorChar}{file}";
            TileCreationHelper.WriteTileIfNeeded(geometriesToProcess, translation, stylingSettings, copyright, createGltf, skipCreateTiles, outputPath, file);

            ProcessLodLevels(bbox, tile, lod, createGltf, keepProjection, localProcessedGeometries);
            if (!useImplicitTiling) {
                UpdateTileBoundingBox(tile, tileHashes, where, keepProjection);
            }

            tile.Available = true;
        }
        
        tiles.Add(tile);
        return localProcessedGeometries;
    }

    private void CreateTile(BoundingBox bbox, Tile tile, List<Tile> tiles, string where, int lod, bool createGltf, bool keepProjection, HashSet<string> processedGeometries)
    {
        tile.BoundingBox = bbox.ToArray();
        var tileHashes = new HashSet<string>();

        var file = $"{tile.Z}_{tile.X}_{tile.Y}";
        if (inputTable.LodColumn != String.Empty) {
            file += $"_{lod}";
        }

        var ext = createGltf ? ".glb" : ".b3dm";
        file += ext;
        Console.Write($"\rCreating tile: {file}  ");
        tile.ContentUri = file;

        int target_srs = keepProjection ? source_epsg : 4978;

        var geometries = GeometryRepository.GetGeometrySubset(conn, inputTable.TableName, inputTable.GeometryColumn, tile.BoundingBox, source_epsg, target_srs, inputTable.ShadersColumn, inputTable.AttributeColumns, where, inputTable.RadiusColumn, keepProjection, processedGeometries);
        
        if (geometries.Count > 0) {
            // Collect hashes of processed geometries
            foreach (var geom in geometries.Where(g => !string.IsNullOrEmpty(g.Hash))) {
                tileHashes.Add(geom.Hash);
                processedGeometries.Add(geom.Hash);
            }

            tile.Lod = lod;

            var outputPath = $"{outputFolder}{Path.AltDirectorySeparatorChar}{file}";
            TileCreationHelper.WriteTileIfNeeded(geometries, translation, stylingSettings, copyright, createGltf, skipCreateTiles, outputPath, file);

            ProcessLodLevels(bbox, tile, lod, createGltf, keepProjection, processedGeometries);
            if (!useImplicitTiling) {
                UpdateTileBoundingBox(tile, tileHashes, where, keepProjection);
            }

            tile.Available = true;
        }
        else {
            tile.Available = false;
        }
        tiles.Add(tile);
    }

    private void ProcessLodLevels(BoundingBox bbox, Tile tile, int lod, bool createGltf, bool keepProjection, HashSet<string> processedGeometries)
    {
        if (inputTable.LodColumn != String.Empty && lod < lods.Max()) {
            // take the next lod
            var currentIndex = lods.FindIndex(p => p == lod);
            var nextIndex = currentIndex + 1;
            var nextLod = lods[nextIndex];
            // make a copy of the tile 
            var t2 = new Tile(tile.Z, tile.X, tile.Y);
            t2.BoundingBox = tile.BoundingBox;
            var lodNextTiles = GenerateTiles(bbox, t2, new List<Tile>(), nextLod, createGltf, keepProjection, processedGeometries);
            tile.Children = lodNextTiles;
        }
    }

    private void UpdateTileBoundingBox(Tile tile, HashSet<string> tileHashes, string where, bool keepProjection)
    {
        // next code is used to fix geometries that have centroid in the tile, but some parts outside...
        var bbox_geometries = GeometryRepository.GetGeometriesBoundingBox(conn, inputTable.TableName, inputTable.GeometryColumn, source_epsg, tile, tileHashes, where, keepProjection);
        var bbox_tile = new double[] { bbox_geometries[0], bbox_geometries[1], bbox_geometries[2], bbox_geometries[3] };
        tile.BoundingBox = bbox_tile;
        tile.ZMin = bbox_geometries[4];
        tile.ZMax = bbox_geometries[5];
    }
}
