# Getting started

## Introduction

In this document we run pg2b3dm on a sample dataset, a shapefile from New York containing building footpints with a height attribute. The generated 3D tiles are visualized in Cesium viewer.

## Download data

We download a dataset from the US Building Footprints.

https://wiki.openstreetmap.org/wiki/Microsoft_Building_Footprint_Data

Download dataset: 

Delaware - Dover (22,532 buildings available)

https://1drv.ms/u/s!AqWv0F0N63JkgQqO6E9e2kI28R16

Donwload zip, unzip. It contains a 'bldg_footprints.shp' shapefile.

## Setup PostGIS

1] Create Docker network

In this tutorial, we'll start 2 containers: PostGIS database and tiling tool pg2b3dm. Because those containers need to communicate
they must be in the same network. So we'll create a network first and add the 2 containers later.

If you have already installed a PostGIS server you can skip this step.

```
$ docker network create  mynetwork
```

2] Start PostGIS database

```
$ docker run --name some-postgis -e POSTGRES_PASSWORD=postgres -p 5432:5432 -it --network mynetwork mdillon/postgis
```

## Import buildings to PostGIS

Import the buildings to database using ogr2ogr.

```
$ ogr2ogr -f "PostgreSQL" PG:"host=localhost user=postgres password=postgres dbname=postgres" bldg_footprints.shp -nlt POLYGON -nln delaware_buildings
```

In PostGIS, a spatial table 'delaware_buildings' is created.

## PSQL into PostGIS

PSQL into PostGIS and do a count on the buildings:

```
$ psql -U postgres
Password for user postgres:
psql (11.5, server 11.2 (Debian 11.2-1.pgdg90+1))
WARNING: Console code page (850) differs from Windows code page (1252)
         8-bit characters might not work correctly. See psql reference
         page "Notes for Windows users" for details.
Type "help" for help.

postgres=# select count(*) from delaware_buildings;
 count
--------
 22532
(1 row)
```

## Install extension postgis_sfcgal

Extension  postgis_sfcgal is needed for running ST_Extrude function.

```
postgres=# CREATE EXTENSION postgis_sfcgal;
```

## Clean data

Maybe there are some invalid polygons, let's remove them first.

```
postgres=# DELETE from delaware_buildings where ST_IsValid(wkb_geometry)=false;
```

## Add id field with text type

```
postgres=# ALTER TABLE delaware_buildings ADD COLUMN id varchar;
postgres=# UPDATE delaware_buildings SET id = ogc_fid::text;
```

## Reproject geometry to 4978

```
postgres=# ALTER TABLE delaware_buildings ADD COLUMN geom_4978 geometry;
postgres=# update delaware_buildings set geom_4978=ST_Transform(wkb_geometry, 4978);
```

## Add column for triangulated geometry

```
postgres=# ALTER TABLE delaware_buildings ADD COLUMN  geom_4978_triangle geometry;
```

## Fill column with triangulated geometry

Todo

## Run pg2b3dm

Run pg2b3dm, the program will make a connection to the database and 1 tileset.json and 927 b3dm's will be created in the output directory.

```
λ docker run -v $(pwd)/output:/app/output -it --network mynetwork geodan/pg2b3dm -h some-postgis -U postgres -c geom_4978_triangle -t delaware_buildings -d postgres -i id
tool: pg2b3dm 0.8.0.0
Password for user postgres:
Start processing....
Calculating bounding boxes...
Writing tileset.json...
Writing 927 tiles...
Progress: tile 927 - 100.00%
Elapsed: 93 seconds
Program finished.
```

## Visualize in Cesium

Copy the generated tiles to sample_data\cesium (overwrite the sample tiles there).

Put [sample_data/index_cesium.html](sample_data/index_cesium.html) on a webserver (for example https://caddyserver.com/).

Open a browser and if all goes well in Delaware - Dover you can find some 3D Tiles buildings.
