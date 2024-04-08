# pg2b3dm
 
 ![Build status](https://github.com/Geodan/pg2b3dm/actions/workflows/main.yml/badge.svg)[![Nuget](https://img.shields.io/nuget/vpre/pg2b3dm)](https://www.nuget.org/packages/pg2b3dm)
[![NuGet](https://img.shields.io/nuget/dt/pg2b3dm.svg)][![Join the chat at https://discord.gg/gGCka4Nd](https://img.shields.io/discord/1013017110814932993?color=%237289DA&label=pg2b3dm&logo=discord&logoColor=white)](https://discord.gg/uSKvUwPgmG)

 Tool for converting 3D geometries from PostGIS to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles). The generated 
 3D Tiles can be visualized in Cesium JS, Cesium for Unreal, Cesium for Unity3D, Cesium for Omniverse, QGIS, ArcGIS Pro, ArcGIS Maps SDK for JavaScript, Mapbox GL JS v3 (experimental) or other 3D Tiles client viewers.

![image](https://user-images.githubusercontent.com/538812/227500590-bebe59b6-5697-462d-9ebd-b40fe9a2dc2b.png)

Features:

- 3D Tiles 1.1 Implicit tiling;

- 3D Tiles extensions EXT_Mesh_Features and EXT_Structural_Metadata; 

- Valid glTF 2.0 files;

- Shading PbrMetallicRoughness and PbrSpecularGlossiness;

- Query parameter support;

- Cesium: LOD support and Outlines support (using CESIUM_primitive_outline);

- Triangulation of input geometries LineStrings/Polygon/MultiPolygon/PolyhedralSurface with Z values;

- Docker support.

Resulting tilesets can be validated against 3D Tiles Validator (https://github.com/CesiumGS/3d-tiles-validator).

Tileset.json and glb/b3dm tiles are by default created in the 'output/content' subdirectory (or specify output directory with   -o, --output).

## Getting started

Convert 3D Data (Multipolygon Z) to 3D Tiles

- Download Geopackage from https://3dbag.nl/, for example Sibbe [https://3dbag.nl/nl/download?tid=7-688-32](https://3dbag.nl/nl/download?tid=7-688-32)

Result: 7-688-32.gpkg (34 MB)

- Import in PostGIS database, convert to EPSG:4979 (WGS84 ellipsoidal heights). Note: in the Cesium client viewer the terrain should be added to see the buildings on the correct height.

```
$ ogr2ogr -f PostgreSQL pg:"host=localhost user=postgres password=postgres" -t_srs epsg:4979 7-688-32.gpkg lod22_3d
```

When the terrain is not used, omit the -t_srs parameter (in this case the Dutch EPSG code EPSG:7415 of the input data will be used).

- Convert to 3D Tiles using pg2b3dm

```
$ pg2b3dm -h localhost -U postgres -c geom -d postgres -t lod22_3d -a identificatie
```

- The resulting tileset can be added to CesiumJS using:

```
   const tileset = await Cesium.Cesium3DTileset.fromUrl(
      "./1.1/tileset.json"
    );  
    viewer.scene.primitives.add(tileset);
```

- The Dutch terrain can be added in CesiumJS using:

```
var terrainProvider = await Cesium.CesiumTerrainProvider.fromUrl('https://api.pdok.nl/kadaster/3d-basisvoorziening/ogc/v1_0/collections/digitaalterreinmodel/quantized-mesh');
viewer.scene.terrainProvider = terrainProvider;
viewer.scene.globe.depthTestAgainstTerrain=true;
```

- Load 3D Tiles in Cesium viewer, example result see https://geodan.github.io/pg2b3dm/sample_data/3dbag/sibbe/  

Older getting started documents:

1] See [getting started](getting_started.md) for a tutorial how to convert a 2D shapefile of buildings with height attribute to 3D Tiles and visualize in CesiumJS/Cesium for Unreal/Unity3D.

2] For a dataprocessing workflow from CityGML to 3D Tiles using GDAL, PostGIS and FME see [dataprocessing/dataprocessing_citygml](dataprocessing/dataprocessing_citygml.md).

## Demo

![Alt Text](demo_pg2b3dm.gif)

## Live Sample viewers

- 3D Bag by tudelftnl - 10 million Dutch buildings in 3D Tiles https://3dbag.nl/ 

![image](https://user-images.githubusercontent.com/538812/194698535-5b324133-bdf1-4d8c-8d53-37555a6f7b5b.png)

- FOSS4G presentations

Presentation at FOSS4G 2021: A fast web 3D viewer for 11 million buildings https://www.youtube.com/watch?v=1_JM2Xf5mDk

Presentation at FOSS4G 2019: 3D geodata in the MapBox GL JS viewer with 3D Tiles https://www.youtube.com/watch?v=HXQJbyEnC9w

- Texel - 3D Terrain, subsurface and buildings in MapBox GL JS: http://beta.geodan.nl/mapbox3d

![texel](https://user-images.githubusercontent.com/538812/77528003-74f6d900-6e8d-11ea-968e-5c510b6a1ad3.png)

- GeoTop Subsurface in MapBox GL JS: https://geodan.github.io/pg2b3dm/sample_data/geotop/mapbox/

- Amsterdam Buildings in MapBox GL JS: https://geodan.github.io/pg2b3dm/sample_data/amsterdam/mapbox/

- Amsterdam Buildings in Cesium: https://geodan.github.io/pg2b3dm/sample_data/amsterdam/cesium/

- Dover - Delaware buildings in MapBox GL JS: https://geodan.github.io/pg2b3dm/sample_data/delaware/mapbox/

- Dover - Delaware buildings in Cesium: https://geodan.github.io/pg2b3dm/sample_data/delaware/cesium/

- Duisburg buidings converted from CityGML in MapBox GL JS - https://geodan.github.io/pg2b3dm/sample_data/duisburg/mapbox/#15.62/51.430166/6.782675/0/45

## Command line options

All parameters are optional, except the -t --table option. 

If --username and/or --dbname are not specified the current username is used as default.

```
  -U, --username                  Database user

  -h, --host                      (Default: localhost) Database host

  -d, --dbname                    Database name

  -c, --column                    (Default: geom) Geometry column

  -t, --table                     Required. Database table, include database schema if needed

  -p, --port                      (Default: 5432) Database port

  -o, --output                    (Default: output) Output path

  -a, --attributecolumns          (Default: '') Attribute columns

  -q, --query                     (Default: '') Query parameter

  --copyright                     (Default: '') glTF asset copyright

  --default_color                 (Default: #FFFFFF) Default color

  --default_metallic_roughness    (Default: #008000) Default metallic roughness

  --double_sided                  (Default: true) Default double sided

  --create_gltf                   (Default: true) Create glTF files

  --radiuscolumn                  (Default: '') Column with radius values for lines

  --format                        (Default: Cesium) Application mode (Cesium/Mapbox)

  --max_features_per_tile         (Default: 1000) maximum features per tile (Cesium)

  -l, --lodcolumn                 (Default: '') LOD column (Cesium)

  -g, --geometricerrors           (Default: 2000,0) Geometric errors (Cesium)

  --shaderscolumn                 (Default: '') shaders column (Cesium)

  --use_implicit_tiling           (Default: true) use 1.1 implicit tiling (Cesium)

  --add_outlines                  (Default: false) Add outlines (Cesium)

  -r, --refinement                (Default: REPLACE) Refinement ADD/REPLACE (Cesium)

  --zoom                          (Default: 15) Zoom level (Mapbox)

  --help                          Display this help screen.

  --version                       Display version information.
```

Sample command for running pg2b3dm:

```
-h localhost -U postgres -c geom_triangle --shaderscolumn shaders -t delaware_buildings -d postgres -g 100,0 
```

## Installation


Prerequisite: .NET 6.0 SDK is installed https://dotnet.microsoft.com/download/dotnet/6.0

```
$ dotnet tool install -g pg2b3dm
```

Or update

```
$ dotnet tool update -g pg2b3dm
```

To run:

```
$ pg2b3dm
```

## Benchmarking

| Source                   | Table    |  Size     | Features     | Time         | Tiles  | Tiles/Minute   |
|--------------------------|----------|-----------|--------------|--------------|--------|----------------|
| Dutch 3d BAG buildings   | lod12_3d | 12 GB     | 9.712.728    |   1h 54m 23s |  29098 | 255            |

## Styling

For styling see [styling 3D Tiles](styling.md) 

## Geometries

Input geometries must be of type LineString/MultilineString/Polygon/MultiPolygon/PolyhedralSurface (with z values). When the geometry is not triangulated, pg2b3dm will perform
triangulation. Geometries with interior rings are supported.

For large datasets create a spatial index on the geometry column:

```
psql> CREATE INDEX ON the_table USING gist(st_centroid(st_envelope(geom_triangle)));
```

When there the spatial index is not present the following warning is shown.

![image](https://user-images.githubusercontent.com/538812/261248327-c29b4520-a374-4441-83bf-2b60e8313c65.png)

For line geometries a 3D tube is created with a radius of 1 meter. When a radius column is specified (option --radiuscolumn), the radius from that columns is 
used for the tube. The radius column must be of type 'real', sample for random radius between 0.5 and 1.5:

```
postgresql> alter table delaware_buildings add column radius real;
postgresql> update delaware_buildings set radius = 0.5 + random() * (1.5 - 0.5);
```

Sample with pipes (green = data, blue = water, purple = sewage, yellow = gas, red = electricity):

![image](https://github.com/Geodan/pg2b3dm/assets/538812/20280276-02a2-41f1-8b3d-4a893eb82db3)

## Query parameter

The -q --query will be added to the 'where' part of all queries. 

Samples:

Attribute query:

```
-q "ogc_fid=118768"
```

Spatial query:

```
-q "ST_Intersects(wkb_geometry, 'SRID=4326;POLYGON((-75.56996406 39.207228824,-75.56996406 39.2074420320001,-75.5696300339999 39.2074420320001,-75.5696300339999 39.207228824,-75.56996406 39.207228824))'::geometry)"
```

Make sure to check the indexes when using large tables.

## Attributes

With the -a attributecolumns parameter multiple columns with attributes can be specified. The attribute information is stored in the b3dm batch table or in the glTF 
(using EXT_Structural_Metadata extension).

Multiple columns must be comma separated (without spaces):

Sample:  --attributescolumns col1,col2

When using 3D TIles 1.1 and EXT_Structural_Metadata, the following mapping between PostgreSQL data types and 3D Tiles data types is used:

| PostgreSQL data type | 3D Tiles data type  (type / componenttype) |
|----------------------|--------------------|
| boolean | boolean / - |
| smallint | scalar / int16 |
| integer | scalar / int32 |
| bigint | scalar / int64 |
| real | scalar / float32 |
| numeric | scalar / float32 |
| double precision | scalar / float64 |
| numeric[] all of length 3 | vec3 / float32 |
| numeric[] all of length 16 | mat4 / float32 |
| numeric[] | scalar / float32 |
| varchar | string |

When one of the above types (except boolean and array types) is set to NULL in the database, the null values are converted
to a nodata value to be used in the 3D Tiles batch table.

Also arrays of the above types are supported, like: 

bool[], smallint[], int[], bigint[], real[], numeric[][], double precision[] and varchar[]

Arrays can be of fixed length or not.

When the type is numeric[]/numeric[][], it is checked if all the items contain 3 (vector3) or 16 values (mat4)
. If so, the vec3 or mat4 type is used.

Null values are not supported in arrays (including vector3 and matrix types).

When other types are used, there will be a exception.

Example creating string values in a column:

```
postgresql> alter table delaware_buildings add column random_string varchar not null default 'standaard waarde'
```

and for an array of strings:

```
postgresql> ALTER TABLE delaware_buildings  ADD COLUMN random_strings VARCHAR[] DEFAULT '{waarde1, waarde2}'
```

In the options you can now specify the column name 'random_string' and/or 'random_strings' to add the values.


## Cesium support

For Cesium support (tiling schema, LODS, outlines) see [Cesium notes](cesium_notes.md) 

## Mapbox support

MapBox GL JS v3 (experimental) support is available in this version.

Use parameter "-f Mapbox" to create tiles for Mapbox.

Tiles are written in format {z}-{x}-{y}.b3dm or {z}-{x}-{y}.glb in the content directory.

The tiles should be Draco compressed, for example use gltf-pipeline (https://github.com/CesiumGS/gltf-pipeline)

To load the tiles in Mapbox GL JS v3 (v3.2.0) use the following code:

```
ap.on('style.load', () => {

map.addSource('bag-3d', {
        "type": "batched-model",
        "maxzoom": 15,
        "minzoom": 15,
        "tiles": [
          "{url_to_tiles}/content/{z}-{x}-{y}.glb"
        ]
      }
)});

// add the custom style layer to the map
map.on('style.load', () => {
  map.addLayer({
    id: 'bag-layer',
    type: 'model',
    source: 'bag-3d',          
  });
});


```

For previous Mapbox support notes see [Mapbox notes](mapbox_notes.md) 

## ArcGIS Pro support

In ArcGIS Pro 3.2 support for 3D Tiles is added (https://pro.arcgis.com/en/pro-app/latest/help/mapping/layer-properties/work-with-3d-tiles-layers.htm)

Sample: Use option 'Data from path' with  https://geodan.github.io/pg2b3dm/sample_data/3dbag/sibbe/1.0/tileset.json

![image](https://github.com/Geodan/pg2b3dm/assets/538812/bf82df73-781c-41a4-97f2-a26c601a78ec)

![image](https://github.com/Geodan/pg2b3dm/assets/538812/ad3332c7-1a95-46f2-bcce-92a5e10ceccc)


## QGIS support

In QGIS 3.34 support for 3D Tiles is added see https://cesium.com/blog/2023/11/07/qgis-now-supports-3d-tiles/

To create 3D Tiles for QGIS use parameters '--create_gltf false --use_implicit_tiling false' as 3D Tiles 1.1 features are not supported yet. 

Sample dataset Sibbe https://geodan.github.io/pg2b3dm/sample_data/3dbag/sibbe/1.0/tileset.json

![image](https://github.com/Geodan/pg2b3dm/assets/538812/a89e531c-6aa5-4f0b-b7ae-35f43ee52ef8)


## Game engines Unity3D / Unreal / Omniverse support

To create 3D Tiles for game engines use parameters '--create_gltf false --use_implicit_tiling false' as 3D Tiles 1.1 features are not supported yet.

Sample dataset Sibbe: https://geodan.github.io/pg2b3dm/sample_data/3dbag/sibbe/1.0/tileset.json

## Run from Docker

Docker image: https://hub.docker.com/repository/docker/geodan/pg2b3dm

Tags used (https://hub.docker.com/repository/docker/geodan/pg2b3dm/tags): 

- {version}: specific version

- latest: is build automatically after push to master


### Building Dockers

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src
$ docker build -t geodan/pg2b3dm .
```

Test feature branch:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ git checkout {name_of_feature_branch}
$ cd pg2b3dm/src
$ docker build -t geodan/pg2b3dm:{name_of_feature_branch} .
```

### Running

Sample on Linux:

```
$ docker run -v $(pwd)/output:/app/output -it geodan/pg2b3dm -h my_host -U my_user -d my_database -t my_schema.my_table
```

## Run from source

Requirement: Install .NET 6.0 SDK

https://dotnet.microsoft.com/download/dotnet/6.0

Installation guide see https://docs.microsoft.com/en-us/dotnet/core/install/

To run the app:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src/pg2b3dm
$ dotnet run -- -h my_host -U my_user -d my_database -t my_schema.my_table
```

To create an self-contained executable '~/bin/pg2b3dm' for Linux:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src/pg2b3dm
$ dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true
$ cp ./bin/Release/net6.0/linux-x64/publish/pg2b3dm ~/bin
$ ~/bin/pg2b3dm
```

Alternative options for parameter -r in dotnet publish: 'osx-x64' (Mac), 'win-x64' (Windows)

## Debugging in Visual Studio Code

```
$ git clone https://github.com/Geodan/pg2b3dm.git

$ cd src

$ code .
```

In Visual Studio Code, open .vscode/launch.json and adjust the 'args' parameter to your environment

```
"args": ["-h" ,"my_host", "-U" ,"my_user", "-d", "my_database", "-t", "my_table"],            
```

Press F5 to start debugging.

## Dependencies

- b3dm-tile (https://github.com/bertt/b3dm-tile-cs) - for generating b3dm files;

- CommandLineParser (https://github.com/commandlineparser/commandline) for parsing command line options;

- Npgsql (https://www.npgsql.org/) - for access to PostgreSQL;

- SharpGLTF (https://github.com/vpenades/SharpGLTF) for generating glTF;

- Subtree (https://github.com/bertt/subtree) - for subtree file handling

- Triangulator (https://github.com/bertt/triangulator) - for triangulating geometries

- Wkx (https://github.com/cschwarz/wkx-sharp) - for geometry handling.

## History

2024-03-07: release 2.7.0, create more tileset.json files with explicit tiling + change spatial index check + performance improvement count features

2024-03-20: release 2.6.1, fix z of boundingvolumes

2024-03-06: release 2.6.0, add support for Mapbox v3 (experimental), added parameter --format (default Cesium) Cesium/Mapbox

2024-02-20: release 2.5.1, add support for multiline strings

2024-02-15: release 2.5.0 

- add support for single shaders per geometry https://github.com/Geodan/pg2b3dm/pull/147

- add lines support, added option --radiuscolumn https://github.com/Geodan/pg2b3dm/pull/146
 
- update triangulator for higher precision normals calculation

2024-02-08: release 2.4.0, add support for polygons with interior rings

2024-01-26: release 2.3.0, add support for null values in attribute columns (except array types)

2024-01-26: release 2.2.1, fix for degenerated triangles

2024-01-25: release 2.2.0, add support for 3D Tiles 1.1 all EXT_Structural_Metadata types + create tileset.json file with 
version 1.0 when create_gltf is false and use_implicit_tiling is false

2024-01-23: release 2.1.0, fix of offsets + add polygonZ support

2024-01-10: release 2.0.1, fix for triangulator + add check on interrior rings (not supported)

2024-01-03: release 2.0.0, 

- Breaking change: removed input coordinate system requirement (EPSG:4978), use EPSG:4326/EPSG:4979 or local coordinate system instead. 

- glTF transformation is defined in tileset.json (instead of in glTF asset). As a result, the glTF assets are no longer 'skewed' when visualized in a glTF viewer.

- removed parameter 'boundingvolume_heights', heights are calculated from the input data 

2023-11-13: release 1.8.5, fix for dataset with geometries on 1 location

2023-10-25: release 1.8.4, add -r --refinement option

2023-10-17: release 1.8.3, tileset.json asset version from 1.0 to 1.1, database connection timeout removed

2023-10-04: release 1.8.2, use humanizer with resources 

2023-09-26: release 1.8.1, updating triangulator 

2023-09-22: release 1.8, adding 3D Tiles 1.1 Metadata support (EXT_Mesh_Features / EXT_Structural_Metadata). Options added: create_gltf (default true), double_sided (default true)

2023-08-29: release 1.7.1, improve spatial index check

2023-08-29: release 1.7.0, add triangulator - runs only when geometry is not triangulated

2023-08-29: release 1.6.3, add support for MultiPolygonZ

2023-08-17: release 1.6.2, add check for spatial index

2023-08-16: release 1.6.1, translate b3dm's to center of tile for Mapbox GL JS v3

2023-08-16: release 1.6.0, add experimental support for Mapbox GL JS v3

2023-06-20: release 1.5.5, fix issue when only 1 level is generated

2023-04-06: release 1.5.3, fix disappearing features

2023-04-04: release 1.5.2, fix query parameter

2023-03-27: release 1.5.1, add outlines support for multiple shaders

2023-03-15: release 1.5.0, adding options 'add_outlines' (default false) and 'default_color' (#FFFFFF)

2023-02-16: release 1.4.3, fix for implicit tiling - missing b3dm's on high z-levels

2023-02-02: release 1.4.2, fix subtree files generation

2023-02-01: release 1.4.1, fix global tool

2023-02-01: release 1.4, adding tree of subtree files support

2023-01-10: release 1.3, adding LOD support

2022-12-13: release 1.2.3, fixing parameter use_implicit_tiling

2022-08-30: release 1.2.2, fixing initial boundingbox issue

2022-08-29: release 1.2.1 

- Fixing debug boundingVolumes and query parameter;

- Option 'use_implicit_tiling' default value changed from False to True;

2022-08-24: release 1.1: adding parameters sql_command_timeout (default: 30 seconds) and boundingvolume_heights (default: 0,100)

2022-08-23: release 1.0

Use a quadtree tiling method by default, fix skewed bounding volumes in Cesium.

MapBox GL JS support is discontinued at the moment.

Breaking changes:

- removed: parameter -i, --idcolumn

- removed: parameter -e, --extenttile

- renamed: parameter implicit_tiling_max_features to max_features_per_tile

2022-08-09: release 0.16, fixing materials (MetallicRoughness and SpecularGlossiness)

2022-08-09: release 0.15, use 1 geometric error for implicit tiling

2022-07-20: release 0.14, adding 3D Tiles 1.1 implicit tiling option

2022-07-05: release 0.13, adding glTF asset copyright

2022-01-24: release 0.12, to .NET 6, fixing decimal symbols regional settings on Windows

2021-10-27: release 0.11.2, fixing non latin characters issue in batch table

2021-09-30: release 0.11, adding multiple attribute columns support. 0.11.1 contains bug fix for batch table length 

2020-11-17: release 0.10, adding shader support PbrMetallicRoughness and PbrSpecularGlossiness + to .NET 5.0

2020-06-18: release 0.9.4, adding query parameter support (-q --query)

2020-05-07: release 0.9.3, rewriting tiling method 

2019-11-18: release 0.8 adding -f, --featurespertile and -e, --extenttile options

2019-10-02: release 0.7 adding id column option (default 'id')

2019-09-02: release 0.6 adding batching option on single column (-a option)

2019-08-21: release 0.5.1 with fix for non trusted Postgres connection

2019-08-20: release 0.5 adds support for multiple colors

2019-08-15: release 0.4.4 improving roof colors

2019-08-15: release 0.4.3 change degenerated triangles detection + removal

2019-08-14: release 0.4.2 fixing roof colors + filter very small triangles (<0.01)

2019-08-13: release 0.4.1 with fix for roof colors (option -r)

2019-08-12: release 0.4 adding roof color column option (-r)

2019-08-01: release 0.3.3 with 2 colors

2019-07-09: release 0.3 using library SharpGLTF

2019-06-01: release 0.2.1 with some small fixes

2019-06-01: initial release 0.2

2019-05-01: initial release 0.1
