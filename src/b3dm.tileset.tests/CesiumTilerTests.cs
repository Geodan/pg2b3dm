using System;
using System.Collections.Generic;
using System.IO;
using B3dm.Tileset.settings;
using NUnit.Framework;
using pg2b3dm;
using subtree;

namespace B3dm.Tileset.Tests;
public class CesiumTilerTests
{
    [Test]
    public void TestCreateImplicitTileset()
    {
        // Arrange
        var version = new Version(1, 0);
        var createGltf = true;
        var outputDirectory = "test";
        var outputSettings = OutputDirectoryCreator.GetFolders(outputDirectory);
        var translation = new double[] { 0, 0, 0 };
        var geometricError = 100;
        var rootBoundingVolumeRegion = new double[] { 0, 0, 0, 0, 0, 0 };
        var tile1 = new Tile(1, 1, 1);
        var tiles = new List<Tile>() { tile1 };
        var tilesetSettings = new TilesetSettings
        {
            Version = version,
            Translation = translation,
            GeometricError = geometricError,
            RootBoundingVolumeRegion = rootBoundingVolumeRegion
        };

        // Act
        var subtreeLevels = CesiumTiler.CreateSubtreeFiles(outputSettings, tiles);
        CesiumTiler.CreateImplicitTileset(tilesetSettings, outputSettings, createGltf, false);

        // Assert
        var json = File.ReadAllText("test/tileset.json");
        Assert.That(json.Contains("geometricError"));
    }

    [Test]
    public void TestCreateExplicitTilesetsJson()
    {
        // Arrange
        var version = new Version(1, 0);
        var outputDirectory = "testExplicit";
        var outputDirs = OutputDirectoryCreator.GetFolders(outputDirectory);
        var zmin = 0.0;
        var zmax = 1.0;
        var translation = new double[] { 0, 0, 0 };
        var geometricError = 100;
        var geometricErrorFactor = 2;
        var refinement = RefinementType.ADD;
        var rootBoundingVolumeRegion = new double[] { 0, 0, 0, 0, 0, 0, zmin, zmax };
        var tile0 = new Tile(0, 0, 0);
        tile0.ZMin = zmin;
        tile0.ZMax = zmax;

        var tile1 = new Tile(2, 0, 0);
        tile1.ZMin = zmin;
        tile1.ZMax = zmax;
        var tile2 = new Tile(2, 0, 0);
        tile2.ZMin = zmin;
        tile2.ZMax = zmax;
        var tile3 = new Tile(3, 0, 0) { Available = true, BoundingBox = new double[] { 5,50,6,55} };
        tile3.ZMin = zmin;
        tile3.ZMax = zmax;

        var tiles = new List<Tile>() { tile0, tile1, tile2, tile3};

        // Act
        CesiumTiler.CreateExplicitTilesetsJson(version, outputDirectory, translation, geometricError, geometricErrorFactor, refinement, rootBoundingVolumeRegion, tile1, tiles);

        // Assert
        var json = File.ReadAllText("testExplicit/tileset.json");
        Assert.That(json.Contains("geometricError"));
    }
}
