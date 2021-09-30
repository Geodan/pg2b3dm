# pg2b3dm
 
 ![.NET Core](https://github.com/Geodan/pg2b3dm/workflows/.NET%20Core/badge.svg)

Tool for converting 3D geometries from PostGIS to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles)/b3dm tiles. This software started as a port of py3dtiles (https://github.com/Oslandia/py3dtiles) for generating b3dm tiles. The generated 3D Tiles can be visualized in Cesium JS or MapBox GL JS.

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

- added query parameter support;

- Docker support.

To run this tool there must be a PostGIS table available containing triangulated polyhedralsurface geometries. Those geometries can be created 
by FME (using Triangulator transformer - https://www.safe.com/transformers/triangulator/) or custom tesselation tools.

Tileset.json and b3dm tiles are by default created in the 'output/tiles' subdirectory (or specify directory with   -o, --output).

## Live Sample viewers

- 3D Bag by tudelftnl - 10 million Dutch buildings in 3D Tiles https://3dbag.nl/ 

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

  -a, --attributecolumns (Default: '') attributes column names (comma separated)

  -e, --extenttile       (Default: 1000) Maximum extent per tile

  -g, --geometricerrors  (Default: 500, 0) Geometric errors

  -q, --query            (Default: ) Query parameter

   --refine              (Default: REPLACE) Refinement method (REPLACE/ADD)
  
  --shaderscolumn        (Default: ) shaders column
  
  --help                Display this help screen.

  --version             Display version information.  
```

## Installation


Prerequisite: .NET 5.0 SDK is installed https://dotnet.microsoft.com/download/dotnet/5.0

```
$ dotnet tool install -g pg2b3dm
```

Or update

```
$ dotnet tool update -g pg2b3dm
```

## Remarks

### Geometries

- All geometries must be type polyhedralsurface consisting of triangles with 4 vertices each. If not 4 vertices exception is thrown.

### Shaders

In release 0.10 the shaders functionality is changed to support PbrMetallicRoughness and PbrSpecularGlossiness. 

See document <a href= "src/release_notes_0.10.md">release_notes_0.10.md</a> for details.

### Id Column

- Id column must be type string;

- Id column should be indexed for better performance.

### LOD

- if there are no features within a tile boundingbox, the tile (including children) will not be generated. 

### Geometric errors

- By default, as geometric errors [500,0] are used (for 1 LOD). When there multiple LOD's, there should be number_of_lod + 1 geometric errors specified in the -g option. When using multiple LOD and the -g option is not specified, the geometric errors are calculated using equal intervals between 500 and 0.

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

## Getting started

See [getting started](getting_started.md) for a tutorial how to run pg2b3dm and visualize buildings in MapBox GL JS or Cesium.

For a dataprocessing workflow from CityGML to 3D Tiles using GDAL, PostGIS and FME see [dataprocessing/dataprocessing_citygml](dataprocessing/dataprocessing_citygml.md).

## Run from Docker

Docker image: https://hub.docker.com/repository/docker/geodan/pg2b3dm

Tags used (https://hub.docker.com/repository/docker/geodan/pg2b3dm/tags): 

- 0.10 stable build

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

Requirement: Install .NET 5.0 SDK

https://dotnet.microsoft.com/download/dotnet/5.0

TL;DR:

```
$ sudo apt-get update
$ sudo apt-get install apt-transport-https
$ sudo apt-get update
$ sudo apt-get install dotnet-sdk-5.0
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
$ cp ./bin/Release/net5.0/linux-x64/publish/pg2b3dm ~/bin
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

2021-09-30: release 0.11, adding multiple attribute columns support 

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

