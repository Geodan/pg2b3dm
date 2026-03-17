using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace pg2b3dm;

public sealed record ConnectionStringResolution(string ConnectionString, string UserName, IReadOnlyList<string> Warnings);

public static class ConnectionStringResolver
{
    private const string DefaultHost = "localhost";
    private const string DefaultPort = "5432";
    private const string ExampleCommand = "pg2b3dm --connection \"Host=localhost;Username=postgres;Database=postgres;Ssl Mode=Require;CommandTimeOut=0\" -t my_schema.my_table";
    private static readonly string[] DeprecatedParameterOrder = ["--username", "--host", "--dbname", "--port"];
    private static readonly Dictionary<string, string> DeprecatedParameterAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["-U"] = "--username",
        ["--username"] = "--username",
        ["-h"] = "--host",
        ["--host"] = "--host",
        ["-d"] = "--dbname",
        ["--dbname"] = "--dbname",
        ["-p"] = "--port",
        ["--port"] = "--port",
    };

    public static ConnectionStringResolution Resolve(string[] args, Options options, string currentUserName)
    {
        var deprecatedParameters = GetDeprecatedParameters(args);
        var warnings = new List<string>();
        string connectionString;
        string userName;

        if (!string.IsNullOrWhiteSpace(options.Connection))
        {
            var builder = new NpgsqlConnectionStringBuilder(options.Connection);
            connectionString = builder.ConnectionString;
            userName = string.IsNullOrWhiteSpace(builder.Username) ? currentUserName : builder.Username;

            if (deprecatedParameters.Count > 0)
            {
                warnings.Add(CreateConnectionOverrideWarning(deprecatedParameters));
            }
        }
        else
        {
            var host = string.IsNullOrWhiteSpace(options.Host) ? DefaultHost : options.Host;
            var user = string.IsNullOrWhiteSpace(options.User) ? currentUserName : options.User;
            var database = string.IsNullOrWhiteSpace(options.Database) ? currentUserName : options.Database;
            var port = string.IsNullOrWhiteSpace(options.Port) ? DefaultPort : options.Port;

            connectionString = BuildLegacyConnectionString(host, user, database, port);
            userName = user;

            if (deprecatedParameters.Count > 0)
            {
                warnings.Add(CreateDeprecatedParametersWarning(deprecatedParameters));
            }
        }

        return new ConnectionStringResolution(connectionString, userName, warnings);
    }

    public static string AddPassword(string connectionString, string password)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Password = password
        };

        return builder.ConnectionString;
    }

    public static IReadOnlyList<string> GetDeprecatedParameters(string[] args)
    {
        var usedParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            if (TryGetDeprecatedParameter(arg, out var deprecatedParameter))
            {
                usedParameters.Add(deprecatedParameter);
            }
        }

        return DeprecatedParameterOrder
            .Where(usedParameters.Contains)
            .ToList();
    }

    private static bool TryGetDeprecatedParameter(string arg, out string deprecatedParameter)
    {
        deprecatedParameter = string.Empty;

        if (DeprecatedParameterAliases.TryGetValue(arg, out var matchedParameter))
        {
            deprecatedParameter = matchedParameter;
            return true;
        }

        var equalsIndex = arg.IndexOf('=');
        if (equalsIndex > 0)
        {
            var parameterName = arg[..equalsIndex];
            if (DeprecatedParameterAliases.TryGetValue(parameterName, out matchedParameter))
            {
                deprecatedParameter = matchedParameter;
                return true;
            }
        }

        if (arg.StartsWith('-') && !arg.StartsWith("--") && arg.Length > 2)
        {
            var shortParameterName = arg[..2];
            if (DeprecatedParameterAliases.TryGetValue(shortParameterName, out matchedParameter))
            {
                deprecatedParameter = matchedParameter;
                return true;
            }
        }

        return false;
    }

    private static string BuildLegacyConnectionString(string host, string user, string database, string port)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            ConnectionString = $"Host={host};Username={user};Database={database};Port={port};CommandTimeOut=0"
        };

        return builder.ConnectionString;
    }

    private static string CreateDeprecatedParametersWarning(IReadOnlyList<string> deprecatedParameters)
    {
        var parameterLabel = deprecatedParameters.Count == 1 ? "parameter" : "parameters";
        var verb = deprecatedParameters.Count == 1 ? "is" : "are";
        var parameterList = string.Join(", ", deprecatedParameters);

        return string.Join(Environment.NewLine, new[]
        {
            "-----------------------------------------------------------------------------",
            $"WARNING: Database {parameterLabel} {parameterList} {verb} deprecated and will be removed in a future version.",
            "Use --connection instead.",
            $"Example: {ExampleCommand}",
            "-----------------------------------------------------------------------------"
        });
    }

    private static string CreateConnectionOverrideWarning(IReadOnlyList<string> deprecatedParameters)
    {
        var parameterLabel = deprecatedParameters.Count == 1 ? "parameter" : "parameters";
        var verb = deprecatedParameters.Count == 1 ? "was" : "were";
        var parameterList = string.Join(", ", deprecatedParameters);

        return string.Join(Environment.NewLine, new[]
        {
            "-----------------------------------------------------------------------------",
            $"WARNING: Deprecated database {parameterLabel} {parameterList} {verb} provided together with --connection.",
            "--connection takes precedence and will be used.",
            $"Example: {ExampleCommand}",
            "-----------------------------------------------------------------------------"
        });
    }
}
