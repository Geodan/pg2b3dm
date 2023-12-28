using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using pg2b3dm;
using Wkx;

namespace Wkb2Gltf.Tests;

public class B3dmWriterTests
{
    [Test]
    public void FirstB3dmWriterTest()
    {
        // arrange
        var geometryRecord = new GeometryRecord(0);
        var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
        var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
        var polyhedralsurface = ((PolyhedralSurface)g);
        geometryRecord.Geometry = polyhedralsurface;
        var attributes = new Dictionary<string, object>();
        attributes.Add("id", "testbuilding");
        geometryRecord.Attributes = attributes;

        // act
        var b3dmBytes = TileWriter.ToTile(new List<GeometryRecord> { geometryRecord }, addOutlines: true);

        // assert
        Assert.That(b3dmBytes != null, Is.True);
    }
}
