using System;
using B3dm.Tileset.extensions;
using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests
{
    public class BoundingBoxTests
    {
        [Test]
        public void FirstTEst()
        {
            var bbLibertyIsland = new BoundingBox(-74.049949, 40.687549, -74.039741, 40.691747);
            // var bb = new BoundingBox(4.9777, 52.1021, 5.1859, 52.0682);
            //var bb_dela = new BoundingBox(-75.6145, 39.0964, -75.4353, 39.2124);
            var radians = bbLibertyIsland.ToRadians();

            var s= $"{radians.XMin}, {radians.YMin}, {radians.XMax}, {radians.YMax}";
        }
    }
}
