using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests;
public class BBox3DTests
{
    [Test]
    public void TestGetBBox3DWithPolygonZ()
    {
        var polygonWkt = "POLYGON Z ((4.674683 52.254709 60,4.674683 52.448477 60,5.07843 52.448477 60,5.07843 52.254709 60,4.674683 52.254709 60))";
        var geometry = Geometry.Deserialize<WktSerializer>(polygonWkt);
        var result = BBox3D.GetBoundingBoxPoints(geometry);
        Assert.That(result.Length == 6, Is.True);
        Assert.That(result[4] == 60);
        Assert.That(result[5] == 61);
    }

    [Test]
    public void TestGetBBox3DWithPolyhedralSurface()
    {
        var polygonWkt = "POLYHEDRALSURFACE Z (((4.674683 52.254709 60,4.674683 52.448477 60,5.07843 52.448477 60,5.07843 52.254709 60,4.674683 52.254709 60)),((4.674683 52.254709 70,5.07843 52.254709 70,5.07843 52.448477 70,4.674683 52.448477 70,4.674683 52.254709 70)),((4.674683 52.254709 60,4.674683 52.254709 70,4.674683 52.448477 70,4.674683 52.448477 60,4.674683 52.254709 60)),((5.07843 52.254709 60,5.07843 52.448477 60,5.07843 52.448477 70,5.07843 52.254709 70,5.07843 52.254709 60)),((4.674683 52.254709 60,5.07843 52.254709 60,5.07843 52.254709 70,4.674683 52.254709 70,4.674683 52.254709 60)),((4.674683 52.448477 60,4.674683 52.448477 70,5.07843 52.448477 70,5.07843 52.448477 60,4.674683 52.448477 60)))";
        var geometry = Geometry.Deserialize<WktSerializer>(polygonWkt);
        var result = BBox3D.GetBoundingBoxPoints(geometry);
        Assert.That(result.Length == 6, Is.True);
        Assert.That(result[4] == 60);
        Assert.That(result[5] == 70);
    }

}
