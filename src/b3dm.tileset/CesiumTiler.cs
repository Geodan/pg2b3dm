using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B3dm.Tileset.Extensions;
using Newtonsoft.Json;
using subtree;
using Wkx;

namespace B3dm.Tileset;
public static class CesiumTiler
{
    public static void CreateImplicitTileset(Version version, bool createGltf, string outputDirectory, double[] translation, double[] geometricErrors, double[] rootBoundingVolumeRegion, string subtreesDirectory, List<Tile> tiles)
    {
        if (!Directory.Exists(subtreesDirectory)) {
            Directory.CreateDirectory(subtreesDirectory);
        }

        var subtreeFiles = SubtreeCreator.GenerateSubtreefiles(tiles);
        Console.WriteLine($"Writing {subtreeFiles.Count} subtree files...");
        foreach (var s in subtreeFiles) {
            var t = s.Key;
            var subtreefile = $"{subtreesDirectory}{Path.AltDirectorySeparatorChar}{t.Z}_{t.X}_{t.Y}.subtree";
            File.WriteAllBytes(subtreefile, s.Value);
        }

        var subtreeLevels = subtreeFiles.Count > 1 ? ((Tile)subtreeFiles.ElementAt(1).Key).Z : 2;
        var availableLevels = tiles.Max(t => t.Z) + 1;
        Console.WriteLine("Available Levels: " + availableLevels);
        Console.WriteLine("Subtree Levels: " + subtreeLevels);
        var tilesetjson = TreeSerializer.ToImplicitTileset(translation, rootBoundingVolumeRegion, geometricErrors[0], availableLevels, subtreeLevels, version, createGltf);
        var file = $"{outputDirectory}{Path.AltDirectorySeparatorChar}tileset.json";
        var json = JsonConvert.SerializeObject(tilesetjson, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        Console.WriteLine("SubdivisionScheme: QUADTREE");
        Console.WriteLine($"Writing {file}...");
        File.WriteAllText(file, json);
    }

    public static void CreateExplicitTilesetsJson(Version version, string outputDirectory, double[] translation, double[] geometricErrors, string refinement, bool use10, double[] rootBoundingVolumeRegion, Tile tile, List<Tile> tiles)
    {
        var splitLevel = (int)Math.Ceiling((tiles.Max((Tile s) => s.Z) + 1.0) / 2.0);

        var rootTiles = TileSelector.Select(tiles, tile, 0, splitLevel);
        var rootTileset = TreeSerializer.ToTileset(rootTiles, translation, rootBoundingVolumeRegion, geometricErrors, version, refinement, use10);

        var maxlevel = tiles.Max((Tile s) => s.Z);

        var externalTilesets = 0;
        if (maxlevel > splitLevel) {
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
                        var tileset = TreeSerializer.ToTileset(children, null, childrenBoundingVolumeRegion, geometricErrors, version, refinement, use10);
                        var detailedJson = JsonConvert.SerializeObject(tileset, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                        var filename = $"tileset_{splitLevel}_{i}_{j}.json";
                        Console.Write($"\rWriting {filename}...");

                        File.WriteAllText($"{outputDirectory}{Path.AltDirectorySeparatorChar}{filename}", detailedJson);
                        externalTilesets++;
                        // add the child tilesets to the root tileset
                        var child = new Child();
                        child.boundingVolume = new Boundingvolume() { region = childrenBoundingVolumeRegion };
                        child.refine = refinement;
                        child.geometricError = geometricErrors[0];
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

}
