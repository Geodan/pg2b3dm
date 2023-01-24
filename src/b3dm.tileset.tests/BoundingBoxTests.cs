using B3dm.Tileset.Extensions;
using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests;

public class BoundingBoxTests
{
    [Test]
    public void ToRadiansTest()
    {
        var bbox = new BoundingBox(5, 51, 6, 52);
        var radians = bbox.ToRadians();
        Assert.IsTrue(radians.XMin == 0.087266462599716474);
    }
}
