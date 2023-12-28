using System.Numerics;
using NUnit.Framework;

namespace Wkb2Gltf.Tests;

public class NormalTests
{
    [Test]
    public void CalculateNormal()
    {
        var p0 = new Wkx.Point(-4809.9107400178909, 1112.3783205118962, -692.219980129972);
        var p1 = new Wkx.Point(-4822.8970195085276, 1111.9907626458444, -688.55403241701413);
        var p2 = new Wkx.Point(-4821.631989160087, 1106.6135776401497, -684.35551724396635);

        var t = new Triangle(p0,p1,p2, 0);

        var expected = new Vector3(0.193098128f, 0.6316621f, 0.750810444f);
        var normal = t.GetNormal();
        Assert.That(normal.Equals(expected), Is.True);
    }
}
