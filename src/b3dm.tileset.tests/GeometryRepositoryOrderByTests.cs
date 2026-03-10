using NUnit.Framework;

namespace B3dm.Tileset.Tests;

public class GeometryRepositoryOrderByTests
{
    [Test]
    public void GetOrderBy_Area_UsesEnvelopeArea()
    {
        var sql = GeometryRepository.GetOrderBy("geom", SortBy.AREA);
        Assert.That(sql, Does.Contain("ST_Area(ST_Envelope(geom))"));
        Assert.That(sql, Does.Contain("DESC"));
    }

    [Test]
    public void GetOrderBy_Volume_UsesZExtents()
    {
        var sql = GeometryRepository.GetOrderBy("geom", SortBy.VOLUME);
        Assert.That(sql, Does.Contain("ST_ZMax(geom)"));
        Assert.That(sql, Does.Contain("ST_ZMin(geom)"));
    }

    [Test]
    public void TilingSettings_DefaultSortBy_IsArea()
    {
        var settings = new TilingSettings();
        Assert.That(settings.SortBy, Is.EqualTo(SortBy.AREA));
    }
}
