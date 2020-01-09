using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class SqlBuilderTests
    {
        [Test]
        public void TestOptionalSqlFields()
        {
            Assert.IsTrue (SqlBuilder.GetOptionalColumnsSql("haha", "hoho")== ", haha, hoho");
            Assert.IsTrue(SqlBuilder.GetOptionalColumnsSql("", "hoho") == ", hoho");
            Assert.IsTrue(SqlBuilder.GetOptionalColumnsSql("", "") == "");
            Assert.IsTrue(SqlBuilder.GetOptionalColumnsSql("haha", "") == ", haha");
        }
    }
}
