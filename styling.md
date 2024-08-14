# Styling 3D Tiles

There are 2 ways to style 3D Tiles:

1] Client side styling

The 3D Tiles are styled on the client side (in the browser) using the CesiumJS API.

2] Server side styling

The 3D Tiles are styled on the server side (in the database) using a json document.


## Client side styling

When using client side styling the 3D Tiles are styled in the client. In CesiumJS there is a 3D Tiles Styling Language.
For the specs of 3D Tiles Styling Language see https://github.com/CesiumGS/3d-tiles/tree/main/specification/Styling 

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


## Server side styling

When using server side styling the 3D Tiles are styled in the database. 

The styling is stored in a json document in a column of type json. A hex color notation is used for the colors, in the form of #RRGGBBAA.

The json document contains the shaders for the 3D Tiles. When the option --shaderscolumn is used, the shaders are read from the column specified in the option.

When the option --shaderscolumn is not used, a default PbrMetallicRoughness shader is used for all triangles in the geometry, 
witht following properties:

- BaseColor: #FFFFFF (option --default_color)

R = 255, G = 255, B = 255, A = 1 

- MetallicRoughness: #008000 (option --default_metallic_roughness)

Metallic factor: 0, Roughness factor: 0.5019608 (128/255)

- Doubleside: true (option double_sided)

Alternative option is to specify a shader using the ShadersColumn.

Shaderscolumn is a column of type json. In the json documents the shaders are defined like PbrMetallicRoughness and
PbrSpecularGlossiness. Note: PbrSpecularGlossiness is deprecated by Khronos, so advise is to use PbrMetallicRoughness.

### JSON Structure

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

### Rules for amount of shaders

- The amount of colors in the lists can be 1, in  which case the same color is used for all 
triangles in the geometry;

Example:

```
update delaware_buildings set simple_shader = 
'{
    "PbrMetallicRoughness": {
        "BaseColors": ["#ff0000"]
    }
}';
```

- For collection types (like MultiPolygon, MultiLine or PolyhedralSurface) the number of shaders can be equal to the number of 
inner geometries . In this case each inner geometry is styled with the corresponding shader. 

- The number of shaders can be equal to the number of triangles (of the generated mesh). The amount of triangles must
    be known in advance.

If the amount of colors is otherwise an exception is thrown.

Example: 

Consider a Multipolygon geometry of 2 squares:

```
MULTIPOLYGON Z(((0 0 0, 0 1 0, 1 1 0, 1 0 0, 0 0 0)),((2 2 0, 2 3 0, 3 3 0, 3 2 0, 2 2 0)))
```

The number of shaders can be:

- 1: all triangles are styled with the same shader;

```
{
  "PbrMetallicRoughness": {
    "BaseColors": [
      "#008000"
    ]
  }
}
```


- 2: each square is styled with a different shader;

```
{
  "PbrMetallicRoughness": {
    "BaseColors": [
      "#008000", 
      "#FF0000"
    ]
  }
}
```

- 4: each triangle is styled with a different shader.

```
{
  "PbrMetallicRoughness": {
    "BaseColors": [
        "#008000",
        "#FF0000",
        "#EEC900",
        "#EEC900"
    ]
  }
}
```

### Sql

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

### Samples

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

### AlphaMode

It is possible to specify glTF material alphaMode property (see: https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#alpha-coverage) using `--default_alpha_mode` option for all materials. By default it is set to OPAQUE. Other options are BLEND and MASK.
Using non OPAQUE default alpha mode for all materials might affect rendering performance. It should be used only when most of the materials have alpha value different than 1. 

### Remarks

- Fallback scenario from SpecularGlossiness to MetallicRoughness shader for clients that do not support 
SpecularGlossiness is not supported (yet)

- Shader 'unlit' is not supported (yet)

