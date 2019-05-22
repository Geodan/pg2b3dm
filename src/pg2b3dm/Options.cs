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
    }
}
