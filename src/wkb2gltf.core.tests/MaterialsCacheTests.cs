using NUnit.Framework;

namespace Wkb2Gltf.Tests
{
    public class MaterialsTests
    {
        [Test]
        public void FirstTest()
        {
            var materialCache = new MaterialsCache();
            var builder = materialCache.GetMaterialBuilderByColor("#d117b8");
            Assert.IsTrue(builder != null);
        }
    }
}
