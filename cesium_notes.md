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

- In the root tileset.json the maximum geometric error is used. For lower levels the geometric error is calculated based on the parent
- geometric error and the geometric error factor. For example when the geometric error is 2000 and the geometric error factor is 2, the geometric error for the children will be 1000.

- When using larger geometries (that intersect partly with a quadtree tile) and implicit tiling there can be issues with feature visibility.

For more information about Implicit Tiling see https://github.com/CesiumGS/3d-tiles/tree/draft-1.1/specification/ImplicitTiling

## LOD

With the LOD function there can be multiple representations of features depending on the distance to the camera (geometric error). So 
for example visualize a simplified geometry when the camera is far away, and a more detailed geometry when the camera is close.

Sample command for using LOD's using the delaware_buildings_lod table (see script 2_create_delaware_table.sql script in the database test project):

```
-h localhost -U postgres -p 5432 -c geom_triangle  -t delaware_buildings_lod -d postgres -g 2000 --shaderscolumn shaders --lodcolumn lodcolumn --use_implicit_tiling false -r REPLACE --geometricerrorfactor 8
```

The LOD function will be enabled when parameter --lodcolumn is not empty.

The LOD column in the database contains integer LOD values (like 0,1). First the program queries the distinct values of 
the LOD column.

For each LOD the program will generate a tile (when there are features available). 

The generated files (for example '4_6_8_0.b3dm') will have 4 parameters (z, x, y and lod). All the tiles will be included in the tileset.json, 
corresponding with the calculated geometric error.

Demo 2 LODS https://bertt.github.io/cesium_3dtiles_samples/samples/lod_bag3d/

![bag_lods](https://github.com/Geodan/pg2b3dm/assets/538812/cc5bd11e-0302-4271-b39d-7065b98177ba)

Notes:

- if there are no features within a tile boundingbox, the tile (including children) will not be generated. 

- LOD function is not available when implicit tiling is used.


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
