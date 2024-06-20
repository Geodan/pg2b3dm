# Styling 3D Tiles

## Shaders

By default the following PbrMetallicRoughness shader is used with parameters for all triangles:

- BaseColor: #FFFFFF (option --default_color)

R = 255, G = 255, B = 255, A = 1 

- MetallicRoughness: #008000 (option --default_metallic_roughness)

Metallic factor: 0, Roughness factor: 0.5019608 (128/255)

- Doubleside: true (hardcoded)

- Alpha: 0 (hardcoded)

## Client side styling

An alternative option is to style the 3D Tiles on runtime (in the client).

Example for styling buildings in a 3D Tileset based on attribute 'bouwjaar' in CesiumJS:

```
   var buildings = new Cesium.Cesium3DTileset({
        url : './buildings/tileset.json'
    });

    buildings.style = new Cesium.Cesium3DTileStyle({
      color: {
        conditions: [
        ["${feature['bouwjaar']} <= 1700", "color('#430719')"],
        ["${feature['bouwjaar']} > 1700", "color('#740320')"],
        ]
      }
    }
  );
```

Example for showing a subset based on a query (show only buildings with bouwjaar > 1975):

```
buildings.style.show = "${feature['bouwjaar']} > 1975"
```
Remember to add attribute bouwjaar with '-a bouwjaar' when creating the 3D Tiles.

For the specs of 3D Tiles Styling Language see https://github.com/CesiumGS/3d-tiles/tree/main/specification/Styling 
