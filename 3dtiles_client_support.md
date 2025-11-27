# 3D Tiles client support

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

