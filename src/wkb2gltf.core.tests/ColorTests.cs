using System.Drawing;
using NUnit.Framework;

namespace Wkb2Gltf.Tests;

public class ColorTests
{
    [Test]
    public void HexColorToRgbTest()
    {
        var hexcolor = "#ff5555";
        var color = ColorTranslator.FromHtml(hexcolor);
        Assert.That(color.R == 255 && color.G == 85 && color.B == 85, Is.True);
    }
    
    [Test]
    public void HexColorWithAlphaToRgbaTest()
    {
        var hexcolor = "#55ff5657";
        var color = ColorTranslator.FromHtml(hexcolor);
        Assert.That(color.A == 85 && color.R == 255 && color.G == 86 && color.B == 87, Is.True);
    }
}
