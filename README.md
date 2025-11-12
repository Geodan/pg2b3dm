# pg2b3dm
 
 ![Build status](https://github.com/Geodan/pg2b3dm/actions/workflows/main.yml/badge.svg)[![Nuget](https://img.shields.io/nuget/vpre/pg2b3dm)](https://www.nuget.org/packages/pg2b3dm)
[![NuGet](https://img.shields.io/nuget/dt/pg2b3dm.svg)][![Join the chat at https://discord.gg/gGCka4Nd](https://img.shields.io/discord/1013017110814932993?color=%237289DA&label=pg2b3dm&logo=discord&logoColor=white)](https://discord.gg/uSKvUwPgmG)

 Tool for converting 3D geometries from PostGIS to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles). The generated 
 3D Tiles can be visualized in Cesium JS, Cesium for Unreal/Unity3D/Omniverse/Godot, QGIS, ArcGIS Pro, ArcGIS Maps SDK for JavaScript or other 3D Tiles client viewers.

![image](https://user-images.githubusercontent.com/538812/227500590-bebe59b6-5697-462d-9ebd-b40fe9a2dc2b.png)

Features:

- Explicit and Implicit tiling;

- QUADTREE and OCTREE tiling schema;

- 3D Tiles extensions EXT_Mesh_Features and EXT_Structural_Metadata; 

- Valid glTF 2.0 files;

- Shading PbrMetallicRoughness and PbrSpecularGlossiness;

- Query parameter support;

- Cesium: LOD support and Outlines support (using CESIUM_primitive_outline);

- Triangulation of input geometries LineStrings/Polygon/MultiPolygon/PolyhedralSurface/TIN with Z values;

- 3D Tiles in global coordinates (EPSG:4978) or in local cartesian coordinates;

- Docker support.

Resulting tilesets can be validated against 3D Tiles Validator (https://github.com/CesiumGS/3d-tiles-validator).

Tileset.json and glb/b3dm tiles are by default created in the 'output/content' subdirectory (or specify output directory with   -o, --output).

## Getting started

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

Result: 8-688-40.gpkg (12 MB)

### Data processing

- Import in PostGIS database, convert to EPSG:4979 (WGS84 ellipsoidal heights). Note: in the Cesium client viewer the terrain should be added to see the buildings on the correct height.

```
$ ogr2ogr -f PostgreSQL pg:"host=localhost user=postgres password=postgres" -t_srs epsg:4979 8-688-40.gpkg lod22_3d -nln sibbe
```

When the terrain is not used, omit the -t_srs parameter (in this case the Dutch EPSG code EPSG:7415 of the input data will be used).

- Optional: Add spatial index

```
postgresql> CREATE INDEX ON sibbe USING gist(st_centroid(st_envelope(geom)))
```

- Convert to 3D Tiles using pg2b3dm

```
$ pg2b3dm -h localhost -U postgres -c geom -d postgres -t sibbe -a identificatie
```

Output should be as follows:

https://gist.github.com/bertt/fa084f55217dded35c6fb1607e81a9f3

### Visualize

- The resulting tileset can be added to CesiumJS using:

```
   const tileset = await Cesium.Cesium3DTileset.fromUrl(
      "./1.1/tileset.json"
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

3] Converting CityGML to 3D Tiles using 3DCityDB v5

For a dataprocessing workflow from CityGML to 3D Tiles using 3DCityDB v5 see [dataprocessing/dataprocessing_citygml](dataprocessing/dataprocessing_citygml.md).

### Older getting started documents

See [getting started](getting_started.md) for a tutorial how to convert a 2D shapefile of buildings with height attribute to 3D Tiles and visualize in CesiumJS/Cesium for Unreal/Unity3D.

## Demo

![Alt Text](demo_pg2b3dm.gif)

## Live Sample viewers

- 3D Bag by tudelftnl - 10 million Dutch buildings in 3D Tiles https://3dbag.nl/ 

![image](https://user-images.githubusercontent.com/538812/194698535-5b324133-bdf1-4d8c-8d53-37555a6f7b5b.png)

- FOSS4G presentations

Presentation at FOSS4G 2021: A fast web 3D viewer for 11 million buildings https://www.youtube.com/watch?v=1_JM2Xf5mDk

Presentation at FOSS4G 2019: 3D geodata with 3D Tiles https://www.youtube.com/watch?v=HXQJbyEnC9w

- Amsterdam Buildings in Cesium: https://geodan.github.io/pg2b3dm/sample_data/amsterdam/cesium/

- Dover - Delaware buildings in Cesium: https://geodan.github.io/pg2b3dm/sample_data/delaware/cesium/

## Command line options

All parameters are optional, except the -t --table option. 

If --username and/or --dbname are not specified the current username is used as default.

```
  -U, --username                  Database user

  -h, --host                      (Default: localhost) Database host

  -d, --dbname                    Database name

  -c, --column                    (Default: geom) Geometry column

  -t, --table                     Required. Database table, include database schema if needed

  -p, --port                      (Default: 5432) Database port

  -o, --output                    (Default: output) Output path

  -a, --attributecolumns          (Default: '') Attribute columns

  -q, --query                     (Default: '') Query parameter

  --copyright                     (Default: '') glTF asset copyright

  --default_color                 (Default: #FFFFFF) Default color, in RGB(A) order

  --default_metallic_roughness    (Default: #008000) Default metallic roughness

  --double_sided                  (Default: true) Default double sided
  
  --default_alpha_mode            (Default: OPAQUE) Default glTF material
                                  AlphaMode. Other values: BLEND and MASK.
                                  Defines how the alpha value is interpreted.

  --create_gltf                   (Default: true) Create glTF files

  --radiuscolumn                  (Default: '') Column with radius values for lines

  --max_features_per_tile         (Default: 1000) maximum features per tile

  -l, --lodcolumn                 (Default: '') LOD column

  -g, --geometricerror            (Default: 2000) Geometric error

  --geometricerrorfactor          (Default: 2) Geometric error factor

  --shaderscolumn                 (Default: '') shaders column

  --tilesetVersion                (Default: '') Tileset version

  --use_implicit_tiling           (Default: true) use 1.1 implicit tiling

  --add_outlines                  (Default: false) Add outlines

  -r, --refinement                (Default: ADD) Refinement ADD/REPLACE

  --skip_create_tiles             (Default: false) Skip creating tiles

  --keep_projection               (Default: false) Keep projection of input data

  --subdivision                   (Default: QUADTREE) Subdivision schema QUADTREE/OCTREE

  --help                          Display this help screen.

  --version                       Display version information.
```

Sample command for running pg2b3dm:

```
$ pg2b3dm -h localhost -U postgres -c geom_triangle --shaderscolumn shaders -t delaware_buildings -d postgres -g 100,0 
```

Database password will be asked to create the database connection, unless:

- Trusted authentication is enabled;

- Environment variable 'PGPASSWORD' is set.

## Installation

### Pre-built binaries

See Releases https://github.com/Geodan/pg2b3dm/releases for pre-built binaries for Windows, Linux and OSX. Binaries are available for X64 and ARM.

Sample Windows installation (note use correct version):

```
$ wget https://github.com/Geodan/pg2b3dm/releases/download/{version}/pg2b3dm-win-x64.zip
$ unzip pg2b3dm-win-x64.zip
$ pg2b3dm
```

### .NET tool

As alternative use .NET 8.0 SDK to install the tool:

Prerequisite: .NET 8.0 SDK is installed https://dotnet.microsoft.com/download/dotnet/8.0

```
$ dotnet tool install -g pg2b3dm
```

Or update

```
$ dotnet tool update -g pg2b3dm
```

To run:

```
$ pg2b3dm
```

### Docker

To run the latest Docker image:

```
$ docker pull geodan/pg2b3dm
$ docker run geodan/pg2b3dm
```

## Benchmarking

| Source                   | Table                 |  Size     | Features     | Time         | Tiles    | Tiles/Minute   | Tile uncompressed  | Tile compressed |
|--------------------------|-----------------------|-----------|--------------|--------------|----------|----------------|--------------------|-----------------|
| Dutch 3d BAG buildings   | lod12_3d              | 12 GB     | 9.712.728    |  1h 54m 23s  |  29098   | 255            |                    |                 |
| French IGN buildings     | batiment_construction | 142 GB    | 46.286.334   |  3h 21m 33s  |  117745  | 585            | 92GB               | 11GB            |

## Styling

For styling see [styling 3D Tiles](styling.md) 

## Geometries

Input geometries must be of type LineString/MultilineString/Polygon/MultiPolygon/PolyhedralSurface (with z values). When the geometry is not triangulated, pg2b3dm will perform
triangulation. Geometries with interior rings are supported.

For large datasets create a spatial index on the geometry column:

```
psql> CREATE INDEX ON the_table USING gist(st_centroid(st_envelope(geom_triangle)));
```

When there the spatial index is not present the following warning is shown.

![image](https://user-images.githubusercontent.com/538812/261248327-c29b4520-a374-4441-83bf-2b60e8313c65.png)

For line geometries a 3D tube is created with a radius of 1 meter. When a radius column is specified (option --radiuscolumn), the radius from that columns is 
used for the tube. 

Sample for random radius between 0.5 and 1.5:

```
postgresql> alter table delaware_buildings add column radius real;
postgresql> update delaware_buildings set radius = 0.5 + random() * (1.5 - 0.5);
```

Sample with pipes (green = data, blue = water, purple = sewage, yellow = gas, red = electricity):

![image](https://github.com/Geodan/pg2b3dm/assets/538812/20280276-02a2-41f1-8b3d-4a893eb82db3)

## Keep projection parameter

When using the keep_projection parameter (default false), no transformation to global coordinates (EPSG:4978) is performed. All 
the coordinates are kept in the original coordinate system. 

In case of keep_projection, the boundingVolume box property is used instead of boundingVolume region in tileset.json.

In tileset.json - Asset section there is extra property 'crs' for describing the coordinate system of the input data.

Note: The keep_projection parameter is implemented for implicit tiling, not for explicit tiling. When using explicit tiling and keep_projection, 
an error will show up and the program will exit.

## Subdivision parameter

There are 2 tiling schemas supported: QUADTREE and OCTREE (default is QUADTREE), use parameter --subdivision.

When the input geometries are distributed in a flat area (like buildings in a city), QUADTREE is preferred.

OCTREE is used when the input geometries are distributed in a cube-like area.

Most features are supported when using OCTREE subdivision, except LOD support;

## Query parameter

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

## Attributes

With the -a attributecolumns parameter multiple columns with attributes can be specified. The attribute information is stored in the b3dm batch table or in the glTF 
(using EXT_Structural_Metadata extension).

Multiple columns must be comma separated (without spaces):

Sample:  --attributecolumns col1,col2

When using 3D TIles 1.1 and EXT_Structural_Metadata, the following mapping between PostgreSQL data types and 3D Tiles data types is used:

| PostgreSQL data type | 3D Tiles data type  (type / componenttype) | 3D Tiles Nodata value |
|----------------------|--------------------|-------------------|
| boolean | boolean / - | - |
| smallint | scalar / int16 | Int16.MinValue |
| integer | scalar / int32 | Int32.MinValue  |
| bigint | scalar / int64 | Int64.MinValue |
| real | scalar / float32 | UInt32.MinValue |
| numeric | scalar / float32 |  UInt32.MinValue |
| double precision | scalar / float64 | Double.MinValue |
| numeric[] all of length 3 | vec3 / float32 | - |
| numeric[] all of length 16 | mat4 / float32 | - |
| numeric[] | scalar / float32 | - |
| varchar | string | "" |
| datetime | string | "" |
| datetime[] | string[] | - |


When one of the above types (except boolean and array types) is set to NULL in the database, the null values are converted
to a nodata value to be used in the 3D Tiles batch table.

Also arrays of the above types are supported, like: 

bool[], smallint[], int[], bigint[], real[], numeric[][], double precision[], datetime[] and varchar[]

Arrays can be of fixed length or not.

When the type is numeric[]/numeric[][], it is checked if all the items contain 3 (vector3) or 16 values (mat4)
. If so, the vec3 or mat4 type is used.

Null values are not supported in arrays (including vector3 and matrix types).

When other types are used, there will be a exception.

Example creating string values in a column:

```
postgresql> alter table delaware_buildings add column random_string varchar not null default 'standaard waarde'
```

and for an array of strings:

```
postgresql> ALTER TABLE delaware_buildings  ADD COLUMN random_strings VARCHAR[] DEFAULT '{waarde1, waarde2}'
```

In the options you can now specify the column name 'random_string' and/or 'random_strings' to add the values.


## Cesium support

For Cesium support (tiling schema, LODS, outlines) see [Cesium notes](cesium_notes.md) 

## ArcGIS Pro support

In ArcGIS Pro 3.2 support for 3D Tiles is added (https://pro.arcgis.com/en/pro-app/latest/help/mapping/layer-properties/work-with-3d-tiles-layers.htm)

Sample: Use option 'Data from path' with  https://geodan.github.io/pg2b3dm/sample_data/3dbag/sibbe/1.0/tileset.json

![image](https://github.com/Geodan/pg2b3dm/assets/538812/bf82df73-781c-41a4-97f2-a26c601a78ec)

![image](https://github.com/Geodan/pg2b3dm/assets/538812/ad3332c7-1a95-46f2-bcce-92a5e10ceccc)


## QGIS support

In QGIS 3.34 support for 3D Tiles is added see https://cesium.com/blog/2023/11/07/qgis-now-supports-3d-tiles/

To create 3D Tiles for QGIS use parameters '--create_gltf false --use_implicit_tiling false' as 3D Tiles 1.1 features are not supported yet. 

Sample dataset Sibbe https://geodan.github.io/pg2b3dm/sample_data/3dbag/sibbe/1.0/tileset.json

![image](https://github.com/Geodan/pg2b3dm/assets/538812/a89e531c-6aa5-4f0b-b7ae-35f43ee52ef8)


## Game engines Unity3D / Unreal / Omniverse support

To create 3D Tiles for game engines use parameters '--create_gltf false --use_implicit_tiling false' as 3D Tiles 1.1 features are not supported yet.

Sample dataset Sibbe: https://geodan.github.io/pg2b3dm/sample_data/3dbag/sibbe/1.0/tileset.json

## Run from Docker

Docker image: https://hub.docker.com/repository/docker/geodan/pg2b3dm

Tags used (https://hub.docker.com/repository/docker/geodan/pg2b3dm/tags): 

- {version}: specific version

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

Sample on Linux:

```
$ docker run -v $(pwd)/output:/app/output -it geodan/pg2b3dm -h my_host -U my_user -d my_database -t my_schema.my_table
```

## Run from source

Requirement: Install .NET 8.0 SDK

https://dotnet.microsoft.com/download/dotnet/8.0

Installation guide see https://docs.microsoft.com/en-us/dotnet/core/install/

To run the app:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src/pg2b3dm
$ dotnet run -- -h my_host -U my_user -d my_database -t my_schema.my_table
```

To create an self-contained executable '~/bin/pg2b3dm' for Linux:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src/pg2b3dm
$ dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true
$ cp ./bin/Release/net8.0/linux-x64/publish/pg2b3dm ~/bin
$ ~/bin/pg2b3dm
```

Alternative options for parameter -r in dotnet publish: 'osx-x64' (Mac), 'win-x64' (Windows)

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

- b3dm-tile (https://github.com/bertt/b3dm-tile-cs) - for generating b3dm files;

- CommandLineParser (https://github.com/commandlineparser/commandline) for parsing command line options;

- Npgsql (https://www.npgsql.org/) - for access to PostgreSQL;

- SharpGLTF (https://github.com/vpenades/SharpGLTF) for generating glTF;

- Subtree (https://github.com/bertt/subtree) - for subtree file handling

- Triangulator (https://github.com/bertt/triangulator) - for triangulating geometries

- Wkx (https://github.com/cschwarz/wkx-sharp) - for geometry handling.

## History

2025-11-12: release 2.24.0: add support OCTREE subdivision with explicit tiling

2025-11-06: release 2.23.1: fix octree subdivision ath higher z-levels visibility

2025-11-05: release 2.23.0: add multiple subtree files support for OCTREE tiling schema

2025-30-10: release 2.22.0: add subdivision parameter for QUADTREE/OCTREE tiling schema (default QUADTREE)

2025-10-01: release 2.21.0 use opaque mode when basecolor is opaque

2025-08-11: release 2.20.0, 2.20.1, 2.20.2 improve keep_projection parameter for implicit tiling

2025-04-16: release 2.19.0 add keep_projection parameter

2024-10-23: release 2.18.1 add null checking in attribute columns

2024-10-15: release 2.18.0 add option tilesetVersion

2024-09-04: release 2.17.0:

- Add support for DateTime in 1.1 Metadata (EXT_Structural_Metadata)

- Add option skip_create_tiles (default false)

- Fix triangulation of polygons with multiple interior rings

2024-08-29: release 2.16.0: 

- Parameter -g --geometricerrors changed to -g, --geometricerror;

- Added parameter --geometricerrorfactor;

- Parameter -r --refinement default changed from REPLACE to ADD;

2024-08-15: release 2.15.1: assume RGBA format for hex color values, not ARGB

2024-08-14: release 2.15: add alpha blending support. Parameter --default_alpha_mode (default OPAQUE, options OPAQUE/BLEND/MASK)

2024-08-08: release 2.14.1: make zip releases smaller

2024-08-08: release 2.14.0: 

- add support for lines without z values

- add support for multiple postgres types for the radius column (not only real)

2024-08-07: release 2.13.0: add support for shaders per inner geometry of collections

2024-07-30: release 2.12.0: use LineCurve instead of Catmullrom curve for lines (better performance + more accurate)

2024-07-17: release 2.11.1: fix .NET 8.0 release 

2024-07-17: release 2.11: from .NET 6 .0 to .NET 8.0

2024-07-16: release 2.10.1: add pre-built release binaries for Windows/Linux/IOS (for X64 and ARM)

2024-07-10: release 2.10.0: add Github releases for Windows and Linux

2024-07-04: release 2.9.0: add TIN geometry support (https://github.com/Geodan/pg2b3dm/pull/178 by [@sebastianmattar] (https://www.github.com/sebastianmattar))

2024-06-20: release 2.8.2: fix lines with constant z values + improve outlines

2024-04-09: release 2.8.1: no rounding of bounding volume values

2024-04-09: release 2.8.0, improve bounding volume z values when using explicit tiling 

2024-04-03: release 2.7.0, create more tileset.json files with explicit tiling + change spatial index check + performance improvement count features

2024-03-20: release 2.6.1, fix z of boundingvolumes

2024-03-06: release 2.6.0, add support for Mapbox v3 (experimental), added parameter --format (default Cesium) Cesium/Mapbox

2024-02-20: release 2.5.1, add support for multiline strings

2024-02-15: release 2.5.0 

- add support for single shaders per geometry https://github.com/Geodan/pg2b3dm/pull/147

- add lines support, added option --radiuscolumn https://github.com/Geodan/pg2b3dm/pull/146
 
- update triangulator for higher precision normals calculation

2024-02-08: release 2.4.0, add support for polygons with interior rings

2024-01-26: release 2.3.0, add support for null values in attribute columns (except array types)

2024-01-26: release 2.2.1, fix for degenerated triangles

2024-01-25: release 2.2.0, add support for 3D Tiles 1.1 all EXT_Structural_Metadata types + create tileset.json file with 
version 1.0 when create_gltf is false and use_implicit_tiling is false

2024-01-23: release 2.1.0, fix of offsets + add polygonZ support

2024-01-10: release 2.0.1, fix for triangulator + add check on interrior rings (not supported)

2024-01-03: release 2.0.0, 

- Breaking change: removed input coordinate system requirement (EPSG:4978), use EPSG:4326/EPSG:4979 or local coordinate system instead. 

- glTF transformation is defined in tileset.json (instead of in glTF asset). As a result, the glTF assets are no longer 'skewed' when visualized in a glTF viewer.

- removed parameter 'boundingvolume_heights', heights are calculated from the input data 

2023-11-13: release 1.8.5, fix for dataset with geometries on 1 location

2023-10-25: release 1.8.4, add -r --refinement option

2023-10-17: release 1.8.3, tileset.json asset version from 1.0 to 1.1, database connection timeout removed

2023-10-04: release 1.8.2, use humanizer with resources 

2023-09-26: release 1.8.1, updating triangulator 

2023-09-22: release 1.8, adding 3D Tiles 1.1 Metadata support (EXT_Mesh_Features / EXT_Structural_Metadata). Options added: create_gltf (default true), double_sided (default true)

2023-08-29: release 1.7.1, improve spatial index check

2023-08-29: release 1.7.0, add triangulator - runs only when geometry is not triangulated

2023-08-29: release 1.6.3, add support for MultiPolygonZ

2023-08-17: release 1.6.2, add check for spatial index

2023-08-16: release 1.6.1, translate b3dm's to center of tile for Mapbox GL JS v3

2023-08-16: release 1.6.0, add experimental support for Mapbox GL JS v3

2023-06-20: release 1.5.5, fix issue when only 1 level is generated

2023-04-06: release 1.5.3, fix disappearing features

2023-04-04: release 1.5.2, fix query parameter

2023-03-27: release 1.5.1, add outlines support for multiple shaders

2023-03-15: release 1.5.0, adding options 'add_outlines' (default false) and 'default_color' (#FFFFFF)

2023-02-16: release 1.4.3, fix for implicit tiling - missing b3dm's on high z-levels

2023-02-02: release 1.4.2, fix subtree files generation

2023-02-01: release 1.4.1, fix global tool

2023-02-01: release 1.4, adding tree of subtree files support

2023-01-10: release 1.3, adding LOD support

2022-12-13: release 1.2.3, fixing parameter use_implicit_tiling

2022-08-30: release 1.2.2, fixing initial boundingbox issue

2022-08-29: release 1.2.1 

- Fixing debug boundingVolumes and query parameter;

- Option 'use_implicit_tiling' default value changed from False to True;

2022-08-24: release 1.1: adding parameters sql_command_timeout (default: 30 seconds) and boundingvolume_heights (default: 0,100)

2022-08-23: release 1.0

Use a quadtree tiling method by default, fix skewed bounding volumes in Cesium.

MapBox GL JS support is discontinued at the moment.

Breaking changes:

- removed: parameter -i, --idcolumn

- removed: parameter -e, --extenttile

- renamed: parameter implicit_tiling_max_features to max_features_per_tile

2022-08-09: release 0.16, fixing materials (MetallicRoughness and SpecularGlossiness)

2022-08-09: release 0.15, use 1 geometric error for implicit tiling

2022-07-20: release 0.14, adding 3D Tiles 1.1 implicit tiling option

2022-07-05: release 0.13, adding glTF asset copyright

2022-01-24: release 0.12, to .NET 6, fixing decimal symbols regional settings on Windows

2021-10-27: release 0.11.2, fixing non latin characters issue in batch table

2021-09-30: release 0.11, adding multiple attribute columns support. 0.11.1 contains bug fix for batch table length 

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
