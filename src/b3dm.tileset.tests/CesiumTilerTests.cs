using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
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
        Directory.CreateDirectory(outputDirectory);
        var translation = new double[] { 0, 0, 0 };
        var geometricErrors = new double[] { 0.1 };
        var rootBoundingVolumeRegion = new double[] { 0, 0, 0, 0, 0, 0 };
        var subtreesDirectory = "output";
        var tile1 = new Tile(1, 1, 1);
        var tiles = new List<Tile>() { tile1 };

        // Act
        CesiumTiler.CreateImplicitTileset(version, createGltf, outputDirectory, translation, geometricErrors, rootBoundingVolumeRegion, subtreesDirectory, tiles);

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
        Directory.CreateDirectory(outputDirectory);
        var zmin = 0.0;
        var zmax = 1.0;
        var translation = new double[] { 0, 0, 0 };
        var geometricErrors = new double[] { 100,0 };
        var refinement = "add";
        var use10 = false;
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
        CesiumTiler.CreateExplicitTilesetsJson(version, outputDirectory, translation, geometricErrors, refinement, use10, rootBoundingVolumeRegion, tile1, tiles);

        // Assert
        var json = File.ReadAllText("testExplicit/tileset.json");
        Assert.That(json.Contains("geometricError"));
    }
}
