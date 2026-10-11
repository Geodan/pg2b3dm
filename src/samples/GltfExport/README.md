# GltfExport

A .NET 8.0 console tool that exports PostGIS geometries as individual glTF 2.0 (`.glb`) files — one file per row, named `{id}.glb`.

Geometry coordinates are used **as-is** (no translation or reprojection is applied).

## Requirements

- .NET 8.0 SDK
- A PostgreSQL database with PostGIS extension
- A table containing a geometry column and an id column

## Usage

```bash
gltfexport --connection "Host=localhost;Database=mydb;Username=myuser;Password=mypassword" \
           -t myschema.mytable \
           -o ./output
```

## Parameters

| Parameter | Short | Required | Default | Description |
|---|---|---|---|---|
| `--connection` | | Yes | | PostgreSQL connection string |
| `--table` | `-t` | Yes | | Database table (include schema if needed, e.g. `public.buildings`) |
| `--output` | `-o` | No | `output` | Output directory for `.glb` files |
| `--column` | `-c` | No | `geom` | Geometry column |
| `--idcolumn` | | No | `id` | Id column — used as the output filename |
| `--shaderscolumn` | | No | *(empty)* | Shaders column (JSON with PBR material colors) |
| `--default_color` | | No | `#FFFFFF` | Default color in RGB(A) order |
| `--default_metallic_roughness` | | No | `#008000` | Default metallic roughness |
| `--double_sided` | | No | `true` | Double-sided rendering |
| `--default_alpha_mode` | | No | `OPAQUE` | glTF AlphaMode: `OPAQUE`, `BLEND`, or `MASK` |
| `--alpha_cutoff` | | No | `0.5` | Alpha cutoff value (used with `MASK` alpha mode) |
| `--help` | | | | Display help |
| `--version` | | | | Display version information |

## Output

Each row in the table produces one `.glb` file in the output directory, named after the value in the id column (invalid filename characters are replaced with `_`).

## Example

Given a table `public.buildings` with columns `gid` (integer), `geom` (geometry), and `shader` (json):

```bash
gltfexport \
  --connection "Host=localhost;Database=citydb;Username=postgres" \
  -t public.buildings \
  -c geom \
  --idcolumn gid \
  --shaderscolumn shader \
  --default_color "#CCCCCC" \
  -o ./glb_output
```

This produces files like `./glb_output/1.glb`, `./glb_output/2.glb`, etc.
