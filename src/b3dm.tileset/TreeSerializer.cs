﻿using System;
using System.Collections.Generic;
using System.IO;
using B3dm.Tileset.Extensions;
using subtree;
using Wkx;

namespace B3dm.Tileset;

public static class TreeSerializer
{
    public static TileSet ToImplicitTileset(double[] translate, double[] box, double maxGeometricError, int availableLevels, int subtreeLevels, Version version = null, bool createGltf = false, string tilesetVersion = "", string crs="", bool keepProjection = false, SubdivisionScheme subDivisionScheme = SubdivisionScheme.QUADTREE)
    {
        var ext = createGltf ? ".glb" : ".b3dm";
        var geometricError = maxGeometricError;
        var tileset = GetTilesetObject(version, maxGeometricError, false, tilesetVersion, crs);
        var t = new double[] {   1.0, 0.0, 0.0, 0.0,
                                 0.0,1.0, 0.0, 0.0,
                                 0.0, 0.0, 1.0, 0.0,
        translate[0], translate[1], translate[2], 1.0};
        var root = GetRoot(geometricError, t, box, keepProjection: keepProjection);
        var content = new Content() { uri = "content/{level}_{x}_{y}" + ext };
        root.content = content;
        var subtrees = new Subtrees() { uri = "subtrees/{level}_{x}_{y}.subtree" };
        root.implicitTiling = new Implicittiling() { subdivisionScheme = subDivisionScheme, availableLevels = availableLevels, subtreeLevels = subtreeLevels, subtrees = subtrees };
        tileset.root = root;
        return tileset;
    }

    public static TileSet ToTileset(List<Tile> tiles, double[] translate, double[] region, double geometricError, double geometricErrorFactor = 2, Version version = null, string refine="ADD", bool use10 = false, string tilesetVersion = "", string crs="")
    {
        var tileset = GetTilesetObject(version, geometricError, use10, tilesetVersion, crs);

        var t = new double[] {   1.0, 0.0, 0.0, 0.0,
                                     0.0,1.0, 0.0, 0.0,
                                     0.0, 0.0, 1.0, 0.0,
            0.0, 0.0, 0.0, 1.0};

        if (translate != null) {
            t = new double[] {   1.0, 0.0, 0.0, 0.0,
                                 0.0,1.0, 0.0, 0.0,
                                 0.0, 0.0, 1.0, 0.0,
            translate[0], translate[1], translate[2], 1.0};
        }

        var root = GetRoot(geometricError, t, region, refine);
        tileset.geometricError = geometricError;
        root.geometricError = GeometricErrorCalculator.GetGeometricError(geometricError, geometricErrorFactor, 1);
        var childrenGeometricError = GeometricErrorCalculator.GetGeometricError(geometricError, geometricErrorFactor, 2);
        var children = GetChildren(tiles, childrenGeometricError, geometricErrorFactor);
        root.children = children;

        tileset.root = root;
        return tileset;
    }

    public static TileSet GetTilesetObject(Version version, double geometricError, bool use10 = false, string tilesetVersion = "", string crs="")
    {
        var version3DTiles = use10 ? "1.0" : "1.1"; 
        var tileset = new TileSet { asset = new Asset() { version = $"{version3DTiles}", generator = $"pg2b3dm {version}" } };
        if(!string.IsNullOrEmpty(tilesetVersion)) {
            tileset.asset.tilesetVersion = tilesetVersion;
        }
        if (!string.IsNullOrEmpty(crs)) {
            tileset.asset.crs = crs;
        }
        tileset.geometricError = geometricError;
        return tileset;
    }

    private static double[] GetBBox(double[] region, double[] translation)
    {
        // return Array of 12 double values representing the bounding box
        var xmin = region[0] - translation[12];
        var ymin = region[1] - translation[13];
        var xmax = region[2]- translation[12];
        var ymax = region[3] - translation[13];
        var zmin = region[4] - translation[14];
        var zmax = region[5] - translation[14];

        var centre = new double[] {
            Math.Round((xmin + xmax) / 2.0, 6), 
            Math.Round((ymin + ymax) / 2.0, 6),
            Math.Round((zmin + zmax) / 2.0, 6)
        };

        var res = new double[] {
            centre[0], centre[1], centre[2],
            (region[2] - region[0]) / 2, 0, 0,
            0, (region[3] - region[1]) / 2, 0,
            0, 0, (region[5] - region[4]) / 2
            };
        return res;
    }

    public static Root GetRoot(double geometricError, double[] translation, double[] region, string refine = "ADD", bool keepProjection = false)
    {
        var boundingVolume = keepProjection ?
            new Boundingvolume { box = GetBBox(region, translation) } :
            new Boundingvolume { region = region };

        var root = new Root {
            geometricError = geometricError,
            refine = refine,
            transform = translation,
            boundingVolume = boundingVolume
        };

        return root;
    }

    private static List<Child> GetChildren(List<Tile> tiles, double geometricError, double geometricErrorFactor)
    {
        var children = new List<Child>();
        foreach (var tile in tiles) {
            if (tile.Available) {
                var ge = GeometricErrorCalculator.GetGeometricError(geometricError, geometricErrorFactor, tile.Z, tile.Lod);
                var child = GetChild(tile, ge);

                if (tile.Children != null) {
                    child.children = GetChildren(tile.Children, geometricError, geometricErrorFactor);
                }
                children.Add(child);
            }
        }

        return children;
    }

    public static Child GetChild(Tile tile, double geometricError)
    {
        var child = new Child {
            geometricError = geometricError,
            content = new Content()
        };
        child.content.uri = $"content{Path.AltDirectorySeparatorChar}{tile.ContentUri}";

        var bbox = tile.BoundingBox;
        var boundingBox = new BoundingBox(bbox[0], bbox[1], bbox[2], bbox[3]);
        var region = boundingBox.ToRadians().ToRegion((double)tile.ZMin, (double)tile.ZMax);
        child.boundingVolume = new Boundingvolume {
            region = region
        };

        return child;
    }
}
