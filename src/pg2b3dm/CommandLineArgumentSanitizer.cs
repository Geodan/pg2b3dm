using System.Linq;
using System.Text.RegularExpressions;

namespace pg2b3dm;

public static partial class CommandLineArgumentSanitizer
{
    public static string SanitizeForLogging(string[] args)
        => string.Join(" ", args.Select(Redact));

    private static string Redact(string arg)
        => PasswordRegex().Replace(arg, "$1***");

    [GeneratedRegex(@"(\b(?:password|pwd)\s*=\s*)[^;]*", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordRegex();
}
