using NUnit.Framework;
using subtree;

namespace B3dm.Tileset.Tests;
public class TileTests
{
    [Test]
    public void GetParentTest0()
    {
        var from = new Tile(6, 3, 18);
        var to = new Tile(9, 20, 110);
        var rel = SubtreeCreator.GetRelativeTile(from, to);
        Assert.IsTrue(rel.Z == 3 && rel.X == 2 && rel.Y == 2);
    }

    [Test]
    public void GetParentTest()
    {
        var from = new Tile(1, 1, 1);
        var to = new Tile(3, 7, 7);

        var rel = SubtreeCreator.GetRelativeTile(from, to);
        Assert.IsTrue(rel.Z == 2 && rel.X == 3 && rel.Y == 3);
    }

    [Test]
    public void GetRelativeTileTest2()
    {
        var from = new Tile(1, 0, 0);
        var to = new Tile(2, 0, 1);
        var rel = SubtreeCreator.GetRelativeTile(from, to);

        Assert.IsTrue(rel.Z == 1 && rel.X == 0 && rel.Y == 1);
    }

    [Test]
    public void GetRelativeTest3()
    {
        var from = new Tile(1, 0, 1);
        var to = new Tile(2, 0, 2);
        var rel = SubtreeCreator.GetRelativeTile(from, to);
        Assert.IsTrue(rel.Z == 1 && rel.X == 0 && rel.Y == 0);
    }


    [Test]
    public void GetParentTest4()
    {
        var from = new Tile(1, 0, 1);
        var to = new Tile(2, 0, 3);
        var rel = SubtreeCreator.GetRelativeTile(from, to);
        Assert.IsTrue(rel.Z == 1 && rel.X == 0 && rel.Y == 1);
    }
}
