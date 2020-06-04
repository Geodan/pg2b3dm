# pg2b3dm
 
 ![.NET Core](https://github.com/Geodan/pg2b3dm/workflows/.NET%20Core/badge.svg)

Tool for converting from PostGIS to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles)/b3dm tiles. This software started as a port of py3dtiles (https://github.com/Oslandia/py3dtiles) 
for generating b3dm tiles.

![mokum](https://user-images.githubusercontent.com/538812/63088752-24fa8000-bf56-11e9-9ba8-3273a21dfda0.png)

Differences to py3dtiles:

- performance improvements;

- memory usage improvements;

- fixed glTF warnings;

- added colors option;

- added LOD support;

- added output directory option;

- added refinement method (add or replace) support;

- added glTF shader support for PbrMetallicRoughness and PbrSpecularGlossiness

- Docker support.

To run this tool there must be a PostGIS table available containing triangulated polyhedralsurface geometries. Those geometries can be created 
by FME (using Triangulator transformer - https://www.safe.com/transformers/triangulator/) or custom tesselation tools.

Tileset.json and b3dm tiles are by default created in the 'output/tiles' subdirectory (or specify directory with   -o, --output).

## Live Sample viewers

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

  -i, --idcolumn         (Default: id): Identifier column

  -t, --table            (Required) Database table name, include database schema if needed

  -o, --output           (Default: ./output/tiles) Output directory, will be created if not exists

  -p, --port             (Default: 5432) Database port

  -a, --attributescolumn (Default: '') attributes column name 

  -e, --extenttile       (Default: 1000) Maximum extent per tile

  -g, --geometricerrors  (Default: 500, 0) Geometric errors

  --shaderscolumn         (Default: '') shaders column (must be type json) 

  --refine               (Default: REPLACE) Refinement method (REPLACE/ADD)
  
  --help                Display this help screen.

  --version             Display version information.  
```

## Remarks

## Geometries

- All geometries must be type polyhedralsurface consisting of triangles with 4 vertices each. If not 4 vertices exception is thrown.

### Shaders

In release 0.10 the shaders functionality is changed to support PbrMetallicRoughness and PbrSpecularGlossiness. 

See document release_notes_0.10.md for details.

## LOD

- if there are no features within a tile boundingbox, the tile (including children) will not be generated. 

## Geometric errors

- By default, as geometric errors [500,0] are used (for 1 LOD). When there multiple LOD's, there should be number_of_lod + 1 geometric errors specified in the -g option. When using multiple LOD and the -g option is not specified, the geometric errors are calculated using equal intervals between 500 and 0.

## Getting started

See [getting started](getting_started.md) for a tutorial how to run pg2b3dm and visualize buildings in MapBox GL JS or Cesium.

For a dataprocessing workflow from CityGML to 3D Tiles using GDAL, PostGIS and FME see [dataprocessing/dataprocessing_citygml](dataprocessing/dataprocessing_citygml.md).

## Run from Docker

Docker image: https://hub.docker.com/repository/docker/geodan/pg2b3dm

Tags used (https://hub.docker.com/repository/docker/geodan/pg2b3dm/tags): 

- 0.10.0_preview1 : preview release of 0.10

- 0.9.3 stable build

- latest: is build automatically after push to master


### Building

```
$ docker build -t geodan/pg2b3dm .
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

Requirement: Install .NET Core 3.1 SDK

https://dotnet.microsoft.com/download/dotnet-core/3.1

TL;DR:

```
$ sudo apt-get update
$ sudo apt-get install apt-transport-https
$ sudo apt-get update
$ sudo apt-get install dotnet-sdk-3.1
```

Build app:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src
$ dotnet build
$ dotnet run
```

To create an self-contained executable '~/bin/pg2b3dm':

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src/pg2b3dm
$ dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true
$ cp ./bin/Release/netcoreapp3.1/linux-x64/publish/pg2b3dm ~/bin
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


## History

2020-06-04: preview release 0.10, adding shader support PbrMetallicRoughness and PbrSpecularGlossiness

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

