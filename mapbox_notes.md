## Experimental MapBox GL JS Support

In release 1.6 experimental support for MapBox GL JS v3 beta is added.

See announcement https://www.mapbox.com/blog/standard-core-style

To get things running (see also https://github.com/Geodan/pg2b3dm/blob/master/getting_started.md):

- Create EPSG:3857 triangulated geometries in your database;

- Run pg2b3dm 1.6 or higher, use Mapbox specific parameters min_zoom (default 15)/ max_zoom (default 15);

- Tiles are written in format {z}-{x}-{y}.b3dm in the content directory;

- Add the resulting tiles as 'batched-model' to your Mapbox GL JS v3 beta viewer

Sample code:

```
map.addSource('3d tiles', {
        "type": "batched-model",
        "maxzoom": 15,
        "minzoom": 15,
        "tiles": [
          "http://localhost:2015/content/{z}-{x}-{y}.b3dm"
        ]
      }
)});
```

Note: In the currrent MapBox GL JS 3 beta (v3.0.0-beta.1) version the tiles are requested but the glTF's are not rendered correct yet.

Live demo see https://geodan.github.io/pg2b3dm/sample_data/delaware/mapboxv3/ (zoom in a bit)

