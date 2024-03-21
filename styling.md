# Styling 3D Tiles

## Shaders

By default (when option --shaderscolumn is not used), the following PbrMetallicRoughness shader is used with parameters for all triangles:

- BaseColor: #FFFFFF (option --default_color)

R = 255, G = 255, B = 255, A = 1 

- MetallicRoughness: #008000 (option --default_metallic_roughness)

Metallic factor: 0, Roughness factor: 0.5019608 (128/255)

- Doubleside: true (hardcoded)

- Alpha: 0 (hardcoded)

Alternative option is to specify a shader using the ShadersColumn.

Shaderscolumn is a column of type json. In this json document the shaders are defined like PbrMetallicRoughness and
PbrSpecularGlossiness. Note: PbrSpecularGlossiness is deprecated by Khronos, so advise is to use PbrMetallicRoughness.

## JSON Structure

The json must have the following structure:

```
{
    "EmissiveColors": [list_of_emissivecolors in hex],
    "PbrMetallicRoughness": {
        "BaseColors": [ list_of_basecolors in hex],
        "MetallicRoughness": [list_of_metallic_roughness in hex]
    },
    "PbrSpecularGlossiness": {
        "DiffuseColors": [list_of_diffuse in hex],
        "SpecularGlossiness": [list_of_specular_glossiness in hex]
    }
}
```

The amount of colors in the lists 

- must correspond to the number of triangles in the geometry;

- or be 1, in which case the same color is used for all triangles in the geometry;

Example:

```
update delaware_buildings set simple_shader = 
'{
    "PbrMetallicRoughness": {
        "BaseColors": ["#ff0000"]
    }
}';
```

- otherwise an exception is thrown.


Warning: When using a shader per triangle, the input geometries must be triangulated for this to work. Otherwise pg2b3dm will triangulate the geometries and the number of triangles will be unknown.

## Sql

Sample query in SQL:

```
ALTER TABLE mytable ADD COLUMN simple_shader json;

update mytable set simple_shader = 
'{
    "PbrMetallicRoughness": {
        "BaseColors": ["#008000", "#008000"]
    }
}';
```

## Samples

Sample for using shader PbrMetallicRoughness with BaseColor for 2 triangles:

```
{
    "PbrMetallicRoughness": {
        "BaseColors": ["#008000","#008000"]
    }
}
```

Sample for Specular Glossiness with Diffuse and SpecularGlossiness for 2 triangles :

```
{
    "PbrSpecularGlossiness": {
        "DiffuseColors": ["#E6008000","#E6008000"],
        "SpecularGlossiness": ["#4D0000ff", "#4D0000ff"]
    }
}
```


In the hexadecimal values there are 4 numbers (x, y, z, w) available. The following material channels table defines which number should be used for the various shader properties.

## Material channels

<table>
<thead>
<tr>
<th>Channel</th>
<th>Shader Style</th>
<th>X</th>
<th>Y</th>
<th>Z</th>
<th>W</th>
</tr>
</thead>
<tbody>
<tr>
<tr>
<td>Emissive</td>
<td>All</td>
<td>Red</td>
<td>Green</td>
<td>Blue</td>
<td></td>
</tr>
<tr>
<td>BaseColor</td>
<td>Metallic Roughness</td>
<td>Red</td>
<td>Green</td>
<td>Blue</td>
<td>Alpha</td>
</tr>
<tr>
<td>MetallicRoughness</td>
<td>Metallic Roughness</td>
<td>Metallic Factor</td>
<td>Roughness Factor</td>
<td></td>
<td></td>
</tr>
<tr>
<td>Diffuse</td>
<td>Specular Glossiness</td>
<td>Diffuse Red</td>
<td>Diffuse Green</td>
<td>Diffuse Blue</td>
<td>Alpha</td>
</tr>
<tr>
<td>SpecularGlossiness</td>
<td>Specular Glossiness</td>
<td>Specular Red</td>
<td>Specular Green</td>
<td>Specular Blue</td>
<td>Glossiness</td>
</tr>
</tbody>
</table>

Sample channel conversion:

- DiffuseColor in Hex = '#E6008000'

Converted to RGBA:

(230, 0, 128, 0)

So Diffuse Red = 230, Diffuse Green = 0, Diffuse Blue = 128, Alpha = 0

## Remarks

- Fallback scenario from SpecularGlossiness to MetallicRoughness shader for clients that do not support 
SpecularGlossiness is not supported (yet)

- Shader 'unlit' is not supported (yet)

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
