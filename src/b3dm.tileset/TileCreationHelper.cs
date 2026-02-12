using System;
using System.Collections.Generic;
using System.IO;
using pg2b3dm;
using Wkb2Gltf;

namespace B3dm.Tileset;

public static class TileCreationHelper
{
    public static void WriteTileIfNeeded(List<GeometryRecord> geometries, double[] translation, StylingSettings stylingSettings, string copyright, bool createGltf, bool skipCreateTiles, string outputPath, string displayName)
    {
        if (skipCreateTiles) {
            return;
        }

        var bytes = TileWriter.ToTile(geometries, translation, copyright: copyright, addOutlines: stylingSettings.AddOutlines, defaultColor: stylingSettings.DefaultColor, defaultMetallicRoughness: stylingSettings.DefaultMetallicRoughness, doubleSided: stylingSettings.DoubleSided, defaultAlphaMode: stylingSettings.DefaultAlphaMode, alphaCutoff: stylingSettings.AlphaCutoff, createGltf: createGltf);
        Console.Write($"\rCreating tile: {displayName}  ");
        File.WriteAllBytes(outputPath, bytes);
    }
}
