# pg2b3dm
 
 ![Build status](https://github.com/Geodan/pg2b3dm/workflows/.NET%20Core/badge.svg)[![Nuget](https://img.shields.io/nuget/vpre/pg2b3dm)](https://www.nuget.org/packages/pg2b3dm)
 [![Matrix](https://img.shields.io/matrix/3d-tiles:matrix.org.svg?style=flat)](https://matrix.to/#/#3d-tiles:matrix.org)


Tool for converting 3D geometries from PostGIS to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles)/b3dm tiles. This software started as a port of py3dtiles (https://github.com/Oslandia/py3dtiles) for generating b3dm tiles. 
The generated 3D Tiles can be visualized in Cesium JS/Cesium for Unreal or other 3D Tiles client viewer.

![mokum](https://user-images.githubusercontent.com/538812/63088752-24fa8000-bf56-11e9-9ba8-3273a21dfda0.png)

Differences to py3dtiles:

- performance improvements;

- memory usage improvements;

- added 3D Tiles 1.1 Implicit tiling support

- fixed glTF warnings;

- added colors option;

- added LOD support;

- added output directory option;

- added refinement method (add or replace) support;

- added glTF shader support for PbrMetallicRoughness and PbrSpecularGlossiness

- added query parameter support;

- Docker support.

To run this tool there must be a PostGIS table available containing triangulated polyhedralsurface geometries. Those geometries can be created 
by FME (using Triangulator transformer - https://www.safe.com/transformers/triangulator/) or custom tesselation tools.

Tileset.json and b3dm tiles are by default created in the 'output/content' subdirectory (or specify output directory with   -o, --output).

## Demo

![Alt Text](demo_pg2b3dm.gif)

## Live Sample viewers

- 3D Bag by tudelftnl - 10 million Dutch buildings in 3D Tiles https://3dbag.nl/ 

![image](https://user-images.githubusercontent.com/538812/194698535-5b324133-bdf1-4d8c-8d53-37555a6f7b5b.png)

- Baupotential analyse -  https://www.modoplus.de/

![image](https://user-images.githubusercontent.com/538812/194698451-b4c2b1ed-b99b-411c-97d3-34939d32d588.png)


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
  -U, --username         (Default: username) Database user

  -h, --host             (Default: localhost) Database host

  -d, --dbname           (Default: username) Database name

  -c, --column           (Default: geom) Geometry column name

  -t, --table            (Required) Database table name, include database schema if needed

  -o, --output           (Default: ./output/tiles) Output directory, will be created if not exists

  -p, --port             (Default: 5432) Database port

  -a, --attributecolumns (Default: '') attributes column names (comma separated)

  -g, --geometricerrors  (Default: 2000, 0) Geometric errors

  -q, --query            (Default: '') Query parameter

   --copyright           (Default: '') glTF copyright 

  --shaderscolumn        (Default: '') shaders column

  --use_implicit_tiling  (Default: True) Use 3D Tiles 1.1 Implicit tiling

  --max_features_per_tile (Default 1000) Maximum number of features per tile in 3D Tiles 1.1 Implicit tiling
  
  --sql_command_timeout  (Default: 30) Command timeout for database queries (in seconds)

  --boundingvolume_heights (Default: '0,100') Height of boundingVolume (min, max) in meters 
                         
  --help                Display this help screen.

  --version             Display version information.  
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

## Getting started

See [getting started](getting_started.md) for a tutorial how to run pg2b3dm and visualize buildings in MapBox GL JS or CesiumJS/Cesium for Unreal.

For a dataprocessing workflow from CityGML to 3D Tiles using GDAL, PostGIS and FME see [dataprocessing/dataprocessing_citygml](dataprocessing/dataprocessing_citygml.md).

## Remarks

### Geometries

- All geometries must be type polyhedralsurface consisting of triangles with 4 vertices each. If not 4 vertices exception is thrown.

For large datasets create a spatial index on the geometry column:

```
psql> CREATE INDEX ON the_table USING gist(st_centroid(st_envelope(geom_triangle)));
```

### LOD

- if there are no features within a tile boundingbox, the tile (including children) will not be generated. 

### Geometric errors

- By default, as geometric errors [500,0] are used (for 1 LOD). When there multiple LOD's, there should be number_of_lod + 1 geometric errors specified in the -g option. 
When using multiple LOD and the -g option is not specified, the geometric errors are calculated using equal intervals 
between 500 and 0.

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
is activated a subtree file (0_0_0.subtree) will be created (in folder subtrees) and the tileset.json file will no longer explitly list all tiles.

At the moment, Implicit tiling is only supported in the CesiumJS client.

Some remarks about implicit tiling:

- There is no support (yet) for creating octree instead of quadtree;

- There is no support (yet) for multiple contents per tile;

- There is no support (yet) for creating child subtrees, only root subtree file 0_0_0.subtree is created;

- There is no support (yet) for implicit tiling metadata;

- Parameter '-l --lodcolumn' is ignored when using impliciting tiling;

- Only the first value of parameter 'geometricerrors' is used in tileset.json.

For more information about Implicit Tiling see https://github.com/CesiumGS/3d-tiles/tree/draft-1.1/specification/ImplicitTiling

## Shaders

Shaderscolumn is a column of type json. In the json document the shaders are defined like PbrMetallicRoughness and
PbrSpecularGlossiness. Note: PbrSpecularGlossiness is deprecated by Khronos, so advise is to use PbrMetallicRoughness.

### JSON Structure

The json must have the following structure:

```
{
    "EmissiveColors": [list_of_emissivecolors in hex],
    "PbrMetallicRoughness": {
        "BaseColors": [ list_of_basecolors in hex],
        "MetallicRoughness": [list_of_metallic_roughness in hex]
    },
    "PbrSpecularGlossiness": {
        "DiffuseColors": [list_of_diffuse in hex],
        "SpecularGlossiness": [list_of_specular_glossiness in hex]
    }
}
```

The amount of colors in the lists must correspond to the number of triangles in the geometry, otherwise an exception is thrown.

### Samples

Sample for using shader PbrMetallicRoughness with BaseColor for 2 triangles:

```
{
    "PbrMetallicRoughness": {
        "BaseColors": ["#008000","#008000"]
    }
}
```

Sample for Specular Glossiness with Diffuse and SpecularGlossiness for 2 triangles :

```
{
    "PbrSpecularGlossiness": {
        "DiffuseColors": ["#E6008000","#E6008000"],
        "SpecularGlossiness": ["#4D0000ff", "#4D0000ff"]
    }
}
```


In the hexadecimal values there are 4 numbers (x, y, z, w) available. The following material channels table defines which number should be used for the various shader properties.

### Material channels

<table>
<thead>
<tr>
<th>Channel</th>
<th>Shader Style</th>
<th>X</th>
<th>Y</th>
<th>Z</th>
<th>W</th>
</tr>
</thead>
<tbody>
<tr>
<tr>
<td>Emissive</td>
<td>All</td>
<td>Red</td>
<td>Green</td>
<td>Blue</td>
<td></td>
</tr>
<tr>
<td>BaseColor</td>
<td>Metallic Roughness</td>
<td>Red</td>
<td>Green</td>
<td>Blue</td>
<td>Alpha</td>
</tr>
<tr>
<td>MetallicRoughness</td>
<td>Metallic Roughness</td>
<td>Metallic Factor</td>
<td>Roughness Factor</td>
<td></td>
<td></td>
</tr>
<tr>
<td>Diffuse</td>
<td>Specular Glossiness</td>
<td>Diffuse Red</td>
<td>Diffuse Green</td>
<td>Diffuse Blue</td>
<td>Alpha</td>
</tr>
<tr>
<td>SpecularGlossiness</td>
<td>Specular Glossiness</td>
<td>Specular Red</td>
<td>Specular Green</td>
<td>Specular Blue</td>
<td>Glossiness</td>
</tr>
</tbody>
</table>

Sample channel conversion:

- DiffuseColor in Hex = '#E6008000'

Converted to RGBA:

(230, 0, 128, 0)

So Diffuse Red = 230, Diffuse Green = 0, Diffuse Blue = 128, Alpha = 0

### Remarks

- Fallback scenario from SpecularGlossiness to MetallicRoughness shader for clients that do not support 
SpecularGlossiness is not supported (yet)

- BaseColor of default material is not configureable (yet)

- Shader 'unlit' is not supported (yet)

## Run from Docker

Docker image: https://hub.docker.com/repository/docker/geodan/pg2b3dm

Tags used (https://hub.docker.com/repository/docker/geodan/pg2b3dm/tags): 

- 1.2.2 stable build

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

Build app:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd src/pg2b3dm
$ dotnet run -- -h my_host -U my_user -d my_database -t my_schema.my_table
```

To create an self-contained executable '~/bin/pg2b3dm':

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src/pg2b3dm
$ dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true
$ cp ./bin/Release/net6.0/linux-x64/publish/pg2b3dm ~/bin
$ ~/bin/pg2b3dm
```

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
