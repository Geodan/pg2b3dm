# pg2b3dm

Tool for converting from PostGIS to b3dm tiles. This software is a partial port of py3dtiles (https://github.com/Oslandia/py3dtiles) 
for generating b3dm tiles.

![huisjes](https://user-images.githubusercontent.com/538812/60990513-e6671980-a348-11e9-9205-a7ab580ee69b.png)

Differences to py3dtiles:

- 2* performance improvement;

- loading geometries in batches in memory instead of full datataset;

- fixed glTF warnings;

- added styling options like roof color column per geometry;

- added output directory option;

- Docker support.

To run this tool there must be a PostGIS table available containing triangulated polyhedralsurface geometries.

Tileset.json and b3dm tiles are created in the 'output/tiles' subdirectory.

## History

2019-08-14: release 0.4.2 fixing roof colors + filter very small triangles (<0.01)

2019-08-13: release 0.4.1 with fix for roof colors (option -r)

2019-08-12: release 0.4 adding roof color column option (-r)

2019-08-01: release 0.3.3 with 2 colors

2019-07-09: release 0.3 using library SharpGLTF

2019-06-01: release 0.2.1 with some small fixes

2019-06-01: initial release 0.2

2019-05-01: initial release 0.1

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

  -r , --roofcolorcolumn (default: '') Roof color column name, must contain hex code colors like '#ff5555'
  
  --help                Display this help screen.

  --version             Display version information.  
```

## Run from Docker

Docker image: https://hub.docker.com/r/geodan/pg2b3dm

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

## Cesium sample code 

Cesium sample code to add 3D Tiles - b3dm's:

```
    var viewer = new Cesium.Viewer('cesiumContainer');
    viewer.scene.debugShowFramesPerSecond = true;
    var tileset = viewer.scene.primitives.add(new Cesium.Cesium3DTileset({
      url : './tiles/tileset.json'
    }));
    viewer.zoomTo(tileset, new Cesium.HeadingPitchRange(0, -0.5, 0));
```
