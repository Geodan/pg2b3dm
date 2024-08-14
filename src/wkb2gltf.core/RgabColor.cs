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
        if (hex.StartsWith("#")) {
            hex = hex.Substring(1);
        }

        if (hex.Length == 8) {
            var red = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var green = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var blue = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            var alpha = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);

            return Color.FromArgb(alpha, red, green, blue);
        }
        else if (hex.Length == 6) {
            var red = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var green = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var blue = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

            return Color.FromArgb(255, red, green, blue); 
        }
        else {
            throw new ArgumentException("Hex-color code must be 6 of 8 characters.");
        }
    }
}