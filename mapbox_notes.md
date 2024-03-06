## Experimental Mapbox GL JS Support

In pg2b3dm release 2.6.0 experimental support for Mapbox GL JS v3 is added.

See announcement https://www.mapbox.com/blog/standard-core-style

To get things running (see also https://github.com/Geodan/pg2b3dm/blob/master/getting_started.md):

- Run pg2b3dm 2.6.0 or higher, use --format Mapbox and Mapbox specific parameters zoom (default 15);

- Tiles are written in format {z}-{x}-{y}.b3dm or {z}-{x}-{y}.glb in the content directory;

- Add the resulting tiles as 'batched-model' to your Mapbox GL JS v3 viewer;

- Draco compress the resulting tiles.

Sample code:

```
map.addSource('3d tiles', {
        "type": "batched-model",
        "maxzoom": 15,
        "minzoom": 15,
        "tiles": [
          "http://localhost:2015/content/{z}-{x}-{y}.glb"
        ]
      }
)});
```

Limitiations:

- Terrain

- Query attributes

- Styling
