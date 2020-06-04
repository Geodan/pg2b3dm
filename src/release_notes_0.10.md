# Release notes 0.10

## Breaking change

- Option -r, --roofcolorcolumn is removed and replaced by optional --shaderscolumn (default '')

When shaderscolumn is not specified a default PbrMetallicRoughness shader with BaseColor will 
be used.

## Shaders

Shaderscolumn is a column of type json. In the json document the shaders are defined like PbrMetallicRoughness and
PbrSpecularGlossiness. In previous releases of pg2b3dm only shader PbrMetallicRoughness with option BaseColor was 
supported. 

## JSON Structure

The json must have the following structure:

```
{
    "EmissiveColors": [list_of_emissivecolors in hex]
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

The amount of colors in the lists must correspond to the number of triangles in the geometry, otherwise an exception is thrown.

## Samples

Sample for using shader PbrMetallicRoughness with BaseColor for 2 triangles (note: this is the same method as used in previous releases):

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

## Remarks

- Fallback scenario from SpecularGlossiness to MetallicRoughness shader for clients that do not support 
SpecularGlossiness is not supported (yet)

- BaseColor of default material is not configureable (yet)

- Shader 'unlit' is not supported (yet)

## Testing

### Docker

```
$ docker pull geodan/pg2b3dm:0.10.0_preview1
$ docker run geodan/pg2b3dm:0.10.0_preview1
tool: pg2b3dm 0.10.0.0
pg2b3dm 0.10.0
Copyright (C) 2020 pg2b3dm
```

### Source code

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ git checkout shaders
$ cd pg2b3dm/src
$ dotnet build
$ dotnet run
```

## Testing shaders

- Add json column and fill with shader json documents

- run pg2b3dm and inspect the results in client of choice
