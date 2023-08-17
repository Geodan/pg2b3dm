# pg2b3dm
 
 ![Build status](https://github.com/Geodan/pg2b3dm/workflows/.NET%20Core/badge.svg)[![Nuget](https://img.shields.io/nuget/vpre/pg2b3dm)](https://www.nuget.org/packages/pg2b3dm)
 [![Join the chat at https://discord.gg/gGCka4Nd](https://img.shields.io/discord/1013017110814932993?color=%237289DA&label=pg2b3dm&logo=discord&logoColor=white)](https://discord.gg/uSKvUwPgmG)

 Tool for converting 3D geometries from PostGIS to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles)/b3dm tiles. The generated 
 3D Tiles can be visualized in Cesium JS, Cesium for Unreal, Cesium for Unity3D, Mapbox GL JS v3 beta (experimental) or other 3D Tiles client viewers.

![image](https://user-images.githubusercontent.com/538812/227500590-bebe59b6-5697-462d-9ebd-b40fe9a2dc2b.png)

Features:

- 3D Tiles 1.1 Implicit tiling;

- Valid glTF 2.0 files;

- Shading PbrMetallicRoughness and PbrSpecularGlossiness;

- LOD support;

- Query parameter support;

- Outlines support (using CESIUM_primitive_outline);

- Docker support.

Resulting tilesets are validated against 3D Tiles Validator (https://github.com/CesiumGS/3d-tiles-validator).

To run this tool there must be a PostGIS table available containing triangulated polyhedralsurface geometries. Those geometries can be created 
by FME (using Triangulator transformer - https://www.safe.com/transformers/triangulator/) or custom tesselation tools.

Tileset.json and b3dm tiles are by default created in the 'output/content' subdirectory (or specify output directory with   -o, --output).

## Getting started

See [getting started](getting_started.md) for a tutorial how to convert a shapefile of buildings to 3D Tiles and visualize in CesiumJS/Cesium for Unreal.

For a dataprocessing workflow from CityGML to 3D Tiles using GDAL, PostGIS and FME see [dataprocessing/dataprocessing_citygml](dataprocessing/dataprocessing_citygml.md).

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

  --sql_command_timeout           (Default: 30) SQL command timeout

  --default_color                 (Default: #FFFFFF) Default color

  --default_metallic_roughness    (Default: #008000) Default metallic roughness

  --max_features_per_tile         (Default: 1000) maximum features per tile (Cesium)

  -l, --lodcolumn                 (Default: '') LOD column (Cesium)

  -g, --geometricerrors           (Default: 2000,0) Geometric errors (Cesium)

  --shaderscolumn                 (Default: '') shaders column (Cesium)

  --use_implicit_tiling           (Default: true) use 1.1 implicit tiling (Cesium)

  --boundingvolume_heights        (Default: 0,100) Tile boundingVolume heights (min, max) in meters (Cesium)

  --add_outlines                  (Default: false) Add outlines (Cesium)

  --min_zoom                      (Default: 15) Minimum zoom level (Mapbox)

  --max_zoom                      (Default: 15) Maximum zoom level (Mapbox)

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

## Experimental MapBox GL JS Support

In release 1.6 experimental support for MapBox GL JS v3 beta is added.

See announcement https://www.mapbox.com/blog/standard-core-style

To get things running (see also https://github.com/Geodan/pg2b3dm/blob/master/getting_started.md):

- Create EPSG:3857 triangulated geometries in your database;

- Run pg2b3dm 1.6 or higher, use Mapbox specific parameters min_zoom (default 15)/ max_zoom (default 15);

- Tiles are written in format {z}-{x}-{y}.b3dm in the content directory;

- Add the resulting tiles as 'batched-model' to your Mapbox GL JS v3 beta viewer

Sample code:

```
map.addSource('3d tiles', {
        "type": "batched-model",
        "maxzoom": 15,
        "minzoom": 15,
        "tiles": [
          "http://localhost:2015/content/{z}-{x}-{y}.b3dm"
        ]
      }
)});
```

Note: In the currrent MapBox GL JS 3 beta (v3.0.0-beta.1) version the tiles are requested but the glTF's are not rendered correct yet.

Live demo see https://geodan.github.io/pg2b3dm/sample_data/delaware/mapboxv3/ (zoom in a bit)

## Styling

For styling see [styling 3D Tiles](styling.md) 

## Remarks

### Geometries

- All geometries must be type polyhedralsurface consisting of triangles with 4 vertices each. If not 4 vertices exception is thrown.

For large datasets create a spatial index on the geometry column:

```
psql> CREATE INDEX ON the_table USING gist(st_centroid(st_envelope(geom_triangle)));
```

In release 1.6.2 a check is added for the spatial index. If the spatial index is not present the following warning is shown.

![image](https://user-images.githubusercontent.com/538812/261248327-c29b4520-a374-4441-83bf-2b60e8313c65.png)

### LOD

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

The generated files (for example '4_6_8_0.b3dm') will have 4 parameters (x, y,z and lod). All the tiles will be included in the tileset.json, 
corresponding with the calculated geometric error.

Notes:

- if there are no features within a tile boundingbox, the tile (including children) will not be generated. 

- LOD function is not available when implicit tiling is used.

### Geometric errors

By default, as geometric errors [2000,0] are used (for 1 LOD). When there multiple LOD's, there should be number_of_lod + 1 geometric errors specified in the -g option. 
When using multiple LOD and the -g option is not specified, the geometric errors are calculated using equal intervals 
between 2000 and 0.
When using implicit tiling only the first value of the geometric errors is used, the rest is automatically calculated by implicit tiling.

### Query parameter

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

### Attributes

With the -a attributecolumns parameter multiple columns with attributes can be specified. The attribute information is stored in the b3dm batch table. 
Multiple columns must be comma separated:

Sample:  --attributescolumns col1,col2

Attribute columns can be of any type.

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

Sample on Windows: 

```
$ docker run -v C:\output:/app/output -it geodan/pg2b3dm -h my_host -U my_user -d my_database -t my_table
```

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

- SharpGLTF (https://github.com/vpenades/SharpGLTF) for generating glTF;

- CommandLineParser (https://github.com/commandlineparser/commandline) for parsing command line options;

- Npgsql (https://www.npgsql.org/) - for access to PostgreSQL;

- b3dm-tile (https://github.com/bertt/b3dm-tile-cs) - for generating b3dm files;

- Wkx (https://github.com/cschwarz/wkx-sharp) - for geometry handling.

- Subtree (https://github.com/bertt/subtree) - for subtree file handling

## History

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
