using CommandLine;
using SharpGLTF.Materials;

namespace GltfExport;

public class Options
{
    [Option("connection", Required = true, HelpText = "Database connection string.")]
    public string Connection { get; set; } = string.Empty;

    [Option('o', "output", Required = false, Default = "output", HelpText = "Output directory.")]
    public string Output { get; set; } = "output";

    [Option('t', "table", Required = true, HelpText = "Database table, include database schema if needed.")]
    public string Table { get; set; } = string.Empty;

    [Option('c', "column", Required = false, Default = "geom", HelpText = "Geometry column.")]
    public string GeometryColumn { get; set; } = "geom";

    [Option("shaderscolumn", Required = false, Default = "", HelpText = "Shaders column.")]
    public string ShadersColumn { get; set; } = string.Empty;

    [Option("idcolumn", Required = false, Default = "id", HelpText = "Id column.")]
    public string IdColumn { get; set; } = "id";

    [Option("default_color", Required = false, Default = "#FFFFFF", HelpText = "Default color, in RGB(A) order.")]
    public string DefaultColor { get; set; } = "#FFFFFF";

    [Option("default_metallic_roughness", Required = false, Default = "#008000", HelpText = "Default metallic roughness.")]
    public string DefaultMetallicRoughness { get; set; } = "#008000";

    [Option("double_sided", Required = false, Default = true, HelpText = "Default double sided.")]
    public bool DoubleSided { get; set; } = true;

    [Option("default_alpha_mode", Required = false, Default = AlphaMode.OPAQUE, HelpText = "Default glTF material AlphaMode. Other values: BLEND and MASK. Defines how the alpha value is interpreted.")]
    public AlphaMode DefaultAlphaMode { get; set; } = AlphaMode.OPAQUE;

    [Option("alpha_cutoff", Required = false, Default = 0.5f, HelpText = "Default glTF material AlphaCutoff (used with MASK alpha mode).")]
    public float AlphaCutoff { get; set; } = 0.5f;
}
