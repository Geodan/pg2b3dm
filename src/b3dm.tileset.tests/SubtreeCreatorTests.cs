using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using subtree;

namespace B3dm.Tileset.Tests;

public class SubtreeCreatorTests
{
    [Test]
    public void CreateSubtreeTest()
    {
        var tile = new Tile(0, 0, 0);
        tile.Available = true;
        var subtreeFiles = SubtreeCreator.GenerateSubtreefiles(new List<Tile> { tile });
        Assert.IsNotNull(subtreeFiles);
        var stream = new MemoryStream(subtreeFiles.FirstOrDefault().Value);
        var subtree = SubtreeReader.ReadSubtree(stream);
        Assert.IsTrue(subtree.TileAvailability[0]);
        Assert.IsTrue(subtree.ContentAvailability[0]);
        Assert.IsTrue(subtree.ChildSubtreeAvailability==null);
    }

    [Test]
    public void CreateSubtreeRootTest()
    {
        var rootTile = new Tile(0, 0, 0);
        var tiles = new List<Tile> { rootTile };

        // level 1
        tiles.Add(new Tile(1, 0, 0));
        tiles.Add(new Tile(1, 0, 1));
        tiles.Add(new Tile(1, 1, 0));
        tiles.Add(new Tile(1, 1, 1));
        
        // level 2
        var l2 = new Tile(2, 0, 0);
        l2.Available = true;
        tiles.Add(l2);

        // level 3
        var l3 = new Tile(3, 0, 0);
        l3.Available = true;
        tiles.Add(l3);

        // asct
        var subtreeFiles = SubtreeCreator.GenerateSubtreefiles(tiles);

        // read root subtree file
        Assert.IsTrue(subtreeFiles.Count == 2);
        var stream = new MemoryStream(subtreeFiles.FirstOrDefault().Value);
        var subtree = SubtreeReader.ReadSubtree(stream);

        Assert.IsTrue(subtree.TileAvailability[0]);
        Assert.IsTrue(subtree.TileAvailability[1]);
        Assert.IsFalse(subtree.TileAvailability[2]);
        Assert.IsFalse(subtree.TileAvailability[3]);
        Assert.IsFalse(subtree.TileAvailability[4]);

        Assert.IsTrue(subtree.ContentAvailabiltyConstant==0);
        Assert.IsTrue(subtree.ChildSubtreeAvailability != null);
        Assert.IsTrue(subtree.ChildSubtreeAvailability[0]);
        Assert.IsFalse(subtree.ChildSubtreeAvailability[1]);
        Assert.IsFalse(subtree.ChildSubtreeAvailability[2]);
        Assert.IsFalse(subtree.ChildSubtreeAvailability[3]);

        // read child subtree file
        var streamChild = new MemoryStream(subtreeFiles.LastOrDefault().Value);
        var subtreeChild = SubtreeReader.ReadSubtree(streamChild);
        Assert.IsTrue(subtreeChild.ContentAvailability[0]);
        Assert.IsTrue(subtreeChild.TileAvailability[0]);
        Assert.IsTrue(subtreeChild.ContentAvailability[1]);
        Assert.IsTrue(subtreeChild.TileAvailability[1]);


    }

    [Test]
    public void GetSubtreeTilesTest()
    {
        var rootTile = new Tile(0, 0, 0);
        var tiles = new List<Tile> { rootTile };

        // level 1
        tiles.Add(new Tile(1, 0, 0));
        tiles.Add(new Tile(1, 0, 1));
        tiles.Add(new Tile(1, 1, 0));
        tiles.Add(new Tile(1, 1, 1));

        // level 2
        var l2 = new Tile(2, 0, 0);
        l2.Available = true;
        tiles.Add(l2);

        var subtreeTiles = SubtreeCreator.GetSubtreeTiles(tiles, new Tile(1,0,0));
        var subtreeTile = subtreeTiles.FirstOrDefault();
        Assert.IsTrue(subtreeTile.Z == 0 );
    }
}
