using System.Collections.Generic;
using NUnit.Framework;
using Wkb2Gltf;

namespace B3dm.Tileset.Tests
{
    public class NodeTests
    {
        private Node GetSimpleNode(BoundingBox3D bb)
        {
            var n = new Node();
            var f1 = new Feature() { BoundingBox3D = bb };
            n.Features = new List<Feature>() { f1 };
            return n;
        }
        [Test]
        public void TestBoundingBox() {
            // arrange
            var bb_expected = new BoundingBox3D(0, 0, 0, 15, 15, 15);
            var n = GetSimpleNode(new BoundingBox3D(0, 0, 0, 10, 10, 10));
            n.Children = new List<Node>();
            var n1 = GetSimpleNode(new BoundingBox3D(5, 5, 5, 15, 15, 15));
            n.Children.Add(n1); 

            // act
            var bbox = n.CalculateBoundingBox3D();

            // assert
            Assert.IsTrue(bbox.Equals(bb_expected));
        }
    }
}
