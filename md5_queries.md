## Queries for MD5


### Initial

1] Get bounding box whole table (1.9 s)

```sql
SELECT st_xmin(geom1),st_ymin(geom1), st_xmax(geom1), st_ymax(geom1), st_zmin(geom1), st_zmax(geom1)  FROM (select st_transform(ST_3DExtent(geom), 4979) as geom1 from bertt.nantes_reconstructed_buildings ) as t
```
Result: 

```
-1.8471041030488762	47.14626298148698	-1.1473131952502678	47.62268076404559	34.427586472817715	475.03764899302183
```

## Tile 0_0_0.glb

2] Count geometries in bounding box (0.2s)

```sql
SELECT COUNT(geom) FROM bertt.nantes_reconstructed_buildings WHERE ST_Centroid(ST_Envelope(geom)) && st_transform(ST_MakeEnvelope(-1.847105103048876, 47.14626198148698, -1.1473121952502678, 47.62268176404559, 4326), 5698) 
```

Result: 385856

3] Get geometries for tile 0_0_0.glb - 1000 largest geometries in whole table (2 s)

```sql
SELECT ST_AsBinary(st_transform(geom, 4978)), id , MD5(ST_AsBinary(geom)::text) as geom_hash FROM bertt.nantes_reconstructed_buildings where ST_Centroid(ST_Envelope(geom)) && st_transform(ST_MakeEnvelope(-1.847105103048876, 47.14626198148698, -1.1473121952502678, 47.62268176404559, 4326), 5698)  ORDER BY ST_Area(ST_Envelope(geom)) DESC LIMIT 1000
```

md5 hashes (for example '9759cdee666f512a0c13df8245b667f9') are remembered to be excluded in higher level (z) tile

potential improvement: make exception for first tile on z=0 -  do not filter on envelope (all features are included)

## Tile 1_0_0.glb (level 1, x=0, y=0)

4] Filter the hashes from previous list to only geometries within tile 1_0_0

```sql
        SELECT MD5(ST_AsBinary(geom)::text) as geom_hash
        FROM bertt.nantes_reconstructed_buildings
        WHERE MD5(ST_AsBinary(geom)::text) = ANY($1)
        AND ST_Within(
            ST_Centroid(ST_Envelope(geom)),
            ST_Transform(ST_MakeEnvelope($2, $3, $4, $5, 4326), 5698)
        )
```

Note: Using parameterized query with array parameter instead of string concatenation.

5] Count geometries in bounding box on level 1 excluding the geometries from tile 0_0_0.glb, including only the geometries within the tile

```sql
SELECT COUNT(geom) FROM bertt.nantes_reconstructed_buildings WHERE ST_Centroid(ST_Envelope(geom)) && st_transform(ST_MakeEnvelope(-1.847105103048876, 47.14626198148698, -1.497208649149572, 47.384471872766284, 4326), 5698)  AND MD5(ST_AsBinary(geom)::text) != ALL($1)
```

Note: Using parameterized query with array parameter instead of string concatenation.

Result: 235787

6] Get geometries for tile 1_0_0.glb - 1000 largest geometries in tile 1_0_0

```sql
SELECT ST_AsBinary(st_transform(geom, 4978)), id , MD5(ST_AsBinary(geom)::text) as geom_hash FROM bertt.nantes_reconstructed_buildings where ST_Centroid(ST_Envelope(geom)) && st_transform(ST_MakeEnvelope(-1.847105103048876, 47.14626198148698, -1.497208649149572, 47.384471872766284, 4326), 5698)  AND MD5(ST_AsBinary(geom)::text) != ALL($1) ORDER BY ST_Area(ST_Envelope(geom)) DESC LIMIT 1000
```

Note: Using parameterized query with array parameter instead of string concatenation.

## Issue

List of hashes can get long (maximum z*1000 items). Previously this was handled with string concatenation which could lead to performance issues and potential SQL injection vulnerabilities.

**Solution**: Now using parameterized queries with PostgreSQL's `= ANY()` and `!= ALL()` operators for better performance and security.

## Spatial indexing

 Recommended Indexes

  1. Spatial Index with MD5 Hash (Composite)

    CREATE INDEX idx_geom_centroid_hash ON the_table
    USING btree(MD5(ST_AsBinary(geom_triangle)::text));

  2. Spatial Index (GIST) - Still Required

    CREATE INDEX idx_geom_centroid_spatial ON the_table
    USING gist(ST_Centroid(ST_Envelope(geom_triangle)));

  Rationale

  The queries now use three main patterns:

  1] Spatial filtering with MD5 hash exclusion (GetGeometrySubset):  WHERE ST_Centroid(ST_Envelope(geom_triangle)) && <envelope>
      AND MD5(ST_AsBinary(geom_triangle)::text) != ALL($1)
      
  2] MD5 hash filtering with spatial validation (FilterHashesByEnvelope):  WHERE MD5(ST_AsBinary(geom_triangle)::text) = ANY($1)
      AND ST_Within(ST_Centroid(ST_Envelope(geom_triangle)), <envelope>)
      
  3] Hash-only filtering (GetGeometriesBoundingBox):  WHERE MD5(ST_AsBinary(geom_triangle)::text) = ANY($1)

  Performance Notes:

  1] The GIST spatial index handles the ST_Centroid(ST_Envelope(geom_triangle)) predicates
  
  2] The MD5 hash BTREE index handles the MD5(ST_AsBinary(geom_triangle)::text) = ANY/!= ALL predicates
  
  3] PostgreSQL will use both indexes (bitmap index scan) for queries with both predicates
  
  4] Using parameterized queries with ANY/ALL operators provides better performance than string-concatenated IN/NOT IN clauses

  Optional: Materialized Hash Column

## Solution

The hash filtering now uses PostgreSQL's `= ANY(@param)` operator with array parameters instead of string concatenation:

1. **Hash Inclusion (IN clause)**: Changed from `MD5(...) IN ('hash1', 'hash2', ...)` to `MD5(...) = ANY(@hashes)` with parameterized array
2. **Hash Exclusion (NOT IN clause)**: Changed from `MD5(...) NOT IN ('hash1', 'hash2', ...)` to `MD5(...) != ALL(@excludeHashes)` with parameterized array

Benefits:
- Eliminates SQL injection risk (even though MD5 hashes are predictable)
- Better performance with large hash lists
- Cleaner, more maintainable code
- Proper use of parameterized queries

## Todo 

- ~~idea: make a temporary blacklist table with the to be exluded hashes?~~ (Solved using parameterized arrays)

- idea: force use of id column (longs)?

- ~~Other solutions?~~ (Implemented using `ANY` and `ALL` operators)
