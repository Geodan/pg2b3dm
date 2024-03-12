# Cesium specific notes

## Tiling method

Tiles are created within a quadtree, with a maximum number of features by max_features_per_tile (default 1000). In pg2b3dm version 0.14 support for 3D Tiles
1.1 Impliciting Tiling is added. Impliciting Tiling can be activated using the parameter 'use_implicit_tiling' (default value 'true'). When Impliciting Tiling 
is activated subtree files (*.subtree) will be created (in folder subtrees) and the tileset.json file will no longer explitly list all tiles.

At the moment, Implicit tiling is only supported in the CesiumJS client.

Some remarks about implicit tiling:

- There is no support (yet) for creating octree instead of quadtree;

- There is no support (yet) for multiple contents per tile;

- There is no support (yet) for implicit tiling metadata;

- Parameter '-l --lodcolumn' is ignored when using implicit tiling;

- Only the first value of parameter of geometric errors is used in tileset.json;

- When using larger geometries (that intersect partly with a quadtree tile) and implicit tiling there can be issues with feature visibility.

For more information about Implicit Tiling see https://github.com/CesiumGS/3d-tiles/tree/draft-1.1/specification/ImplicitTiling

## LOD

With the LOD function there can be multiple representations of features depending on the distance to the camera (geometric error). So 
for example visualize a simplified geometry when the camera is far away, and a more detailed geometry when the camera is close.

Sample command for using LOD's:

```
-h localhost -U postgres -c geom_triangle --shaderscolumn shaders -t delaware_buildings_lod -d postgres -g 1000,100,0 --lodcolumn lodcolumn --use_implicit_tiling false --max_features_per_tile 1000
```

The LOD function will be enabled when parameter --lodcolumn is not empty.

The LOD column in the database contains integer LOD values (like 0,1). First the program queries the distinct values of 
the LOD column.

For each LOD the program will generate a tile (when there are features available). 

The generated files (for example '4_6_8_0.b3dm') will have 4 parameters (z, x, y and lod). All the tiles will be included in the tileset.json, 
corresponding with the calculated geometric error.

Demo 2 LODS https://bertt.github.io/cesium_3dtiles_samples/samples/lod_bag3d/

![bag_lods](https://github.com/Geodan/pg2b3dm/assets/538812/cc5bd11e-0302-4271-b39d-7065b98177ba)

How to create Cesium viewer with multiple lods for Dutch Bag

1] Download geopackage

https://data.3dbag.nl/v20240228/tiles/7/512/560/7-512-560.gpkg

2] Inspect

Contains 

lod12_3d
lod13_3d
lod22_3d

3] Load in PostGIS


$ ogr2ogr -f PostgreSQL pg:"host=localhost user=postgres password=postgres" -t_srs epsg:4979 7-512-560.gpkg lod12_3d -nln werkhoven_lod12

$ ogr2ogr -f PostgreSQL pg:"host=localhost user=postgres password=postgres" -t_srs epsg:4979 7-512-560.gpkg lod13_3d -nln werkhoven_lod13

$ ogr2ogr -f PostgreSQL pg:"host=localhost user=postgres password=postgres" -t_srs epsg:4979 7-512-560.gpkg lod22_3d -nln werkhoven_lod22

4] Combine the data in PostgreSQL

```
CREATE TABLE werkhoven_lods AS SELECT * FROM werkhoven_lod12 
alter table werkhoven_lods add lods decimal
update werkhoven_lods set lods=1

insert into werkhoven_lods(fid, identificatie, b3_pand_deel_id, labels, geom, lods)
select fid, identificatie, b3_pand_deel_id, labels, geom,  2 from werkhoven_lod13 

insert into werkhoven_lods(fid, identificatie, b3_pand_deel_id, labels, geom, lods)
select fid, identificatie, b3_pand_deel_id, labels, geom,  3 from werkhoven_lod22 
```

5] Tile the table

Note: 

- Use --use_implicit_tiling false

- Add  --lodcolumn lods

pg2b3dm -h localhost -U postgres -c geom -t werkhoven_lods -d postgres -a identificatie --lodcolumn lods --use_implicit_tiling false -g 2000,5,1,0

Tiles like 1_0_1_1.glb are created, last number is the lod level

6] View  https://bertt.github.io/cesium_3dtiles_samples/samples/lod_bag3d/

Notes:

- if there are no features within a tile boundingbox, the tile (including children) will not be generated. 

- LOD function is not available when implicit tiling is used.

## Geometric errors

By default, as geometric errors [2000,0] are used (for 1 LOD). When there multiple LOD's, there should be number_of_lod + 1 geometric errors specified in the -g option. 
When using multiple LOD and the -g option is not specified, the geometric errors are calculated using equal intervals 
between 2000 and 0.
When using implicit tiling only the first value of the geometric errors is used, the rest is automatically calculated by implicit tiling.


## Outlines

Outlines using glTF 2.0 extension CESIUM_primitive_outline can be drawn by setting the option 'add_outlines' to true. 

When enabling this 
function the extension 'CESIUM_primitive_outline' will be used in the glTF. The indices of vertices that should take part in outlining are stored 
in the glTF's. The CesiumJS client has functionality to read and visualize the outlines. 

In the CesiumJS client the outline color can be changed using the 'outlineColor' property of Cesium3DTileset:

```
tileset.outlineColor = Cesium.Color.fromCssColorString("#875217");
```

For more information about CESIUM_primitive_outline see https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Vendor/CESIUM_primitive_outline/README.md


When using Draco compression and Outlines there will be an error in the Cesium client: 'Cannot read properties of undefined (reading 'count')'
see also https://github.com/CesiumGS/gltf-pipeline/pull/631 

There is a workaround:

. Use gltf-pipeline with https://github.com/CesiumGS/gltf-pipeline/pull/631

. Use gltf-pipeline settings --draco.compressionLevel 0 --draco.quantizePositionBits 14
