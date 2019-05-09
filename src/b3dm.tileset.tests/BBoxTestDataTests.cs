using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class TestDataTests
    {
        [Test]
        public void BoxesTestDataTest()
        {
            var boxes_actual = BBTestDataReader.GetTestData("testfixtures/bboxes_actual.txt");
            var boxes_expected = BBTestDataReader.GetTestData("testfixtures/bboxes_expected.txt");
            for (var i = 0; i < boxes_actual.Count; i++) {
                var box_act = boxes_actual[i];
                var box_expect = boxes_expected[i];
                Assert.IsTrue(box_act.Equals(box_expect));
            }
        }

        [Test]
        public void ZUpBoxesTestDataTest()
        {
            var zUpBoxes_actual = BBTestDataReader.GetTestData("testfixtures/zupboxes_actual.txt");
            var zUpBoxes_expected = BBTestDataReader.GetTestData("testfixtures/zupboxes_expected.txt");
            for (var i = 0; i < zUpBoxes_actual.Count; i++) {
                var box_act = zUpBoxes_actual[i];
                var box_expect = zUpBoxes_expected[i];
                Assert.IsTrue(box_act.Equals(box_expect));
            }
        }
    }
}
