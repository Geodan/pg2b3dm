using B3dm.Tileset.Extensions;
using NUnit.Framework;
using subtree;

namespace B3dm.Tileset.Tests;
public class TileTests
{
    // [Test]
    public void GetParentTest()
    {
        var t = new Tile(3, 7, 7);
        var res = t.GetParent(2);
        Assert.IsTrue(res.Z == 1 && res.X == 1 && res.Y == 1);
    }

    [Test]
    public void GetParentTest2()
    {
        var t = new Tile(2, 0, 1);
        var res = t.GetParent(1);
        Assert.IsTrue(res.Z == 1 && res.X == 0 && res.Y == 1);
    }

    [Test]
    public void GetParentTest3()
    {
        var t = new Tile(2, 0, 2);
        var res = t.GetParent(1);
        Assert.IsTrue(res.Z == 1 && res.X == 0 && res.Y == 0);
    }

    [Test]
    public void GetParentTest4()
    {
        var t = new Tile(2, 0, 1);
        var res = t.GetParent(1);
        Assert.IsTrue(res.Z == 1 && res.X == 0 && res.Y == 1);
    }

    [Test]
    public void GetParentTest5()
    {
        var t = new Tile(2, 0, 3);
        var res = t.GetParent(1);
        Assert.IsTrue(res.Z == 1 && res.X == 0 && res.Y == 1);
    }


}
