using System.Drawing;
using NUnit.Framework;

namespace Wkb2Gltf.Tests
{
    public class ColorTests
    {
        [Test]
        public void HexColorToRgbTest()
        {
            var hexcolor = "#ff5555";
            var color = ColorTranslator.FromHtml(hexcolor);
            Assert.IsTrue(color.R == 255 && color.G == 85 && color.B == 85);
        }
    }
}
