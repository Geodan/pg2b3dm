using CommandLine;
using SharpGLTF.Materials;

namespace pg2b3dm;


public class Options
{
    [Option('U', "username", Required = false, HelpText = "Database user")]
    public string User { get; set; }
    [Option('h', "host", Required = false, Default = "localhost", HelpText = "Database host")]
    public string Host { get; set; }
    [Option('d', "dbname", Required = false, HelpText = "Database name")]
    public string Database { get; set; }
    [Option('c', "column", Required = false, Default = "geom", HelpText = "Geometry column")]
    public string GeometryColumn { get; set; }
    [Option('t', "table", Required = true, HelpText = "Database table, include database schema if needed")]
    public string GeometryTable { get; set; }
    [Option('p', "port", Required = false, Default = "5432", HelpText = "Database port")]
    public string Port { get; set; }
    [Option('o', "output", Required = false, Default = "output", HelpText = "Output path")]
    public string Output { get; set; }
    [Option('a', "attributecolumns", Required = false, Default = "", HelpText = "Attribute columns")]
    public string AttributeColumns { get; set; }

    [Option('q', "query", Required = false, Default = "", HelpText = "Query parameter")]
    public string Query { get; set; }

    [Option("copyright", Required = false, Default = "", HelpText = "glTF asset copyright")]
    public string Copyright { get; set; }

    [Option("default_color", Required = false, Default = "#FFFFFF", HelpText = "Default color, in (A)RGB order")]
    public string DefaultColor { get; set; }

    [Option("default_metallic_roughness", Required = false, Default = "#008000", HelpText = "Default metallic roughness")]
    public string DefaultMetallicRoughness { get; set; }

    [Option("double_sided", Required = false, Default = true, HelpText = "double sided")]
    public bool? DoubleSided { get; set; }
    
    [Option("default_alpha_mode", Required = false, Default = AlphaMode.OPAQUE, HelpText = "Default glTF material AlphaMode. Other values: BLEND and MASK. Defines how the alpha value is interpreted.")]
    public AlphaMode DefaultAlphaMode { get; set; }

    [Option("create_gltf", Required = false, Default = true, HelpText = "Create glTF")]
    public bool? CreateGltf { get; set; }

    [Option("radiuscolumn", Required = false, Default = "", HelpText = "Radius column")]
    public string RadiusColumn { get; set; }

    [Option('f', "format", Required = false, Default = AppMode.Cesium, HelpText = "Format (Mapbox or Cesium)")]
    public AppMode AppMode { get; set; }

    [Option("shaderscolumn", Required = false, Default = "", HelpText = "shaders column")]
    public string ShadersColumn { get; set; }

    // cesium specific options
    [Option("max_features_per_tile", Required = false, Default = 1000, HelpText = "maximum features per tile (Cesium)", SetName = "Cesium")]
    public int MaxFeaturesPerTile { get; set; }

    [Option('l', "lodcolumn", Required = false, Default = "", HelpText = "LOD column (Cesium)", SetName = "Cesium")]
    public string LodColumn { get; set; }

    [Option('g', "geometricerror", Required = false, Default = 2000, HelpText = "Geometric error (Cesium)", SetName = "Cesium")]
    public double GeometricError{ get; set; }

    [Option("geometricerrorfactor", Required = false, Default = 2, HelpText = "Geometric Error factor (Cesium)", SetName = "Cesium")]
    public double GeometricErrorFactor { get; set; }

    [Option("use_implicit_tiling", Required = false, Default = true, HelpText = "use 1.1 implicit tiling (Cesium)", SetName = "Cesium")]
    public bool? UseImplicitTiling { get; set; }

    [Option("add_outlines", Required = false, Default = false, HelpText = "Add outlines (Cesium)", SetName = "Cesium")]
    public bool? AddOutlines { get; set; }

    [Option('r', "refinement", Required = false, Default = "ADD", HelpText = "Refinement option REPLACE/ADD (Cesium)", SetName = "Cesium")]
    public string Refinement{ get; set; }

    [Option("skip_create_tiles", Required = false, Default = false, HelpText = "Skip creating tiles, only create tileset.json files (Cesium)", SetName = "Cesium")]
    public bool SkipCreateTiles { get; set; }

    // mapbox specific options
    [Option("zoom", Required = false, Default = 15, HelpText = "Zoom level (Mapbox)", SetName = "Mapbox")]
    public int Zoom { get; set; }
 
}
