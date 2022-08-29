using CommandLine;

namespace pg2b3dm
{
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
        [Option('l', "lodcolumn", Required = false, Default = "", HelpText = "LOD column")]
        public string LodColumn { get; set; }

        [Option('g', "geometricerrors", Required = false, Default = "2024,0", HelpText = "Geometric errors")]
        public string GeometricErrors { get; set; }

        [Option("refine", Required = false, Default = "REPLACE", HelpText = "Refinement method (ADD/REPLACE)")]
        public string Refinement { get; set; }

        [Option("shaderscolumn", Required = false, Default = "", HelpText = "shaders column")]
        public string ShadersColumn { get; set; }

        [Option('q', "query", Required = false, Default = "", HelpText = "Query parameter")]
        public string Query { get; set; }

        [Option("copyright", Required = false, Default = "", HelpText = "glTF asset copyright")]
        public string Copyright { get; set; }

        [Option("use_implicit_tiling", Required = true, Default = false, HelpText = "use 1.1 implicit tiling")]
        public bool UseImplicitTiling { get; set; }

        [Option("max_features_per_tile", Required = false, Default = 1000, HelpText = "maximum features per tile")]
        public int MaxFeaturesPerTile { get; set; }

        [Option("sql_command_timeout", Required = false, Default = 30, HelpText = "SQL command timeout")]
        public int SqlCommandTimeout { get; set; }

        [Option("boundingvolume_heights", Required = false, Default = "0,100", HelpText = "Tile boundingVolume heights (min, max) in meters")]
        public string BoundingVolumeHeights { get; set; }
    }
}
