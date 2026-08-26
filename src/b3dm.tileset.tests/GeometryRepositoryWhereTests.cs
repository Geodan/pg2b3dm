using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests;

public class GeometryRepositoryWhereTests
{
    [Test]
    public void GetWhere_2D_UseSourceEpsgDirectly()
    {
        var from = new Point(100000.0, 400000.0);
        var to = new Point(200000.0, 500000.0);

        var sql = GeometryRepository.GetWhere("geom", from, to, "", 5698);

        Assert.That(sql, Does.Contain("5698"));
        Assert.That(sql, Does.Not.Contain("4326"));
        Assert.That(sql, Does.Not.Contain("ST_Transform"));
        Assert.That(sql, Does.Contain("ST_MakeEnvelope"));
    }

    [Test]
    public void GetWhere_2D_Epsg4326_UseSourceEpsgDirectly()
    {
        var from = new Point(-75.8, 38.4);
        var to = new Point(-75.0, 39.8);

        var sql = GeometryRepository.GetWhere("geom", from, to, "", 4326);

        Assert.That(sql, Does.Contain("4326"));
        Assert.That(sql, Does.Not.Contain("ST_Transform"));
        Assert.That(sql, Does.Contain("ST_MakeEnvelope"));
    }

    [Test]
    public void GetWhere_3D_UseSourceEpsgDirectly()
    {
        var from = new Point(100000.0, 400000.0, 200.0);
        var to = new Point(200000.0, 500000.0, 300.0);

        var sql = GeometryRepository.GetWhere("geom", from, to, "", 5698);

        Assert.That(sql, Does.Contain("5698"));
        Assert.That(sql, Does.Not.Contain("4979"));
        Assert.That(sql, Does.Not.Contain("4326"));
        Assert.That(sql, Does.Not.Contain("ST_Transform"));
        Assert.That(sql, Does.Contain("ST_3DMakeBox"));
    }

    [Test]
    public void GetWhere_3D_ContainsCentroidAndSrid()
    {
        var from = new Point(100000.0, 400000.0, 200.0);
        var to = new Point(200000.0, 500000.0, 300.0);

        var sql = GeometryRepository.GetWhere("geom", from, to, "", 5698);

        Assert.That(sql, Does.Contain("st_setsrid"));
        Assert.That(sql, Does.Contain("ST_3DIntersects"));
    }
}
