using CommandLine;

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

    [Option("default_color", Required = false, Default = "#FFFFFF", HelpText = "Default color")]
    public string DefaultColor { get; set; }

    [Option("default_metallic_roughness", Required = false, Default = "#008000", HelpText = "Default metallic roughness")]
    public string DefaultMetallicRoughness { get; set; }

    [Option("double_sided", Required = false, Default = true, HelpText = "double sided")]
    public bool? DoubleSided { get; set; }

    [Option("create_gltf", Required = false, Default = true, HelpText = "Create glTF")]
    public bool? CreateGltf { get; set; }

    [Option("radiuscolumn", Required = false, Default = "", HelpText = "Radius column")]
    public string RadiusColumn { get; set; }

    [Option("appmode", Required = false, Default = AppMode.Cesium, HelpText = "App mode (Mapbox or Cesium)")]
    public AppMode AppMode { get; set; }

    // cesium specific options
    [Option("max_features_per_tile", Required = false, Default = 1000, HelpText = "maximum features per tile (Cesium)", SetName = "Cesium")]
    public int MaxFeaturesPerTile { get; set; }

    [Option('l', "lodcolumn", Required = false, Default = "", HelpText = "LOD column (Cesium)", SetName = "Cesium")]
    public string LodColumn { get; set; }

    [Option('g', "geometricerrors", Required = false, Default = "2000,0", HelpText = "Geometric errors (Cesium)", SetName = "Cesium")]
    public string GeometricErrors { get; set; }

    [Option("shaderscolumn", Required = false, Default = "", HelpText = "shaders column (Cesium)", SetName = "Cesium")]
    public string ShadersColumn { get; set; }

    [Option("use_implicit_tiling", Required = false, Default = true, HelpText = "use 1.1 implicit tiling (Cesium)", SetName = "Cesium")]
    public bool? UseImplicitTiling { get; set; }

    [Option("add_outlines", Required = false, Default = false, HelpText = "Add outlines (Cesium)", SetName = "Cesium")]
    public bool? AddOutlines { get; set; }

    [Option('r', "refinement", Required = false, Default = "REPLACE", HelpText = "Refinement option REPLACE/ADD (Cesium)", SetName = "Cesium")]
    public string Refinement{ get; set; }

    // mapbox specific options
    [Option("min_zoom", Required = false, Default = 15, HelpText = "Minimum zoom level (Mapbox)", SetName = "Mapbox")]
    public int MinZoom { get; set; }
    
    [Option("max_zoom", Required = false, Default = 15, HelpText = "Maximum zoom level (Mapbox) ", SetName = "Mapbox")]
    public int MaxZoom { get; set; }

}
