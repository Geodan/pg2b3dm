﻿using System.IO;
using B3dm.Tileset.settings;

namespace pg2b3dm;

public static class OutputDirectoryCreator
{
    public static OutputSettings GetFolders(string outputFolder)
    {
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
            OutputFolder = outputFolder,
            ContentFolder = contentDirectory,
            SubtreesFolder = subtreesDirectory,
        };
        return outputSettings;

    }

}
