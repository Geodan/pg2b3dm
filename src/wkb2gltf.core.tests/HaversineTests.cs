using NUnit.Framework;

namespace Wkb2Gltf.Tests;
public class HaversineTests
{
    [Test]
    public void FirstHaversineTest()
    {
        // arrange
        var p0 = (4.891738, 52.3734784 ); // dam paleis north
        var p1 = (4.893710516426987, 52.372842020278235); // dam monument

        // act
        var (dx, dy) = Haversine.GetDistances(p0.Item1, p0.Item2, p1.Item1, p1.Item2);

        // assert
        Assert.That(dx, Is.EqualTo(-133).Within(1));
        Assert.That(dy, Is.EqualTo(70).Within(1));
    }
}
