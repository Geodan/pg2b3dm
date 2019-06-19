# pg2b3dm

Tool for converting from PostGIS to b3dm tiles. This software is a partial port of py3dtiles (https://github.com/Oslandia/py3dtiles) 
for generating b3dm tiles.

Differences to py3dtiles:

- 2* performance improvement;

- loading geometries in batches in memory instead of full datataset;

- fixed glTF warnings;

- add styling options;

- Docker support.

To run this tool there must be a PostGIS table available containing triangulated polyhedralsurface geometries.

Tileset.json and b3dm tiles are created in the 'tiles' subdirectory.

## Command line options

All parameters are optional, except the -t --table option. 

If --username and/or --dbname are not specified the current username is used as default.

```
  -U, --username     (Default: username) Database user

  -h, --host        (Default: localhost) Database host

  -d, --dbname      (default: username) Database name

  -c, --column      (Default: geom) Geometry column

  -t, --table       (Required) Database table, include database schema if needed

  -p, --port        (Default: 5432) Database port

  --help            Display this help screen.

  --version         Display version information.  
```

## Run from Docker

Docker image: https://hub.docker.com/r/geodan/pg2b3dm

Building image:

```
$ docker build -t geodan/pg2b3dm .
```

Running image:

Sample on Windows: 

```
$ docker run -v C:/Users/bertt/output:/app/output -it geodan/pg2b3dm -h my_host -U my_user -d my_database -t my_table
```

Sample on Linux:

```
$ docker run -v $(pwd)/output:/app/output -it geodan/pg2b3dm -h my_host -U my_user -d my_database -t my_schema.my_table
```

## Run from source

Requirement: Install .NET Core 2.2 SDK 

https://dotnet.microsoft.com/download/dotnet-core/2.2

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

## Debugging SQL Queries

py3dtiles:

1] select 3D extent of all ST_PolyhedralSurface geometries

```
SELECT ST_3DExtent(geom) FROM tmptom.betterroofs_extruded where ST_GeometryType(geom) like 'ST_PolyhedralSurface'
```

In code: calculate center for transform

2] Select all geometries to process

````
SELECT ST_AsBinary(ST_RotateX(ST_Translate(geom, -3893180.29274884, -337609.41850392846, -5023897.8453492), -pi() / 2)),ST_Area(ST_Force2D(geom)) AS weight FROM tmptom.betterroofs_extruded where ST_GeometryType(geom) like 'ST_PolyhedralSurface' ORDER BY weight DESC
```

