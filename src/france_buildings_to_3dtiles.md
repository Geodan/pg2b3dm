# France buildings to 3D Tiles

This tutorial shows how to convert a set of 3D buildings in France to 3D Tiles using the pg2b3dm command line tool.

The following steps are executed:

1] Download the 3D buildings in France from the French Cadastre;

2] Load and process the 3D buildings in a PostgreSQL database;

3] Create the 3D Tiles using pg2b3dm;

4] Visualize in CesiumJS.

## Prerequisites

- PostgreSQL database with PostGIS extension;

- Sufficient disk (200GB+) and database (200GB+) space;

## Download

The buildings of France are available in the French Cadastre. You can download the data from the following link:

https://www.data.gouv.fr/en/datasets/base-de-donnees-nationale-des-batiments/

There is a database dump file, named 'BDNB - Export france - pgdump' - (21.4 GB)

```
$ wget https://open-data.s3.fr-par.scw.cloud/bdnb_millesime_2023-01-a/millesime_2023-01-a_france/open_data_millesime_2023-01-a_france_pgdump.tar.gz


```

Unzip the data:

```
$ tar -xf open_data_millesime_2023-01-a_france_pgdump.tar.gz
```

Result is a file 'bdnb.sql' (109 GB)

## Load and process the 3D buildings in a PostgreSQL database

Load the dump file using psql, the tables are created in schema 'bdnb_2023_01_a_open_data_france'

```
$ psql -h myserver -p 5432 -U postgres -d mydb -f bdnb.sql  > log_extract.log 2>log_extract_erreurs.log
```

The building geometries are stored in the table 'batiment_construction', it has 46286334 rows.

To create 3D geometries, we need to use information from the following columns:

- 'geom_cstr - multipolygon geometry of the buildings in Lambert 93 projection (EPSG:2154);

- 'hauteur' - height of the buildings in meters;

- 'altitude_sol': altitude of the ground in meters.


First we have to fix some data issues:

1] Buildings without height information

There are 345879 buildings without height information. We will assign a default height of 2 meters.

```
postgresql> update bdnb_2023_01_a_open_data_france.batiment_construction set hauteur = 2 where hauteur is null;
```

2] Buildings with invalid geometries

There are about 30 buildings with invalid geometries (area is 0). We will remove them.

```
postgresql> delete from bdnb_2023_01_a_open_data_france.batiment_construction where st_isvalid(geom_cstr) = false;
```

3] Fix altitude_sol

The column altitude_sol is missing in 1 million cases. It's also not alligned to the Cesium terrain. We will create a new column 'altitude_sol_fixed' and 
fill it with new values. 

Add a colum:

```
 ALTER TABLE bdnb_2023_01_a_open_data_france.batiment_construction ADD COLUMN altitude_sol_fixed float8;
```

DEM used: https://ec.europa.eu/eurostat/web/gisco/geodata/reference-data/elevation/eu-dem/eu-dem-dd

On leda it's stored in /mnt/sdh/bertt/france_buildings/eu_dem

Note it doesn't cover longitude < -5 buildings...

Tool (experimental): https://github.com/bertt/fix_altitude_sol

On leda run in Docker with: 

```
$ docker run -it -v /mnt/sdh/bertt/france_buildings/eu_dem:/dem fix_altitude_sol -d /dem/mosaic.tif -c "leda.geodan.nl;Username=postgres;Database=research;Port=
5432;CommandTimeOut=0"
```

Should be finished in 24 hours.

Now create a new geometry column for storing the 3D geometries:

```
postgresql> alter table bdnb_2023_01_a_open_data_france.batiment_construction add column geom geometry;
```

And fill it using the 'hauteur' and 'altitude_sol_fixed' columns. We'll also transform the geometries from EPSG:5698 (that's horizontal EPSG:2154 + vertical EPSG:5720 for mainland France) to EPSG:4979 (that's horizontal EPSG:4326 + vertical height above the ellipsoid). 
With this conversion we can use the terrain option in 3D Tiles clients like CesiumJS.

Note: this query will take several hours, depending on the hardware. Table size will increase to 142 GB.

```
postgresql> update bdnb_2023_01_a_open_data_france.batiment_construction set geom = st_transform(ST_SetSRID(st_translate(ST_CollectionExtract(st_Extrude(geom_cstr, 0, 0, hauteur),3),0,0,altitude_sol_fixed),5698), 4979); 
```

(about 7 hours)

For Corsica there is a different vertical reference system (EPSG:5722 - compound EPSG:5699), so we need adjust those buildings:

```
postgresql> update bdnb_2023_01_a_open_data_france.batiment_construction set geom = st_transform(ST_SetSRID(st_translate(ST_CollectionExtract(st_Extrude(geom_cstr, 0, 0, hauteur),3),0,0,altitude_sol_fixed),5699), 4979) 
where code_departement_insee = '2A' OR  code_departement_insee = '2B'
```

(about 1 hour) 

Create a new spatial index on the the geom column:

```
postgresql> CREATE INDEX ON bdnb_2023_01_a_open_data_france.batiment_construction USING gist(st_centroid(st_envelope(geom)));
```

And a view 'v_batiments' for more attributes:

```
postgresql> create or replace view bdnb_2023_01_a_open_data_france.v_batiments as 
select bc.geom as geom, bc.batiment_construction_id as id,bc.code_departement_insee as departement,bc.s_geom_cstr as area,bc.hauteur as hauteur,bc.altitude_sol as altitude_sol, a.libelle_adresse as adresse, a.code_postal as code_postal
FROM bdnb_2023_01_a_open_data_france.batiment_construction bc
LEFT JOIN bdnb_2023_01_a_open_data_france.rel_batiment_construction_adresse rel ON bc.batiment_construction_id = rel.batiment_construction_id
left join bdnb_2023_01_a_open_data_france.adresse a on rel.cle_interop_adr = a.cle_interop_adr
 
```

with construction_year?

```
postgresql> create or replace view bdnb_2023_01_a_open_data_france.v_batiments as 
select batiment_groupe_dpe_representatif_logement.annee_construction_dpe as annee_construction, bc.geom as geom, bc.batiment_construction_id as id,
bc.code_departement_insee as departement,bc.s_geom_cstr as area,bc.hauteur as hauteur,bc.altitude_sol as altitude_sol, a.libelle_adresse as adresse, a.code_postal as code_postal
FROM bdnb_2023_01_a_open_data_france.batiment_construction bc
LEFT JOIN bdnb_2023_01_a_open_data_france.rel_batiment_construction_adresse rel ON bc.batiment_construction_id = rel.batiment_construction_id
left join bdnb_2023_01_a_open_data_france.adresse a on rel.cle_interop_adr = a.cle_interop_adr 
LEFT JOIN bdnb_2023_01_a_open_data_france.batiment_groupe groupe ON bc.batiment_groupe_id = groupe.batiment_groupe_id
left join bdnb_2023_01_a_open_data_france.rel_batiment_groupe_dpe_logement dpe_logement on  groupe.batiment_groupe_id = dpe_logement.batiment_groupe_id
left join bdnb_2023_01_a_open_data_france.batiment_groupe_dpe_representatif_logement batiment_groupe_dpe_representatif_logement on dpe_logement.identifiant_dpe = batiment_groupe_dpe_representatif_logement.identifiant_dpe
```


## Create the 3D Tiles using pg2b3dm

Create the 3D Tiles using the pg2b3dm command line tool in Docker. 

```
$ docker run -it -v /mnt/sdh/bertt/france_buildings/1.0:/app/output geodan/pg2b3dm -h leda.geodan.nl -d research -U postgres -c geom -t bdnb_2023_01_a_open_data_france.v_batiments -a id,departement,area,hauteur,altitude_sol,adresse,code_postal,annee_construction --use_implicit_tiling false --create_gltf false --add_outlines true
```

The 3D Tiles are created in the '/mnt/sdh/bertt/france_buildings/1.0' directory. Creating the tiles takes about 8 hours.

Optional: Compress the 3D Tiles using Draco using Compressor5000 (https://github.com/geodan/compressor5000) using outlines (option -o true) 
- warning original tiles will be overwritten:


```
$ cd /mnt/sdh/bertt/france_buildings/1.0/content
$ docker run -v $(pwd):/tiles -it compressor5000 -o true
```

(about 8 hours?)

Copy a CesiumJS client (index.html) with reference to tileset.json to the 'output' directory and start a web server.

```
$ http-server -p 8001
```

## Visualize in CesiumJS

Open a browser and go to http://localhost:8001/index.html

You should see the 3D buildings of France in CesiumJS.


# Deploy to Google Cloud

Web interface: https://console.cloud.google.com/storage/browser/ahp-research;tab=objects?forceOnBucketsSortingFiltering=false&project=ahp-cluster&prefix=&forceOnObjectsSortingFiltering=false

```
$ gcloud auth login
```

Folder: ahp-research/maquette/ign/buildings/beta.2

dataset: 44GB

```
$ cd /mnt/sdh/bertt/france_buildings
$ gsutil -m cp -r index.html gs://ahp-research/maquette/ign/buildings/beta.2/
$ cd 1.0
$ gsutil -m cp -r "*.json" gs://ahp-research/maquette/ign/buildings/beta.2/1.0
$ cd content
$ gsutil -m cp -r "*.b3dm" gs://ahp-research/maquette/ign/buildings/beta.2/1.0/content
```

Result should be on: 

https://storage.googleapis.com/ahp-research/maquette/ign/buildings/beta.2/index.html

