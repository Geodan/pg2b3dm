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

## Tile 1_0_0.glb (level 1, x=0, y=0)

4] Count geometries in bounding box on level 1 excluding 1000 largest geometries from tile 0_0_0.glb (8 seconds!)

```sql
SELECT COUNT(geom) FROM bertt.nantes_reconstructed_buildings WHERE ST_Centroid(ST_Envelope(geom)) && st_transform(ST_MakeEnvelope(-1.847105103048876, 47.14626198148698, -1.497208649149572, 47.384471872766284, 4326), 5698)  AND MD5(ST_AsBinary(geom)::text) NOT IN ('9759cdee666f512a0c13df8245b667f9',..1000 items, ...)
```

Result: 235787

5] Get geometries for tile 1_0_0.glb - 1000 largest geometries in tile 1_0_0 (10 seconds!)

```sql
SELECT ST_AsBinary(st_transform(geom, 4978)), id , MD5(ST_AsBinary(geom)::text) as geom_hash FROM bertt.nantes_reconstructed_buildings where ST_Centroid(ST_Envelope(geom)) && st_transform(ST_MakeEnvelope(-1.847105103048876, 47.14626198148698, -1.497208649149572, 47.384471872766284, 4326), 5698)  AND MD5(ST_AsBinary(geom)::text) NOT IN ('9759cdee666f512a0c13df8245b667f9', ..1000 items, ...) ORDER BY ST_Area(ST_Envelope(geom)) DESC LIMIT 1000
```

Todo: 

- Check spatial indexes

- limit 'not in' list of md5 hashes from above level, e.g. only use the hashes of the geometries that intersect the tile envelope
