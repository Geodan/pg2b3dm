using System;
using System.IO;
using System.Linq;
using B3dm.Tileset;
using Npgsql;
using Wkx;

namespace pg2b3dm;
public static class MapboxTiler
{
    public static void CreateMapboxTiles(string table, string geometryColumn, string defaultColor, string defaultMetallicRoughness, bool createGltf, int zoom, string shadersColumn, string attributeColumns, string copyright, string query, NpgsqlConnection conn, int source_epsg, BoundingBox bbox, string contentDirectory)
    {
        // mapbox specific code

        Console.WriteLine("Starting Experimental MapBox v3 mode...");

        var target_srs = 3857;
        var tiles = Tiles.Tools.Tilebelt.GetTilesOnLevel(new double[] { bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax }, zoom);

        Console.WriteLine($"Creating tiles for level {zoom}: {tiles.Count()}");

        foreach (var t in tiles) {
            var bounds = t.Bounds();

            var query1 = (query != string.Empty ? $" and {query}" : String.Empty);

            var numberOfFeatures = FeatureCountRepository.CountFeaturesInBox(conn, table, geometryColumn, new Point(bounds[0], bounds[1]), new Point(bounds[2], bounds[3]), query1);

            if (numberOfFeatures > 0) {
                var ul = t.BoundsUL();
                var ur = t.BoundsUR();
                var ll = t.BoundsLL();

                var ul_spherical = SphericalMercator.ToSphericalMercatorFromWgs84(ul.X, ul.Y);
                var ur_spherical = SphericalMercator.ToSphericalMercatorFromWgs84(ur.X, ur.Y);
                var ll_spherical = SphericalMercator.ToSphericalMercatorFromWgs84(ll.X, ll.Y);
                var width = ur_spherical[0] - ul_spherical[0];
                var height = ul_spherical[1] - ll_spherical[1];

                var ext = createGltf ? "glb" : "b3dm";
                var geometries = GeometryRepository.GetGeometrySubset(conn, table, geometryColumn, bounds, source_epsg, target_srs, shadersColumn, attributeColumns, query1);

                // in Mapbox mode, every tile has 2^13 = 8192 values
                // see https://github.com/mapbox/mapbox-gl-js/blob/main/src/style-spec/data/extent.js
                var extent = 8192;
                double[] scale = { extent / width, -1 * extent / height, 1 };
                // in Mapbox mode
                //  - we use YAxisUp = false
                //  - all coordinates are relative to the upperleft coordinate
                //  - Outlines is set to false because outlines extension is not supported (yet) in Mapbox client
                var bytes = TileWriter.ToTile(geometries, new double[] { ul_spherical[0], ul_spherical[1], 0 }, scale, copyright, false, defaultColor, defaultMetallicRoughness, createGltf: createGltf, YAxisUp: false);
                File.WriteAllBytes($@"{contentDirectory}{Path.AltDirectorySeparatorChar}{t.Z}-{t.X}-{t.Y}.{ext}", bytes);
                Console.Write(".");

            }
        }
        Console.WriteLine();
        Console.WriteLine("Warning: Draco compress the resulting tiles. If not compressed, visualization in Mapbox will not be correct (v3.2.0)");
        // end mapbox specific code
    }
}
