using Npgsql;
using pg2b3dm;

namespace pg2b3dm.database.tests;

public class ConnectionStringResolverTests
{
    [Test]
    public void Resolve_UsesExplicitConnectionWithoutAddingCommandTimeout()
    {
        var connection = "Host=db.example;Username=alice;Database=gis;Ssl Mode=Require;Command Timeout=15";
        var options = new pg2b3dm.Options
        {
            Connection = connection
        };

        var result = ConnectionStringResolver.Resolve(["--connection", connection], options, "fallback-user");
        var builder = new NpgsqlConnectionStringBuilder(result.ConnectionString);

        Assert.Multiple(() =>
        {
            Assert.That(builder.Host, Is.EqualTo("db.example"));
            Assert.That(builder.Username, Is.EqualTo("alice"));
            Assert.That(builder.Database, Is.EqualTo("gis"));
            Assert.That(builder.SslMode, Is.EqualTo(SslMode.Require));
            Assert.That(builder.CommandTimeout, Is.EqualTo(15));
            Assert.That(result.UserName, Is.EqualTo("alice"));
            Assert.That(result.Warnings, Is.Empty);
        });
    }

    [Test]
    public void Resolve_LegacyParametersBuildConnectionAndWarn()
    {
        var options = new pg2b3dm.Options
        {
            Host = "db.example",
            User = "legacy-user",
            Database = "legacy-db",
            Port = "5433"
        };

        var result = ConnectionStringResolver.Resolve(["-h", "db.example", "-U", "legacy-user", "-d", "legacy-db", "-p", "5433"], options, "fallback-user");
        var builder = new NpgsqlConnectionStringBuilder(result.ConnectionString);

        Assert.Multiple(() =>
        {
            Assert.That(builder.Host, Is.EqualTo("db.example"));
            Assert.That(builder.Port, Is.EqualTo(5433));
            Assert.That(builder.Username, Is.EqualTo("legacy-user"));
            Assert.That(builder.Database, Is.EqualTo("legacy-db"));
            Assert.That(builder.CommandTimeout, Is.EqualTo(0));
            Assert.That(result.UserName, Is.EqualTo("legacy-user"));
            Assert.That(result.Warnings, Has.Count.EqualTo(1));
            Assert.That(result.Warnings[0], Does.Contain("--username"));
            Assert.That(result.Warnings[0], Does.Contain("--host"));
            Assert.That(result.Warnings[0], Does.Contain("--dbname"));
            Assert.That(result.Warnings[0], Does.Contain("--port"));
            Assert.That(result.Warnings[0], Does.Contain("--connection"));
            Assert.That(result.Warnings[0], Does.Contain("CommandTimeOut=0"));
        });
    }

    [Test]
    public void Resolve_ConnectionOverridesLegacyParametersAndWarns()
    {
        var connection = "Host=override-host;Username=preferred-user;Database=preferred-db;Command Timeout=12";
        var options = new pg2b3dm.Options
        {
            Connection = connection,
            Host = "ignored-host",
            User = "ignored-user",
            Database = "ignored-db",
            Port = "5433"
        };

        var result = ConnectionStringResolver.Resolve(["--connection", connection, "--host", "ignored-host", "--username", "ignored-user"], options, "fallback-user");
        var builder = new NpgsqlConnectionStringBuilder(result.ConnectionString);

        Assert.Multiple(() =>
        {
            Assert.That(builder.Host, Is.EqualTo("override-host"));
            Assert.That(builder.Username, Is.EqualTo("preferred-user"));
            Assert.That(builder.Database, Is.EqualTo("preferred-db"));
            Assert.That(builder.CommandTimeout, Is.EqualTo(12));
            Assert.That(result.UserName, Is.EqualTo("preferred-user"));
            Assert.That(result.Warnings, Has.Count.EqualTo(1));
            Assert.That(result.Warnings[0], Does.Contain("--connection takes precedence"));
            Assert.That(result.Warnings[0], Does.Contain("--host"));
            Assert.That(result.Warnings[0], Does.Contain("--username"));
        });
    }

    [Test]
    public void Resolve_DefaultsToCurrentUserWhenNoConnectionOptionsAreProvided()
    {
        var result = ConnectionStringResolver.Resolve(Array.Empty<string>(), new pg2b3dm.Options(), "current-user");
        var builder = new NpgsqlConnectionStringBuilder(result.ConnectionString);

        Assert.Multiple(() =>
        {
            Assert.That(builder.Host, Is.EqualTo("localhost"));
            Assert.That(builder.Port, Is.EqualTo(5432));
            Assert.That(builder.Username, Is.EqualTo("current-user"));
            Assert.That(builder.Database, Is.EqualTo("current-user"));
            Assert.That(builder.CommandTimeout, Is.EqualTo(0));
            Assert.That(result.UserName, Is.EqualTo("current-user"));
            Assert.That(result.Warnings, Is.Empty);
        });
    }
}
