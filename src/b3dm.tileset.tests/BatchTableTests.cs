using Newtonsoft.Json;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class BatchTableTests
    {
        [Test]
        public void FirstBatchTableTest()
        {
            // arrange
            var batchTable = new BatchTable();
            var batchTableItem1 = new BatchTableItem();
            batchTableItem1.Name = "first item";
            batchTableItem1.Values = new string[3] { "0","1","2" };
            var batchTableItem2 = new BatchTableItem();
            batchTableItem2.Name = "second item";
            batchTableItem2.Values = new string[3] { "100", "101", "102" };
            batchTable.BatchTableItems.Add(batchTableItem1);
            batchTable.BatchTableItems.Add(batchTableItem2);

            // act
            var json = JsonConvert.SerializeObject(batchTable, new BatchTableJsonConverter(typeof(BatchTable)));

            // assert
            Assert.IsTrue(json == "{\"first item\":[\"0\",\"1\",\"2\"],\"second item\":[\"100\",\"101\",\"102\"]}");
        }
    }
}
