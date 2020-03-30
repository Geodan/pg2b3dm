# Dataprocessing CityGML -> PolyhedralSurfaceZ

pg2b3dm takes as input triangulated PolyhedralSurfaceZ geometries in PostGIS. In this document a sample 
workflow is described for processing CityGML files using FME, GDAL and PostGIS.

Sample input files: https://www.opengeodata.nrw.de/produkte/geobasis/3dg/lod2_gml/

### Import CityGML

todo

### Filter solids

todo: solids eruit gefiltered

### Triangulate

todo

![fme_triangulate](https://user-images.githubusercontent.com/538812/77904859-8c670500-7285-11ea-8982-69ac0db5b630.png)

### Keep only geometries with 4 vertices in Psql

```
psql> SELECT * FROM duisburg.lod2 WHERE (ST_Npoints(wkb_geometry)::float/ST_NumGeometries(wkb_geometry)::float) = 4
```

### Create PolyHedralZ with ogr2ogr

```
$ ogr2ogr -f "PostgreSQL" "PG:host=leda user=brianv dbname=research" $f -append -nln duisburg.lod2 -dim 3 -nlt POLYHEDRALSURFACEZ
```

Now it's possible to run pg2b3dm on duisburg.lod2 column.

Result looks like: todo


