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

## Command line options

All parameters are optional, except the -t --table option. 

If --username and/or --dbname are not specified the current username is used as default.

```
  -U, --username        (Default: username) Database user

  -h, --host            (Default: localhost) Database host

  -d, --dbname          (Default: username) Database name

  -c, --column          (Default: geom) Geometry column name

  -t, --table           (Required) Database table name, include database schema if needed

  -o, --output          (Default: ./output/tiles) Output directory, will be created if not exists

  -p, --port            (Default: 5432) Database port

  -r , --roofcolorcolumn (default: '') color column name
  
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


## Run from Docker

Docker image: https://hub.docker.com/r/geodan/pg2b3dm

Tags used (https://hub.docker.com/r/geodan/pg2b3dm/tags): 

- 0.5: 0.5 release

- latest: latest release


## Getting started 

A sample dataset with some 3D buildings is provided, see sample_data/buildings.backup.

Steps to get pg2b3dm running on this sample dataset:

1] Start PostGIS database

```
$ docker run --name some-postgis -e POSTGRES_PASSWORD=postgres -p 5432:5432 -it --network mynetwork mdillon/postgis
```

2] Connect to database using pgAdmin or similar db management tool

3] Create schema 'bertt'

4] Select PostgreSQL database and restore file buildings.backup

A table bertt.buildings will be created, contains 100 sample buildings in Amsterdam.

5] Run pg2b3dm, the program will make a connection to the database and 9 b3dm's will be created in the output directory.

```
λ docker run -v $(pwd)/output:/app/output -it --network mynetwork geodan/pg2b3dm -h some-postgis -U postgres -c geom -t  bertt.buildings -d postgres -r colors
tool: pg2b3dm 0.5.1.0
Password for user postgres:
Start processing....
Calculating bounding boxes...
Writing tileset.json...
Writing 9 tiles...
Progress: tile 9 - 100.00%
Elapsed: 2 seconds
Program finished.
```

6] Install Cesium 

See https://cesium.com/docs/tutorials/getting-started/ for installing/running Cesium application

7] Configure Cesium

Now change a basic sample Cesium viewer and add a 3D Tile layer by pointing to the generated tileset.json:

Sample code: 

```
var viewer = new Cesium.Viewer('cesiumContainer');
viewer.scene.debugShowFramesPerSecond = true;
var tileset = viewer.scene.primitives.add(new Cesium.Cesium3DTileset({
    url : './tiles/tileset.json'
}));
viewer.zoomTo(tileset, new Cesium.HeadingPitchRange(0, -0.5, 0));
```

8] Test Cesium

If all goes well In Amsterdam you can find some 3D Tiles buildings:

![Hello World Buildings](https://user-images.githubusercontent.com/538812/63441248-6517a200-c431-11e9-96c5-d1d38d2513a6.png)

9] Advanced scenario - customize building colors

Change some colors in the 'colors' column and run pg2b3m again. Restart Cesium and the new colors should be visible.

10] Visualize in MapBox GL JS

To visualize in MapBox GL JS we have to transform the buildings table to Spherical Mercator (3857):

```
CREATE TABLE bertt.buildings_3857 AS 
SELECT ST_Transform(geom,3857) AS geom, blockid,color, colors 
FROM bertt.buildings;
```

And rerun pg2b3dm on this table:

```
$ docker run -v $(pwd)/output:/app/output -it --network mynetwork geodan/pg2b3dm -h some-postgis -U postgres -c geom -t  bertt.buildings_3857 -d postgres -r colors
```

The b3dm tiles can be visualized by adding Three.JS and glTF functionality to the MapBox GL JS viewer. See https://github.com/Geodan/mapbox-3dtiles for more information about this topic.

Put sample html page sample_data/index_mapbox.html on a webserver, and copy https://github.com/Geodan/mapbox-3dtiles/blob/master/Mapbox3DTiles.js to this folder.

Final result in MapBox GL JS:

![hoofden](https://user-images.githubusercontent.com/538812/63675318-d320e800-c7e8-11e9-82f4-fcfb2a187044.png)

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

