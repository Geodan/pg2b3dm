using System.Numerics;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class EcefConverterTest
    {
        [Test]
        public void Ecef2LlaTest()
        {
            var ecef = new Vector3(1231256.4091099831f, -4800453.896456448f, 4000024.663498499f);
            var wgs4 = EcefConverter.Ecef2lla(ecef);
            Assert.IsTrue(wgs4 != null);
            Assert.IsTrue(wgs4.X == -75.614454164675919);
            Assert.IsTrue(wgs4.Y == 39.096412830732426);
            Assert.IsTrue(wgs4.Z == 36.132278126178093);
        }
    }
}
