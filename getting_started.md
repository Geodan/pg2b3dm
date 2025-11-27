# Getting started

1] Minimal example to create 3D Tiles from a 100 * 100 * 100 meter polyhedralsurface cube on Dam square Amsterdam

See https://github.com/bertt/3dtiles_cube

2] Convert 3D Data (Multipolygon Z) to 3D Tiles

### Prerequisites

- Install latest executable pg2b3dm for your platform (see https://github.com/Geodan/pg2b3dm/releases) 

- PostGIS database

- GDAL (ogr2ogr)

Optional check PostGIS:

```
$ postgresql> select ST_AsText(ST_Transform(ST_GeomFromText('POINT(121302 487371 2.68)', 7415), 4979));
POINT Z (4.892367035931109 52.37317920269912 45.66258579945144)
```

In this query a transformation from epsg:7415 to espg:4979 is performed. When the projection grids are installed the vertikal value = 2.68 is converted 
to 45.66258579945144. 

When the projection grids are not installed the vertikal value stays at 2.68. In this case the projection grids should be installed, using tool projsync --all (https://proj.org/en/9.3/apps/projsync.html)

### Download data

- Download Geopackage from https://3dbag.nl/, for example Sibbe [https://3dbag.nl/nl/download?tid=8-688-40](https://3dbag.nl/nl/download?tid=8-688-40)

Result: 8-688-40.gpkg (12 MB) - data has projection EPSG:7415 (EPSG:28992 horizontal reference + EPSG:5709 vertical reference (NAP))

### Data processing

- Import in PostGIS database

Note: in the Cesium client viewer the terrain should be added to see the buildings on the correct height.

```
$ ogr2ogr -f PostgreSQL pg:"host=localhost user=postgres" 8-688-40.gpkg lod22_3d -nln sibbe
```

- Create spatial index

```
postgresql> CREATE INDEX ON sibbe USING gist(st_centroid(st_envelope(geom)))
```

- Convert to 3D Tiles using pg2b3dm

```
$ pg2b3dm -h localhost -U postgres -c geom -d postgres -t sibbe -a identificatie
```

### Visualize

- The resulting tileset can be added to CesiumJS using:

```
   const tileset = await Cesium.Cesium3DTileset.fromUrl(
      "tileset.json"
    );  
    viewer.scene.primitives.add(tileset);
```

- The Dutch terrain can be added in CesiumJS using:

```
var terrainProvider = await Cesium.CesiumTerrainProvider.fromUrl('https://api.pdok.nl/kadaster/3d-basisvoorziening/ogc/v1_0/collections/digitaalterreinmodel/quantized-mesh');
viewer.scene.terrainProvider = terrainProvider;
viewer.scene.globe.depthTestAgainstTerrain=true;
```

- Load 3D Tiles in Cesium viewer, example result see https://geodan.github.io/pg2b3dm/sample_data/3dbag/sibbe/

