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

            var bbox = new BoundingBox(5, 51, 6, 52);
            var bbdenhelder = new BoundingBox(4.709058, 52.945690, 4.790726, 52.973842);
            // var bb = new BoundingBox(4.9777, 52.1021, 5.1859, 52.0682);
            //var bb_dela = new BoundingBox(-75.6145, 39.0964, -75.4353, 39.2124);
            var radians = bbox.ToRadians();

            var s= $"{radians.XMin}, {radians.YMin}, {radians.XMax}, {radians.YMax}";
        }
    }
}
