using CommandLine;

namespace pg2b3dm
{
    public class Options
    {
        [Option('H', "host", Required = true, HelpText = "Database host")]
        public string Host { get; set; }
        [Option('D', "database", Required = true, HelpText = "Database name")]
        public string Database { get; set; }
        [Option('c', "column", Required = true, HelpText = "Geometry column")]
        public string GeometryColumn { get; set; }
        [Option('t', "table", Required = true, HelpText = "Database table")]
        public string GeometryTable { get; set; }
        [Option('u', "user", Required = true, HelpText = "Database user")]
        public string User { get; set; }
        [Option('p', "password", Required = true, HelpText = "Database password")]
        public string Password { get; set; }

    }
}
