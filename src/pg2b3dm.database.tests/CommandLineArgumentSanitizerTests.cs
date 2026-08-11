namespace pg2b3dm.database.tests;

public class CommandLineArgumentSanitizerTests
{
    [Test]
    public void SanitizeForLogging_RedactsPasswordInConnectionString()
    {
        var args = new[]
        {
            "--connection",
            "Host=db.example;Username=alice;Password=secret;Database=gis",
            "-t",
            "public.buildings"
        };

        var sanitized = CommandLineArgumentSanitizer.SanitizeForLogging(args);

        Assert.That(sanitized, Is.EqualTo("--connection Host=db.example;Username=alice;Password=***;Database=gis -t public.buildings"));
    }

    [Test]
    public void SanitizeForLogging_RedactsPwdAliasCaseInsensitively()
    {
        var args = new[]
        {
            "--connection",
            "Host=db.example;Username=alice;PWD = secret;Database=gis"
        };

        var sanitized = CommandLineArgumentSanitizer.SanitizeForLogging(args);

        Assert.That(sanitized, Is.EqualTo("--connection Host=db.example;Username=alice;PWD = ***;Database=gis"));
    }

    [Test]
    public void SanitizeForLogging_LeavesNonPasswordArgumentsUnchanged()
    {
        var args = new[]
        {
            "--host",
            "localhost",
            "--dbname",
            "gis"
        };

        var sanitized = CommandLineArgumentSanitizer.SanitizeForLogging(args);

        Assert.That(sanitized, Is.EqualTo("--host localhost --dbname gis"));
    }
}
