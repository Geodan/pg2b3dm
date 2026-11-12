-- Tables for TileBoundaryTests: points near tile edges, in several CRSs.
-- points either side of the two edges through (7, 51); each row runs
-- along one edge and crosses the other, with one point on it
CREATE TABLE edge_points AS
SELECT row_number() OVER () AS id,
       ST_SetSRID(ST_MakePoint(lon, lat), 4326) AS geom
FROM (
    SELECT 6.991 + 0.0001 * i AS lon, 51.0 + s * d / 111000.0 AS lat
    FROM generate_series(0, 179) i, unnest(array[-1, 1]) s, unnest(array[0.05, 0.1, 0.2, 0.3, 0.5, 0.8, 1, 2, 5, 10, 20, 50]) d
    UNION ALL
    SELECT 7.0 + s * d / 70000.0, 50.991 + 0.0001 * i
    FROM generate_series(0, 179) i, unnest(array[-1, 1]) s, unnest(array[0.05, 0.1, 0.2, 0.3, 0.5, 0.8, 1, 2, 5, 10, 20, 50]) d
) p;
-- the same points in UTM 32N, 2 degrees west of its central meridian,
-- and in ETRS89, a geographic CRS that is not 4326
CREATE TABLE edge_points_utm AS
SELECT id, ST_Transform(geom, 32632) AS geom FROM edge_points;
CREATE TABLE edge_points_etrs89 AS
SELECT id, ST_SetSRID(geom, 4258) AS geom FROM edge_points;
-- DHDN / 3-degree Gauss-Krueger zone 3 at the edge of its grid, where it and
-- 4326 differ by 1.6 m depending on the direction transformed in: points
-- 5 cm to 5 m either side of the inner edges of four tiles about 35 m wide,
-- and at least 5 m inside the outer ones, which the 1.6 m keep them inside
CREATE TABLE edge_points_dhdn AS
SELECT row_number() OVER () AS id,
       ST_Transform(ST_SetSRID(ST_MakePoint(lon, lat), 4326), 31467) AS geom
FROM (
    SELECT 14.4996 + 0.00001 * (i + 0.5) AS lon, 54.75 + s * d / 111000.0 AS lat
    FROM generate_series(0, 79) i, unnest(array[-1, 1]) s,
         unnest(array[0.05, 0.2, 0.5, 1, 2, 5]) d
    UNION ALL
    SELECT 14.5 + s * d / 64000.0, 54.74975 + 0.000005 * (i + 0.5)
    FROM generate_series(0, 99) i, unnest(array[-1, 1]) s,
         unnest(array[0.05, 0.2, 0.5, 1, 2, 5]) d
) p;
-- points exactly on the edges through (7, 51), and on their crossing
CREATE TABLE on_edge_points AS
SELECT row_number() OVER () AS id, ST_SetSRID(ST_MakePoint(lon, lat), 4326) AS geom
FROM (SELECT 6.995 AS lon, 51.0 AS lat UNION ALL SELECT 7.005, 51.0
      UNION ALL SELECT 7.0, 50.995 UNION ALL SELECT 7.0, 51.005
      UNION ALL SELECT 7.0, 51.0) p;
-- for UtmQuadrants: points either side of the edges through
-- (400000, 5650000)
CREATE TABLE edge_points_kept AS
SELECT row_number() OVER () AS id, ST_SetSRID(ST_MakePoint(x, y), 32632) AS geom
FROM (
    SELECT 399100 + 10 * i AS x, 5650000 + s * d AS y
    FROM generate_series(0, 179) i, unnest(array[-1, 1]) s, unnest(array[0.05, 0.1, 0.2, 0.3, 0.5, 0.8, 1, 2, 5, 10, 20, 50]) d
    UNION ALL
    SELECT 400000 + s * d, 5649100 + 10 * i
    FROM generate_series(0, 179) i, unnest(array[-1, 1]) s, unnest(array[0.05, 0.1, 0.2, 0.3, 0.5, 0.8, 1, 2, 5, 10, 20, 50]) d
) p;
-- two 1-degree tiles across the central meridian (9 E) of UTM 32N, one
-- above the other: their shared edge, the parallel 50.5, is a curve in
-- UTM that sags about 120 m below the tiles' corners at the meridian.
-- Points 5 cm to 200 m either side of it, along the middle half.
CREATE TABLE across_meridian AS
SELECT row_number() OVER () AS id,
       ST_Transform(ST_SetSRID(ST_MakePoint(lon, lat), 4326), 32632) AS geom
FROM (SELECT 8.75 + 0.005 * i AS lon, 50.5 + s * d / 111000.0 AS lat
      FROM generate_series(0, 100) i, unnest(array[-1, 1]) s,
           unnest(array[0.05, 0.5, 5, 50, 100, 200]) d) p;
-- projected extents centred on the central meridian, 25 and 200 km wide,
-- points along all four edges: a straight top edge in UTM curves above
-- its corners in lon/lat
CREATE TABLE extent_25km AS
SELECT row_number() OVER () AS id, ST_SetSRID(ST_MakePoint(x, y), 32632) AS geom
FROM (SELECT 487500 + 250 * i AS x, y FROM generate_series(0, 100) i,
             unnest(array[5650000, 5675000]) y
      UNION ALL
      SELECT x, 5650000 + 250 * i FROM generate_series(0, 100) i,
             unnest(array[487500, 512500]) x) p;
CREATE TABLE extent_200km AS
SELECT row_number() OVER () AS id, ST_SetSRID(ST_MakePoint(x, y), 32632) AS geom
FROM (SELECT 400000 + 1000 * i AS x, y FROM generate_series(0, 200) i,
             unnest(array[5600000, 5800000]) y
      UNION ALL
      SELECT x, 5600000 + 1000 * i FROM generate_series(0, 200) i,
             unnest(array[400000, 600000]) x) p;

-- Large tiles in strongly curved projections: Europe in LAEA, the Arctic in polar
-- stereographic, an off-centre sector of Antarctica (the extents are CurvedExtents in
-- TileBoundaryTests). Points 1e-7 degree inside every edge of every tile of the second
-- level, so every point lies close to an edge at the root, at level 1 and at level 2;
-- spaced so they fall between the points a segmentized edge is transformed at.
CREATE FUNCTION pg_temp.edge_points_of(xmin float8, ymin float8, xmax float8, ymax float8, srid int)
RETURNS TABLE (id bigint, geom geometry) AS $$
    WITH t AS (
        SELECT xmin + (xmax - xmin) * kx / 4.0 AS x0, xmin + (xmax - xmin) * (kx + 1) / 4.0 AS x1,
               ymin + (ymax - ymin) * ky / 4.0 AS y0, ymin + (ymax - ymin) * (ky + 1) / 4.0 AS y1
        FROM generate_series(0, 3) kx, generate_series(0, 3) ky)
    SELECT row_number() OVER (), ST_Transform(ST_SetSRID(ST_MakePoint(lon, lat), 4326), srid)
    FROM (
        SELECT x0 + (x1 - x0) * (i + 0.371) / 256.0 AS lon, y + s * 1e-7 AS lat
        FROM t CROSS JOIN generate_series(0, 255) i
             CROSS JOIN LATERAL (VALUES (y0, 1), (y1, -1)) AS e(y, s)
        UNION ALL
        SELECT x + s * 1e-7, y0 + (y1 - y0) * (i + 0.371) / 256.0
        FROM t CROSS JOIN generate_series(0, 255) i
             CROSS JOIN LATERAL (VALUES (x0, 1), (x1, -1)) AS e(x, s)
    ) p;
$$ LANGUAGE sql;
CREATE TABLE europe_laea AS SELECT * FROM pg_temp.edge_points_of(-20, 27, 45, 72, 3035);
CREATE TABLE arctic AS SELECT * FROM pg_temp.edge_points_of(-180, 60, 180, 89, 3413);
CREATE TABLE antarctic_sector AS SELECT * FROM pg_temp.edge_points_of(-100, -89, 80, -60, 3031);

-- Every table gets the spatial index pg2b3dm asks for, and a copy as 2 cm squares
-- around its points, for the tiler, which reads polygons.
DO $$
DECLARE t text;
BEGIN
    FOREACH t IN ARRAY array['edge_points', 'edge_points_utm', 'edge_points_etrs89', 'edge_points_dhdn',
        'on_edge_points', 'edge_points_kept', 'across_meridian', 'extent_25km', 'extent_200km',
        'europe_laea', 'arctic', 'antarctic_sector'] LOOP
        EXECUTE format('CREATE TABLE %I AS SELECT id, ST_Multi(ST_Force3DZ(ST_Expand(geom, 0.01), 0))::geometry(MultiPolygonZ) AS geom FROM %I', t || '_squares', t);
        EXECUTE format('CREATE INDEX ON %I USING gist (st_centroid(st_envelope(geom)))', t);
        EXECUTE format('CREATE INDEX ON %I USING gist (st_centroid(st_envelope(geom)))', t || '_squares');
        EXECUTE format('ANALYZE %I', t);
    END LOOP;
END $$;
