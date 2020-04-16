# Release notes 0.9

Release 0.9 has some major changes:

- Tileset.json refinement method 'REPLACE' is used instead of 'ADD'

- Level of detail (LOD) functionality is added (option -l --lodcolumn)

- Functionality with building quadtree with features ordered on area size is replaced with a simpler system:

all features are placed into quadtree tiles regardless of area (tilesize: extenttile). NB: Tiles without features are removed 
from the quadtree (including children).

- Option -f, --featurespertile is removed

- Option -g, --geometricerrors is added (default 500,0)

Example for 2 lods: -g 1000,750,0

- More feedback messages to console

- Tile generation performance +  memory usage improvements

## Testing

### Docker

https://hub.docker.com/layers/geodan/pg2b3dm/0.9.2/images/sha256-c9b7478dec60f75e72d91d0f241b280b70e899995e989b74af05644594fb078c?context=explore

```
$ docker pull geodan/pg2b3dm:0.9.2
$ docker run geodan/pg2b3dm:0.9.2
tool: pg2b3dm 0.9.2.0
pg2b3dm 0.9.2
Copyright (C) 2020 pg2b3dm
```

### Source code

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ git checkout issue_5_refine_replace
$ cd pg2b3dm/src
$ dotnet build
$ dotnet run
```

## Testing regular functionality

The tool should work the same as before, remember option -f is removed. Changed behaviour in client application: all features are visibile or none (depending on GeometricError).

## Testing LOD functionality

- Add column 'lod' of type integer

- Fill column lod with values like 0, 1 (for two lods). LOD 0 for more general/simplified features (used when zoomed out), LOD 1 for more detailed tiles (used when zoomed in).

There can also be more than 2 LODS.

- Give unique id's to same geometry on different LOD's

- Change geometry and/or colors per LOD

- add option -l --lodcolumn when running pg2b3dm

Warning: tile creation will take more time when LOD's are used.

Result should look like https://bertt.github.io/mapbox_3dtiles_samples/index.html (see blue or red buildings depending on distance to camera)

# 

History

- 0.9.2: create a real quadtree for lods instead of single subchild

- 0.9.1: goption -g geometric errors introduced

- 0.9.0: initial version of refinement=replace 
