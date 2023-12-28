using System.Collections.Generic;
using System.IO;
using B3dmCore;
using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests;

public class B3dmCreatorTests
{
    [Test]
    public void CreateB3dmTest()
    {
        // arrange
        var p0 = new Point(0, 0, 0);
        var p1 = new Point(1, 1, 0);
        var p2 = new Point(1, 0, 0);
        
        var triangle1 = new Triangle(p0, p1, p2,0);
        var triangles = new List<Triangle>() { triangle1 };

        var attributes = new Dictionary<string, List<object>>();
        attributes.Add("id", new List<object>() { "1" });
        // act
        var bytes = TileCreator.GetTile(attributes, new List<List<Triangle>>() { triangles });

        // assert
        var stream = new MemoryStream(bytes);
        var b3dm = B3dmReader.ReadB3dm(stream);
        Assert.That(b3dm.B3dmHeader.Version == 1, Is.True);
        Assert.That(b3dm.BatchTableJson == "{\"id\":[\"1\"]}    ", Is.True);
    }
}
