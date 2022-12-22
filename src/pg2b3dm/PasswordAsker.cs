using System;

namespace pg2b3dm;

public static class PasswordAsker
{
    public static string GetPassword()
    {
        var result = String.Empty;
        ConsoleKeyInfo keyInfo;

        do {
            keyInfo = Console.ReadKey(true);
            // Skip if Backspace or Enter is Pressed
            if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter) {
                result += keyInfo.KeyChar;
            }
            else {
                if (keyInfo.Key == ConsoleKey.Backspace && result.Length > 0) {
                    // Remove last charcter if Backspace is Pressed
                    result = result.Substring(0, (result.Length - 1));
                }
            }
        }
        // Stops Getting Password Once Enter is Pressed
        while (keyInfo.Key != ConsoleKey.Enter);
        return result;
    }
}
