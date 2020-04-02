# Release notes 0.9.0

Release 0.9 has some major changes:

- Tileset.json refinement method 'REPLACE' is used instead of 'ADD'

- Level of detail (LOD) functionality is added (option -l --lodcolumn)

- Functionality with building quadtree with features ordered on area size is replaced with a simpler system:

all features are placed into tiles regardless of area (tilesize: extenttile). When LOD functionality is used the whole tile is replaced with (1) other level tile.

- Option  -f, --featurespertile is removed

- Option -g, --geometricerror is added (default 500)

- More feedback messages to console

## Testing

### Docker

https://hub.docker.com/layers/geodan/pg2b3dm/0.9.0/images/sha256-c9b7478dec60f75e72d91d0f241b280b70e899995e989b74af05644594fb078c?context=explore

```
$ docker pull geodan/pg2b3dm:0.9.0
$ docker run geodan/pg2b3dm:0.9.0
tool: pg2b3dm 0.9.0.0
pg2b3dm 0.9.0
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


## Discussions / Known issues

- Performance: Step 'Calculating features per tile' with database queries is perhaps unnecessary (because extent tile is already known), to be investigated;

- LOD Functionality: Consider replacing a tile with 4 tiles on other LOD instead of replacing with 1 tile. The more detailed tile can grow too large when detailed 
geometries are used (depending on value of extenttile of course);

- GeometricError: There is an extra option for maximal Geometric Error (-g), default 500

When 2 lods (0,1) are used the used geometric errors (in tileset.json) are:

- root.GeometricError: 500

- lod tiles level 0: 250

- lod tiles level 1: 0

So an equal interval method is used. Is this workable? Consider adding optional geometric errors parameters.
