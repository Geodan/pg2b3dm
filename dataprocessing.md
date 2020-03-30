# Dataprocessing

pg2b3dm takes as input triangulated PolyhedralSurfaceZ geometries in PostGIS. There are several ways to construct these geometries. In this document some sample 
workflows are described.

1] tool: tesselate_building

Tool tesselate_building https://github.com/bertt/tesselate_building can create PolyhedralSurfaceZ geometries from buildings footprints with height attributes.
See https://github.com/Geodan/pg2b3dm/blob/master/getting_started.md for a complete workflow.

2] tools: FME, GDAL, PostGIS

With tooling like FME, GDAL and PostGIS queries the PolyhedralSurfaceZ geometries can also be created.

Here a sample workflow for processing a GML file with buildings:

## Import GML

todo

## Filter solids

todo: solids eruit gefiltered

## Triangulate

todo

![fme_triangulate](https://user-images.githubusercontent.com/538812/77904859-8c670500-7285-11ea-8982-69ac0db5b630.png)

## Keep only geometries with 4 vertices in Psql

```
psql> SELECT * FROM duisburg.lod2 WHERE (ST_Npoints(wkb_geometry)::float/ST_NumGeometries(wkb_geometry)::float) = 4
```

## Create PolyHedralZ with ogr2og3

```
$ ogr2ogr
ogr2ogr -f "PostgreSQL" "PG:host=leda user=brianv dbname=research" $f -append -nln duisburg.lod2 -dim 3 -nlt POLYHEDRALSURFACEZ
```




