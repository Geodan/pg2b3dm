# Converting CityGML to 3D Tiles using 3DCityDB v5

## Overview

3DCityDB v5 is an open-source database designed for storing and managing semantic 3D city models based on the CityGML 3.0 standard. It runs on PostgreSQL with PostGIS and organizes city data into about 17 core tables that handle geometry, metadata, appearances, and relationships.  
This guide outlines how to install 3DCityDB v5, import CityGML data, export it as 3D Tiles, and visualize the results.

---

## 1. Installation  

3DCityDB v5 can be quickly deployed using Docker:

```bash
docker run -d -p 5440:5432 -it -e POSTGRES_PASSWORD=postgres -e PROJ_NETWORK=ON -e SRID=7415 3dcitydb/3dcitydb-pg
```

**Explanation:**
- `-p 5440:5432` — maps host port to container port  
- `POSTGRES_PASSWORD` — sets the database password  
- `PROJ_NETWORK=ON` — enables coordinate transformation downloads  
- `SRID=7415` — specifies the coordinate reference system (Amersfoort / RD New)  

After launching, the database schema is automatically created with tables for city objects, geometry data, attributes, and appearances.

---

## 2. Importing CityGML Data

Download a sample CityGML file, such as the Den Haag Archipelbuurt 3D model (45MB) from [https://ckan.dataplatform.nl/dataset/3d-stadsmodel-den-haag-2021-citygml/resource/be8d3a16-50f3-415f-a8bd-55d24c9d8cdc](https://ckan.dataplatform.nl/dataset/3d-stadsmodel-den-haag-2021-citygml/resource/be8d3a16-50f3-415f-a8bd-55d24c9d8cdc).

Example command for importing a CityGML file:

```bash
citydb import citygml   -H localhost   -d postgres   -u postgres   -p postgres   --db-port 5440   den_haag_3d_archipelbuurt.gml
```

**Notes:**
- The importer loads buildings and other features into the schema.  
- Geometries are stored as `ST_PolyhedralSurface` or `ST_MultiPolygon` in the `geometry_data` table.  
- Attribute data such as building functions and IDs are stored in related tables.  

---

## 3. Converting to 3D Tiles  

Once the data is imported, it can be converted into 3D Tiles for visualization using the `pg2b3dm` tool:

```bash
pg2b3dm   -U postgres   -h localhost   -l   -p 5440   -d postgres   -t citydb.geometry_data   -c geometry   --attributecolumns geometry_properties
```

**Result:**
- A `tileset.json` file describing the dataset’s structure and bounding volumes  
- Multiple subtree files defining hierarchical levels of detail  
- Binary `.b3dm` or `.glb` tiles ready for streaming  

---

## 4. Visualization  

The resulting 3D Tiles can be viewed in any Cesium-compatible viewer.  
When loaded, the 3D model displays buildings and terrain data with geometric accuracy and semantic detail.  
Optional styling can be added to control building colors, materials, or feature visibility.


<img src="denhaag_3dtiles.jpg" alt="3D Tiles Visualization of Den Haag"/>

---

## 5. Conclusion  

3DCityDB v5 streamlines the process of:
- Storing and querying CityGML 3.0 models in PostgreSQL/PostGIS  
- Converting them into web-ready 3D Tiles for efficient visualization  
- Supporting semantic 3D city models suitable for digital twin and urban planning applications  

Future improvements may include the direct handling of texture and material data to enrich the visual quality of the exported 3D Tiles.