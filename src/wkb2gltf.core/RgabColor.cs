using System;
using System.Drawing;
using System.Globalization;

namespace Wkb2Gltf;

public static class RgbaColor
{
    public static string ToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
    }
    public static Color FromHex(string hex)
    {
        var hexSpan = hex.AsSpan();

        if (hexSpan.StartsWith("#")) {
            hexSpan = hexSpan.Slice(1);
        }

        var rgb = GetRgb(hexSpan);
        
        if (hexSpan.Length == 8) {
            var alpha = byte.Parse(hexSpan.Slice(6, 2), NumberStyles.HexNumber);
            return Color.FromArgb(alpha, rgb.red, rgb.green, rgb.blue);
        }
        else if (hexSpan.Length == 6) {
            return Color.FromArgb(255, rgb.red, rgb.green, rgb.blue);
        }
        else {
            throw new ArgumentException("Hex-color code must be 6 or 8 characters.");
        }
    }

    private static (int red, int green, int blue) GetRgb(ReadOnlySpan<char> hexSpan)
    {
        var red = byte.Parse(hexSpan.Slice(0, 2), NumberStyles.HexNumber);
        var green = byte.Parse(hexSpan.Slice(2, 2), NumberStyles.HexNumber);
        var blue = byte.Parse(hexSpan.Slice(4, 2), NumberStyles.HexNumber);
        return (red, green, blue);
    }
}