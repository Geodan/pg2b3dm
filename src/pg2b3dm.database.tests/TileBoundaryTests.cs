using B3dm.Tileset;
using B3dm.Tileset.Extensions;
using B3dm.Tileset.settings;
using DotNet.Testcontainers.Builders;
using Npgsql;
using subtree;
using Testcontainers.PostgreSql;
using Wkb2Gltf;
using Wkx;

namespace pg2b3dm.database.tests;

/// <summary>
/// The quadtree puts a feature into the tile its envelope centroid falls in, so
/// tiles that share an edge must split the features between them, and the root
/// tile must hold them all: every feature in exactly one tile. Checked per id,
/// so that a feature lost and another one duplicated cannot cancel out.
/// </summary>
public class TileBoundaryTests
{
    private PostgreSqlContainer _container;

    // the four tiles meeting at (7, 51), each 0.01 degree on a side
    private static readonly (double xmin, double ymin, double xmax, double ymax)[] Quadrants = {
        (6.99, 50.99, 7.00, 51.00), (7.00, 50.99, 7.01, 51.00),
        (6.99, 51.00, 7.00, 51.01), (7.00, 51.00, 7.01, 51.01) };

    // the same in UTM 32N, kept in that projection: four 1 km tiles meeting at
    // (400000, 5650000)
    private static readonly (double xmin, double ymin, double xmax, double ymax)[] UtmQuadrants = {
        (399000, 5649000, 400000, 5650000), (400000, 5649000, 401000, 5650000),
        (399000, 5650000, 400000, 5651000), (400000, 5650000, 401000, 5651000) };

    // the extents of the curved-projection tables in 7_create_tile_boundary_tables.sql
    private static readonly (string table, int epsg, (double xmin, double ymin, double xmax, double ymax) extent)[] CurvedExtents = {
        ("europe_laea", 3035, (-20, 27, 45, 72)),
        ("arctic", 3413, (-180, 60, 180, 89)),
        ("antarctic_sector", 3031, (-100, -89, 80, -60)) };

    // the tiles of a level, split as QuadtreeTiler splits them (keep the two in step)
    private static List<(double xmin, double ymin, double xmax, double ymax)> TileGrid(
        (double xmin, double ymin, double xmax, double ymax) box, int level)
    {
        if (level == 0) {
            return new() { box };
        }
        var dx = (box.xmax - box.xmin) / 2;
        var dy = (box.ymax - box.ymin) / 2;
        var tiles = new List<(double, double, double, double)>();
        foreach (var quadrant in new[] {
            (box.xmin, box.ymin, box.xmin + dx, box.ymin + dy), (box.xmin + dx, box.ymin, box.xmax, box.ymin + dy),
            (box.xmin, box.ymin + dy, box.xmin + dx, box.ymax), (box.xmin + dx, box.ymin + dy, box.xmax, box.ymax) }) {
            tiles.AddRange(TileGrid(quadrant, level - 1));
        }
        return tiles;
    }

    [OneTimeSetUp]
    public async Task Setup()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:16-3.4-alpine")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();
        await _container.StartAsync();
        var result = await _container.ExecScriptAsync(File.ReadAllText("./postgres-db/7_create_tile_boundary_tables.sql"));
        Assert.That(result.ExitCode, Is.EqualTo(0), result.Stderr);
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    private static string Sql(double value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private List<long> Ids(string sql)
    {
        using var conn = new NpgsqlConnection(_container.GetConnectionString());
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        var ids = new List<long>();
        while (reader.Read()) {
            ids.Add(reader.GetInt64(0));
        }
        return ids;
    }

    private void AssertEachFeatureInOneTile(string table, int epsg,
        IEnumerable<(double xmin, double ymin, double xmax, double ymax)> tiles,
        bool keepProjection = false, string query = "")
    {
        var found = new List<long>();
        foreach (var (xmin, ymin, xmax, ymax) in tiles) {
            var where = GeometryRepository.GetWhere("geom", new Point(xmin, ymin), new Point(xmax, ymax), query, epsg, keepProjection);
            found.AddRange(Ids($"select id from {table} where {where}"));
        }
        var expected = Ids($"select id from {table} where true {query}");
        Assert.Multiple(() => {
            Assert.That(found.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key),
                Is.Empty, "in more than one tile");
            Assert.That(expected.Except(found), Is.Empty, "in no tile");
            Assert.That(found.Except(expected), Is.Empty, "not selected by the query");
        });
    }

    private void AssertTheRootTileHoldsEveryFeature(string table, int epsg, bool keepProjection = false)
    {
        using var conn = new NpgsqlConnection(_container.GetConnectionString());
        var (root, _, _) = BoundingBoxRepository.GetBoundingBoxForQuadtree(conn, table, "geom", epsg, keepProjection);
        AssertEachFeatureInOneTile(table, epsg, new[] { (root.XMin, root.YMin, root.XMax, root.YMax) }, keepProjection);
    }

    [Test]
    public void TilesSharingAnEdgeSplitWgs84Features()
    {
        AssertEachFeatureInOneTile("edge_points", 4326, Quadrants);
    }

    [Test]
    public void TilesSharingAnEdgeSplitEtrs89Features()
    {
        AssertEachFeatureInOneTile("edge_points_etrs89", 4258, Quadrants);
    }

    [Test]
    public void TilesSharingAnEdgeSplitProjectedFeatures()
    {
        AssertEachFeatureInOneTile("edge_points_utm", 32632, Quadrants);
    }

    [Test]
    public void TilesSharingAnEdgeSplitFeaturesWhereTheTransformsDisagree()
    {
        AssertEachFeatureInOneTile("edge_points_dhdn", 31467, new[] {
            (14.4995, 54.7497, 14.5, 54.75), (14.5, 54.7497, 14.5005, 54.75),
            (14.4995, 54.75, 14.5, 54.7503), (14.5, 54.75, 14.5005, 54.7503) });
    }

    [Test]
    public void TilesSharingAnEdgeSplitFeaturesKeptInTheirProjection()
    {
        AssertEachFeatureInOneTile("edge_points_kept", 32632, UtmQuadrants, keepProjection: true);
    }

    [Test]
    public void TilesAcrossTheCentralMeridianSplitProjectedFeatures()
    {
        // offset, so the meridian falls between the points of any even split of the edge
        AssertEachFeatureInOneTile("across_meridian", 32632,
            new[] { (8.47, 49.5, 9.47, 50.5), (8.47, 50.5, 9.47, 51.5) });
    }

    [Test]
    public void AFeatureOnASharedEdgeBelongsToOneTile()
    {
        AssertEachFeatureInOneTile("on_edge_points", 4326, Quadrants);
    }

    [Test]
    public void TheQueryStillFiltersTheTiles()
    {
        AssertEachFeatureInOneTile("edge_points", 4326, Quadrants, query: " and id <= 100");
        AssertEachFeatureInOneTile("edge_points_kept", 32632, UtmQuadrants, keepProjection: true, query: " and id <= 100");
    }

    [Test]
    public void TheRootTileHoldsEveryFeature()
    {
        AssertTheRootTileHoldsEveryFeature("edge_points", 4326);
        AssertTheRootTileHoldsEveryFeature("edge_points_etrs89", 4258);
        AssertTheRootTileHoldsEveryFeature("edge_points_utm", 32632);
        AssertTheRootTileHoldsEveryFeature("edge_points_kept", 32632, keepProjection: true);
    }

    [Test]
    public void TheRootTileHoldsEveryFeatureOfAnExtentOnTheCentralMeridian()
    {
        AssertTheRootTileHoldsEveryFeature("extent_25km", 32632);
        AssertTheRootTileHoldsEveryFeature("extent_200km", 32632);
    }

    [Test]
    public void TilesOfCurvedProjectionsSplitTheirFeatures()
    {
        foreach (var (table, epsg, extent) in CurvedExtents) {
            for (var level = 0; level <= 2; level++) {
                AssertEachFeatureInOneTile(table, epsg, TileGrid(extent, level));
            }
        }
    }

    /// <summary>
    /// Whether the planner picks the index depends on the table; this shows the
    /// filters leave it a choice, for the read and for the count.
    /// </summary>
    [Test]
    public void TheTileFiltersCanUseTheSpatialIndex()
    {
        using var conn = new NpgsqlConnection(_container.GetConnectionString());
        conn.Open();
        using (var off = new NpgsqlCommand("SET enable_seqscan = off", conn)) {
            off.ExecuteNonQuery();
        }
        foreach (var keepProjection in new[] { false, true }) {
            var (table, from, to) = keepProjection ?
                ("edge_points_kept", new Point(399000, 5649000), new Point(400000, 5650000)) :
                ("edge_points_utm", new Point(6.99, 50.99), new Point(7.0, 51.0));
            foreach (var exact in new[] { true, false }) {
                var where = exact ?
                    GeometryRepository.GetWhere("geom", from, to, "", 32632, keepProjection) :
                    GeometryRepository.GetIndexFilter("geom", from, to, "", 32632, keepProjection);
                using var cmd = new NpgsqlCommand($"EXPLAIN select id from {table} where {where}", conn);
                using var reader = cmd.ExecuteReader();
                var plan = new List<string>();
                while (reader.Read()) {
                    plan.Add(reader.GetString(0));
                }
                Assert.That(string.Join("\n", plan), Does.Contain($"{table}_st_centroid_idx"), $"keepProjection {keepProjection}, exact {exact}");
            }
        }
    }

    /// <summary>
    /// A whole quadtree from the root box Program takes, split until no tile holds
    /// more than 50 features. Every feature must be in exactly one tile with
    /// content. Which features a tile holds is read back through the tile filter, on
    /// the box the tiler gave the tile.
    /// </summary>
    [TestCase("edge_points_squares", 4326, false)]
    [TestCase("edge_points_utm_squares", 32632, false)]
    [TestCase("edge_points_kept_squares", 32632, true)]
    [TestCase("extent_25km_squares", 32632, false)]
    [TestCase("europe_laea_squares", 3035, false)]
    public void TheQuadtreePutsEveryFeatureInOneTile(string table, int epsg, bool keepProjection)
    {
        var connectionString = _container.GetConnectionString();
        using var conn = new NpgsqlConnection(connectionString);
        var (root, _, _) = BoundingBoxRepository.GetBoundingBoxForQuadtree(conn, table, "geom", epsg, keepProjection);
        OutputDirectoryCreator.GetFolders("output");
        var inputTable = new InputTable() { TableName = table, GeometryColumn = "geom", EPSGCode = epsg };
        var tiler = new QuadtreeTiler(connectionString, inputTable, new StylingSettings(), 50,
            new double[] { 0, 0, 0 }, "output/content", new List<int>() { 0 }, skipCreateTiles: true);
        var rootTile = new Tile(0, 0, 0) { BoundingBox = root.ToArray() };
        var tiles = tiler.GenerateTiles(root, rootTile, new List<Tile>(), keepProjection: keepProjection);

        var withContent = tiles.Where(t => t.Available).ToList();
        Assert.That(withContent.Count, Is.GreaterThan(1));
        AssertEachFeatureInOneTile(table, epsg, withContent.Select(t => BoxOf((root.XMin, root.YMin, root.XMax, root.YMax), t)), keepProjection);
    }

    // the box QuadtreeTiler gave tile (Z, X, Y), from the root down
    private static (double xmin, double ymin, double xmax, double ymax) BoxOf(
        (double xmin, double ymin, double xmax, double ymax) box, Tile tile)
    {
        for (var level = tile.Z - 1; level >= 0; level--) {
            var right = (tile.X >> level) & 1;
            var up = (tile.Y >> level) & 1;
            var dx = (box.xmax - box.xmin) / 2;
            var dy = (box.ymax - box.ymin) / 2;
            box = (right == 0 ? box.xmin : box.xmin + dx, up == 0 ? box.ymin : box.ymin + dy,
                   right == 0 ? box.xmin + dx : box.xmax, up == 0 ? box.ymin + dy : box.ymax);
        }
        return box;
    }

    /// <summary>
    /// The count decides whether a tile is split, and a tile counted empty is never
    /// read: it may count more than a tile holds, never fewer. What a tile holds is
    /// taken here from the definition, not from the tile filter: the centroids in
    /// the tile, in lon/lat.
    /// </summary>
    [Test]
    public void ATileIsNeverCountedSmallerThanItIs()
    {
        using var conn = new NpgsqlConnection(_container.GetConnectionString());
        var cases = new List<(string table, int epsg, bool keep, (double, double, double, double) tile)>();
        foreach (var t in Quadrants) {
            cases.Add(("edge_points", 4326, false, t));
            cases.Add(("edge_points_utm", 32632, false, t));
        }
        foreach (var t in UtmQuadrants) {
            cases.Add(("edge_points_kept", 32632, true, t));
        }
        foreach (var t in new[] { (14.4995, 54.7497, 14.5, 54.75), (14.5, 54.75, 14.5005, 54.7503) }) {
            cases.Add(("edge_points_dhdn", 31467, false, t));
        }
        foreach (var (table, epsg, extent) in CurvedExtents) {
            for (var level = 0; level <= 2; level++) {
                foreach (var t in TileGrid(extent, level)) {
                    cases.Add((table, epsg, false, t));
                }
            }
        }
        Assert.Multiple(() => {
            foreach (var (table, epsg, keep, (xmin, ymin, xmax, ymax)) in cases) {
                var from = new Point(xmin, ymin);
                var to = new Point(xmax, ymax);
                var counted = FeatureCountRepository.CountFeaturesInBox(conn, table, "geom", from, to, "", epsg, keep);
                var point = keep ? "ST_Centroid(ST_Envelope(geom))" : "st_transform(ST_Centroid(ST_Envelope(geom)), 4326)";
                var held = Ids($"select id from (select id, {point} as p from {table}) as t " +
                    $"where ST_X(p) >= {Sql(xmin)} and ST_X(p) < {Sql(xmax)} and ST_Y(p) >= {Sql(ymin)} and ST_Y(p) < {Sql(ymax)}").Count;
                Assert.That(counted, Is.GreaterThanOrEqualTo(held), $"{table} {xmin} {ymin} {xmax} {ymax}");
            }
        });
    }
}
