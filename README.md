# pg2b3dm

Tool for converting from PostGIS to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles)/b3dm tiles. This software is a partial port of py3dtiles (https://github.com/Oslandia/py3dtiles) 
for generating b3dm tiles.

![mokum](https://user-images.githubusercontent.com/538812/63088752-24fa8000-bf56-11e9-9ba8-3273a21dfda0.png)

Differences to py3dtiles:

- 2* performance improvement;

- loading geometries in batches in memory instead of full datataset;

- fixed glTF warnings;

- added styling options like roof color column per geometry/triangle;

- added output directory option;

- Docker support.

To run this tool there must be a PostGIS table available containing triangulated polyhedralsurface geometries.

Tileset.json and b3dm tiles are by default created in the 'output/tiles' subdirectory (or specify directory with   -o, --output).

## Live Sample viewers

Cesium: https://geodan.github.io/pg2b3dm/sample_data/index_cesium.html

MapBox: https://geodan.github.io/pg2b3dm/sample_data/index_mapbox.html

## Command line options

All parameters are optional, except the -t --table option. 

If --username and/or --dbname are not specified the current username is used as default.

```
  -U, --username        (Default: username) Database user

  -h, --host            (Default: localhost) Database host

  -d, --dbname          (Default: username) Database name

  -c, --column          (Default: geom) Geometry column name

  -i, --idcolumn        (Default: id): Identifier column

  -t, --table           (Required) Database table name, include database schema if needed

  -o, --output          (Default: ./output/tiles) Output directory, will be created if not exists

  -p, --port            (Default: 5432) Database port

  -r, --roofcolorcolumn (default: '') color column name

  -a, --attributescolumn (default: '') attributes column name 
  
  --help                Display this help screen.

  --version             Display version information.  
```

Geometry rules:

- All geometries must be type polyhedralsurface consisting of triangles with 4 vertices each. If not 4 vertices exception is thrown.

Color column rules:

- Colors must be specified as hex colors, like '#ff5555';

- If no color column is specified, a default color (#bb3333) is used for all buildings;

- If color column is specified and database type is 'text', 1 color per building is used;

- If color column is specified and database type is 'text[]', 1 color per triangle is used. Exception is thrown when number

of colors doesn't equal the number of triangles in geometry. Order of colors must be equal to order of triangles.


Id column rules:

	- Id column must be type string;

	- Id column should be indexed for better performance.

## Getting started

See [getting started](getting_started.md) for a tutorial how to run pg2b3dm and visualize in Cesium and MapBox GL JS on a sample dataset.

## Run from Docker

Docker image: https://hub.docker.com/r/geodan/pg2b3dm

Tags used (https://hub.docker.com/r/geodan/pg2b3dm/tags): 

- 0.5: 0.5 release

- 0.6: 0.6 release

- latest: latest release


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

Requirement: Install .NET Core 3.0 SDK 

https://dotnet.microsoft.com/download/dotnet-core/3.0

```
$ git clone https://github.com/Geodan/pg2b3dm.git

$ cd src/pg2b3dm

$ dotnet build

$ dotnet run

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

