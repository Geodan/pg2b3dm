using System.Collections.Generic;
using NUnit.Framework;
using Wkb2Gltf.outlines;
using Wkx;

namespace Wkb2Gltf.Tests.outlines;
public class PartFinderTests
{
    [Test]
    public void PartFinderTest2()
    {
        var t0 = new Triangle(new Point(223785.734375, -389497.78125, -221034.46875), new Point(223779.40625, -389497.75, -221027.78125), new Point(223786.34375, -389490.84375, -221034.890625), 0);
        var t1 = new Triangle(new Point(223785.515625, -389490.8125, -221034.015625), new Point(223786.34375, -389490.84375, -221034.890625), new Point(223779.46875, -389490.65625, -221027.609375), 1);

        var triangles = new List<Triangle>() { t0, t1};
        var parts = PartFinder.GetParts(triangles, 0.1);
        Assert.That(parts.Count, Is.EqualTo(1));
    }

    [Test]
    public void PartFinderFirstTest()
    {
        // arrange
        var t0 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(1, 0, 0), 0);
        var t1 = new Triangle(new Point(1, 0, 0), new Point(0, 1, 0), new Point(1, 1, 0), 1);
        var t2 = new Triangle(new Point(5, 0, 0), new Point(5, 1, 0), new Point(5, 5, 0), 2);

        var triangles = new List<Triangle>() { t0, t1, t2 };

        // In this case wel'll only find one part because only the normal is checked
        var parts = PartFinder.GetParts(triangles);

        Assert.That(parts.Count, Is.EqualTo(1));
        Assert.That(parts[0].Count, Is.EqualTo(3));
        Assert.That(parts[0][0], Is.EqualTo(0));
    }
}
