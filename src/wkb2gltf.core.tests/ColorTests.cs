using System.Drawing;
using NUnit.Framework;

namespace Wkb2Gltf.Tests;

public class ColorTests
{

    [Test]
    public void HexColorToRgbTest()
    {
        var hexcolor = "#ff5555";
        var color = RgbaColor.FromHex(hexcolor);
        Assert.That(color.R == 255 && color.G == 85 && color.B == 85);
    }
    
    [Test]
    public void HexColorWithAlphaToRgbaTest()
    {
        var hexcolor = "#FF000055";
        var color = RgbaColor.FromHex(hexcolor);
        Assert.That(color.A == 85 && color.R == 255 && color.G == 0 && color.B == 0);
    }
}
