using CommandLine;

namespace pg2b3dm
{
    public class Options
    {
        [Option('U', "username", Required = false, HelpText = "Database user")]
        public string User { get; set; }
        [Option('h', "host", Required = false, Default = "localhost",  HelpText = "Database host" )]
        public string Host { get; set; }
        [Option('d', "dbname", Required = false, HelpText = "Database name")]
        public string Database { get; set; }
        [Option('c', "column", Required = false, Default = "geom", HelpText = "Geometry column")]
        public string GeometryColumn { get; set; }
        [Option('t', "table", Required = true, HelpText = "Database table, include database schema if needed")]
        public string GeometryTable { get; set; }
        [Option('p', "port", Required = false, Default ="5432", HelpText = "Database port")]
        public string Port { get; set; }
        [Option('o', "output", Required = false, Default = "./output", HelpText = "Output path")]
        public string Output { get; set; }
        [Option('r', "roofcolorcolumn", Required = false, Default = "", HelpText = "Roof color column")]
        public string RoofColorColumn { get; set; }
        [Option('a', "attributescolumn", Required = false, Default = "", HelpText = "Attributes column")]
        public string AttributesColumn { get; set; }

        [Option('i', "idcolumn", Required = false, Default = "id", HelpText = "Id column")]
        public string IdColumn { get; set; }

        [Option('e', "extenttile", Required = false, Default = 2000.0, HelpText = "Maximum extent per tile")]
        public double ExtentTile{ get; set; }
        [Option('l', "lodcolumn", Required = false, Default = "", HelpText = "LOD column")]
        public string LodColumn { get; set; }

        [Option('g', "geometricerrors", Required = false, Default = "500,0", HelpText = "Geometric errors")]
        public string GeometricErrors { get; set; }
    }
}
