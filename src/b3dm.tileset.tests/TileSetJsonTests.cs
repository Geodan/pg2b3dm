using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    class TileSetJsonTests
    {
        [Test]
        public void FirstTileSetJsonTests()
        {
            // arrange
            var zUpBoxes = BBTestDataReader.GetTestData("testfixtures/zupboxes_actual.txt");
            var tree = TileCutter.ConstructTree(zUpBoxes);
            var translation = new double[] { 141584.2745, 471164.637, 15.81555842685751 };
            var s = File.ReadAllText(@"./testfixtures/tileset_json_expected.json");
            var tileset_json_expected = JsonConvert.DeserializeObject<TileSet>(s);

            // act
            var tileset_json_actual = TreeSerializer.ToTileset(tree,translation);
            var actual_json = JsonConvert.SerializeObject(tileset_json_actual, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText("d:/aaa/sample_tileset_actual.json", actual_json);

            // assert
            Assert.IsTrue(tileset_json_actual.asset.version=="1.0");
            Assert.IsTrue(tileset_json_actual.geometricError==500);

            var all_children_actual = GetAllChildren(tileset_json_actual.root);
            var all_children_expected = GetAllChildren(tileset_json_expected.root);
            var res = AreSimilar(all_children_actual, all_children_expected);

            var sim = IsSimilar(tileset_json_expected.root, tileset_json_actual.root);
            Assert.IsTrue(sim);
            Assert.IsTrue(tileset_json_actual.root.refine == "add"); // 500
            Assert.IsTrue(tileset_json_actual.root.geometricError == 500); // 500
            Assert.IsTrue(tileset_json_actual.root.transform.Length == 16); // 500
            Assert.IsTrue(tileset_json_actual.root.boundingVolume.box.Length == 12);
            Assert.IsTrue(tileset_json_actual.root.children.Count == 1);
        }

        public bool AreSimilar(List<Child> l1, List<Child> l2)
        {
            for(var i = 1; i < l1.Count; i++) {
                var ch1 = l1[i];
                var ch2 = l2[i];
                var sim = IsSimilar(ch1, ch2);
                if (!sim) return false;
            }
            return true;
        }

        public bool IsSimilar(Child ch1,Child ch2)
        {
            var ge = ch1.geometricError == ch2.geometricError;
            var refine = ch1.refine == ch2.refine;
            var ch_count = ch1.children.Count == ch2.children.Count;
            var bb_length = ch1.boundingVolume.box.Length == ch2.boundingVolume.box.Length;
            return (ge && refine && ch_count && bb_length);
        }

        private List<Child> GetAllChildren(Child child)
        {
            var res = new List<Child>();
            res.Add(child);

            foreach (var c in child.children) {
                var newbb = GetAllChildren(c);
                res.AddRange(newbb);
            }
            return res;
        }
    }
}
