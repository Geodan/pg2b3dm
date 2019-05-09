using System.Collections.Generic;
using NUnit.Framework;
using Wkb2Gltf;

namespace B3dm.Tileset.Tests
{
    public class TileCutterTests
    {
        [Test]
        public void TileCutterFirstTests()
        {
            // arrange
            var zUpBoxes = BBTestDataReader.GetTestData("testfixtures/zupboxes_actual.txt");
            var bboxes_grouped_expected = BBTestDataReader.GetTestData("testfixtures/bboxes_grouped_expected.txt");

            // act
            var tree = TileCutter.ConstructTree(zUpBoxes);
            var grouped_bb = GetBoundingBoxes(tree);

            // assert
            foreach (var bb in grouped_bb) {
                var found = FindInList(bboxes_grouped_expected, bb);
                Assert.IsTrue(found);
            }
            Assert.IsTrue(grouped_bb.Count == bboxes_grouped_expected.Count);

            Assert.IsTrue(zUpBoxes.Count == 1580);
            Assert.IsTrue(tree.Children[0].Features.Count == 20);
            Assert.IsTrue(tree.Children[0].Features[0].Id == 0);
            Assert.IsTrue(tree.Children[0].Children[0].Features[0].Id == 20);
        }

        private bool FindInList(List<BoundingBox3D> bbs, BoundingBox3D bb)
        {
            foreach(var b in bbs) {
                if (b.Equals(bb)) return true;
            }
            return false;
        }

        private List<BoundingBox3D> GetBoundingBoxes(Node node)
        {
            var res = new List<BoundingBox3D>();
            var bb = node.CalculateBoundingBox3D();
            res.Add(bb);

            foreach (var c in node.Children) {
                var newbb = GetBoundingBoxes(c);
                res.AddRange(newbb);
            }
            return res;
        }
    }
}
