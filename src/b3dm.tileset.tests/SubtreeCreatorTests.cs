using System.Collections.Generic;
using NUnit.Framework;

namespace B3dm.Tileset.Tests;

public class SubtreeCreatorTests
{
    [Test]
    public void CreateSubtreeTest()
    {
        var tile = new Tile(0, 0, 0);
        var subtreeFile = SubtreeCreator.GenerateSubtreefile(new List<Tile> { tile });
        Assert.IsNotNull(subtreeFile);
    }
}
