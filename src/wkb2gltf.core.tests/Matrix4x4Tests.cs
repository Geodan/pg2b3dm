using NUnit.Framework;
using System.Numerics;

namespace Wkb2Gltf.Tests
{
    public class Matrix4x4Tests
    {
        [Test]
        public void MatrixFlattenTests()
        {
            var m = new Matrix4x4(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15, 16);

            var flatten = m.Flatten();
            Assert.IsTrue(flatten[0] == 1);
            Assert.IsTrue(flatten[1] == 5);
        }
    }
}
