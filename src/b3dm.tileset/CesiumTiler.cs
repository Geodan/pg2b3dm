using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B3dm.Tileset.Extensions;
using B3dm.Tileset.settings;
using Newtonsoft.Json;
using subtree;
using Wkx;

namespace B3dm.Tileset;
public static class CesiumTiler
{
    public static int CreateSubtreeFiles3D(OutputSettings outputSettings, List<Tile3D> tiles)
    {
        var subtreeFiles = SubtreeCreator3D.GenerateSubtreefiles(tiles);
        Console.WriteLine($"Writing {subtreeFiles.Count} subtree files...");
        foreach (var s in subtreeFiles) {
            var t = s.Key;

            var subtreefile = $"{outputSettings.SubtreesFolder}{Path.AltDirectorySeparatorChar}{t.Level}_{t.Z}_{t.X}_{t.Y}.subtree";
            File.WriteAllBytes(subtreefile, s.Value);
        }
        var subtreeLevels = subtreeFiles.Count > 1 ? subtreeFiles.ElementAt(1).Key.Level : 2;
        return subtreeLevels;
    }


    public static int CreateSubtreeFiles(OutputSettings outputSettings, List<Tile> tiles)
    {
        var subtreeFiles = SubtreeCreator.GenerateSubtreefiles(tiles);
        Console.WriteLine($"Writing {subtreeFiles.Count} subtree files...");
        foreach (var s in subtreeFiles) {
            var t = s.Key;
            var subtreefile = $"{outputSettings.SubtreesFolder}{Path.AltDirectorySeparatorChar}{t.Z}_{t.X}_{t.Y}.subtree";
            File.WriteAllBytes(subtreefile, s.Value);
        }
        var subtreeLevels = subtreeFiles.Count > 1 ? subtreeFiles.ElementAt(1).Key.Z : 2;
        return subtreeLevels;
    }

    public static void CreateImplicitTileset(TilesetSettings tilesetSettings, bool createGltf, bool keepProjection)
    {
        var tilesetjson = TreeSerializer.ToImplicitTileset(tilesetSettings.Translation, tilesetSettings.RootBoundingVolumeRegion, tilesetSettings.GeometricError, tilesetSettings.SubtreeLevels, tilesetSettings.Version, createGltf, tilesetSettings.TilesetVersion, tilesetSettings.Crs, keepProjection, tilesetSettings.SubdivisionScheme, tilesetSettings.Refinement);
        var file = $"{tilesetSettings.OutputSettings.OutputFolder}{Path.AltDirectorySeparatorChar}tileset.json";
        var json = JsonConvert.SerializeObject(tilesetjson, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        Console.WriteLine($"Writing {file}...");
        File.WriteAllText(file, json);
    }

    public static void CreateExplicitTilesetsJson(Version version, string outputDirectory, double[] translation, double geometricError, double geometricErrorFactor, RefinementType refinement, double[] rootBoundingVolumeRegion, Tile tile, List<Tile> tiles, string tilesetVersion="", string crs="")
    {
        var splitLevel = (int)Math.Ceiling((tiles.Max((Tile s) => s.Z) + 1.0) / 2.0);

        var rootTiles = TileSelector.Select(tiles, tile, 0, splitLevel - 1);
        var rootTileset = TreeSerializer.ToTileset(rootTiles, translation, rootBoundingVolumeRegion, geometricError, geometricErrorFactor, version, refinement, tilesetVersion, crs);

        var maxlevel = tiles.Max((Tile s) => s.Z);

        var externalTilesets = 0;
        if (maxlevel >= splitLevel) {
            // now create the tileset.json files on splitLevel

            var width = Math.Pow(2, splitLevel);
            var height = Math.Pow(2, splitLevel);
            Console.WriteLine($"Writing tileset.json files...");

            for (var i = 0; i < width; i++) {
                for (var j = 0; j < height; j++) {
                    var splitLevelTile = new Tile(splitLevel, i, j);
                    var children = TileSelector.Select(tiles, splitLevelTile, splitLevel, maxlevel);
                    if (children.Count > 0) {
                        var zminmax = children.Select(t => new double[] { (double)t.ZMin, (double)t.ZMax }).SelectMany(t => t).ToArray();
                        var childrenBoundingVolumeRegion = GetBoundingBox(children).ToRadians().ToRegion(zminmax[0], zminmax[1]);
                        /// translation is the same as identity matrix in case of child tileset
                        var tileset = TreeSerializer.ToTileset(children, null, childrenBoundingVolumeRegion, geometricError, geometricErrorFactor, version, refinement, tilesetVersion);
                       
                        var childGeometricError = GeometricErrorCalculator.GetGeometricError(geometricError, geometricErrorFactor, splitLevel);
                        tileset.geometricError = childGeometricError;
                        tileset.root.geometricError = GeometricErrorCalculator.GetGeometricError(childGeometricError,geometricErrorFactor,1);
                        var detailedJson = JsonConvert.SerializeObject(tileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                        var filename = $"tileset_{splitLevel}_{i}_{j}.json";
                        Console.Write($"\rWriting {filename}...");

                        File.WriteAllText($"{outputDirectory}{Path.AltDirectorySeparatorChar}{filename}", detailedJson);
                        externalTilesets++;
                        // add the child tilesets to the root tileset
                        var child = new Child();
                        child.boundingVolume = new Boundingvolume() { region = childrenBoundingVolumeRegion };
                        child.refine = refinement;
                        child.geometricError = tileset.root.geometricError;
                        
                        child.content = new Content() { uri = filename };
                        rootTileset.root.children.Add(child);
                    }
                }
            }
        }
        // write the root tileset
        Console.WriteLine();
        Console.WriteLine($"External tileset.json files: {externalTilesets}");
        Console.WriteLine("Writing root tileset.json...");
        var rootJson = JsonConvert.SerializeObject(rootTileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        File.WriteAllText($"{outputDirectory}{Path.AltDirectorySeparatorChar}tileset.json", rootJson);
    }

    private static BoundingBox GetBoundingBox(List<Tile> children)
    {
        var minx = children.Min(t => t.BoundingBox[0]);
        var maxx = children.Max(t => t.BoundingBox[2]);
        var miny = children.Min(t => t.BoundingBox[1]);
        var maxy = children.Max(t => t.BoundingBox[3]);
        return new BoundingBox(minx, miny, maxx, maxy);
    }

    private static BoundingBox3D GetBoundingBox3D(List<Tile3D> children, Dictionary<string, BoundingBox3D> tileBounds)
    {
        var keys = children.Select(t => $"{t.Level}_{t.Z}_{t.X}_{t.Y}").ToList();
        var boxes = keys.Select(k => tileBounds[k]).ToList();
        
        var minx = boxes.Min(b => b.XMin);
        var maxx = boxes.Max(b => b.XMax);
        var miny = boxes.Min(b => b.YMin);
        var maxy = boxes.Max(b => b.YMax);
        var minz = boxes.Min(b => b.ZMin);
        var maxz = boxes.Max(b => b.ZMax);
        
        return new BoundingBox3D(minx, miny, minz, maxx, maxy, maxz);
    }

    public static List<Tile3D> GetChildren(List<Tile3D> tiles, Tile3D tile)
    {
        var res = new List<Tile3D>();
        var root = tiles.Where(x => x.Equals(tile) && x.Available).FirstOrDefault();
        if (root != null) {
            res.Add(root);
        }
        var children = tiles.Where(x => tile.HasChild(x) && x.Available);
        res.AddRange(children);

        return res;
    }

    public static void CreateExplicitTilesetsJson3D(Version version, string outputDirectory, double[] translation, double geometricError, double geometricErrorFactor, RefinementType refinement, double[] rootBoundingVolumeRegion, Tile3D tile, List<Tile3D> tiles, Dictionary<string, BoundingBox3D> tileBounds, bool createGltf = false, string tilesetVersion = "", string crs = "")
    {
        var splitLevel = (int)Math.Ceiling((tiles.Max((Tile3D s) => s.Level) + 1.0) / 2.0);

        var rootTiles = TileSelector3D.Select(tiles, tile, 0, splitLevel - 1);
        var rootTileset = TreeSerializer.ToTileset3D(rootTiles, tileBounds, translation, rootBoundingVolumeRegion, geometricError, geometricErrorFactor, version, refinement, createGltf, tilesetVersion, crs);

        var maxlevel = tiles.Max((Tile3D s) => s.Level);

        var externalTilesets = 0;
        if (maxlevel >= splitLevel) {
            // now create the tileset.json files on splitLevel

            var dimension = Math.Pow(2, splitLevel);
            Console.WriteLine($"Writing tileset.json files...");

            for (var x = 0; x < dimension; x++) {
                for (var y = 0; y < dimension; y++) {
                    for (var z = 0; z < dimension; z++) {
                        var splitLevelTile = new Tile3D(splitLevel, x, y, z);
                        var children = GetChildren(tiles, splitLevelTile);

                        if (children.Count > 0) {
                            var childrenBbox3D = GetBoundingBox3D(children, tileBounds);
                            var childrenBbox2D = new BoundingBox(childrenBbox3D.XMin, childrenBbox3D.YMin, childrenBbox3D.XMax, childrenBbox3D.YMax);
                            var childrenBoundingVolumeRegion = childrenBbox2D.ToRadians().ToRegion(childrenBbox3D.ZMin, childrenBbox3D.ZMax);
                            
                            // Translation is the same as identity matrix in case of child tileset.
                            var tileset = TreeSerializer.ToTileset3D(children, tileBounds, null, childrenBoundingVolumeRegion, geometricError, geometricErrorFactor, version, refinement, createGltf, tilesetVersion);

                            var childGeometricError = GeometricErrorCalculator.GetGeometricError(geometricError, geometricErrorFactor, splitLevel);
                            tileset.geometricError = childGeometricError;
                            tileset.root.geometricError = GeometricErrorCalculator.GetGeometricError(childGeometricError, geometricErrorFactor, 1);
                            var detailedJson = JsonConvert.SerializeObject(tileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                            var filename = $"tileset_{splitLevel}_{z}_{x}_{y}.json";
                            Console.Write($"\rWriting {filename}...");

                            File.WriteAllText($"{outputDirectory}{Path.AltDirectorySeparatorChar}{filename}", detailedJson);
                            externalTilesets++;
                            // add the child tilesets to the root tileset
                            var child = new Child();
                            child.boundingVolume = new Boundingvolume() { region = childrenBoundingVolumeRegion };
                            child.refine = refinement;
                            child.geometricError = tileset.root.geometricError;

                            child.content = new Content() { uri = filename };
                            rootTileset.root.children.Add(child);
                        }
                    }
                }
            }
        }
        // write the root tileset
        Console.WriteLine();
        Console.WriteLine($"External tileset.json files: {externalTilesets}");
        Console.WriteLine("Writing root tileset.json...");
        var rootJson = JsonConvert.SerializeObject(rootTileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        File.WriteAllText($"{outputDirectory}{Path.AltDirectorySeparatorChar}tileset.json", rootJson);
    }

}
