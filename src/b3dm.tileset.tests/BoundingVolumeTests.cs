﻿using Newtonsoft.Json;
using NUnit.Framework;
using subtree;

namespace B3dm.Tileset.Tests;

public class BoundingVolumeTests
{
    [Test]
    public void SerializeToJSONTest()
    {
        // arrange
        var bv = new Boundingvolume();
        bv.region = new double[] {
 -1.31972,
    0.68236,
    -1.31659,
    0.68439,
    -0.0,
    76.58};

        // act
        var output = JsonConvert.SerializeObject(bv);

        var back = JsonConvert.DeserializeObject<Boundingvolume>(output);

        // assert
        Assert.That(output != null, Is.True);
        Assert.That(back.region[0] == -1.31972, Is.True);
    }
}
