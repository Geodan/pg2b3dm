using System.Collections.Generic;
using NUnit.Framework;
using Wkb2Gltf.outlines;
using Wkx;

namespace Wkb2Gltf.Tests.outlines;
public class PartFinderTests
{
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
