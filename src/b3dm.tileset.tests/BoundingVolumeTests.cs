using Newtonsoft.Json;
using NUnit.Framework;
using Wkx;

namespace B3dm.Tileset.Tests
{
    public class BoundingVolumeTests
    {
        [Test]
        public void SerializeToJSONTest()
        {
            // arrange
            var bv = new Boundingvolume();
            bv.box = new double[] {
                9.3132257461547852E-10,15.165075950091705,-4.6566128730773926E-10,
                1130.6094067522015,0.0,0.0,
                0.0,2168.6548668255564,0.0,
                0.0,0.0,1272.8861378305592};

            // act
            var output = JsonConvert.SerializeObject(bv);

            var back = JsonConvert.DeserializeObject<Boundingvolume>(output);

            // assert
            Assert.IsTrue(output != null);
            Assert.IsTrue(back.box[0] == 0);
        }


        [Test]
        public void TestCalculateVolumetric()
        {
            // tile 1 boundingbox
            var actualbbTile1 = new BoundingBox(-8414816.8743550442, 4744484.5449978458, - 8412816.8743550442, 4746484.5449978458);
            var translation = new double[] { -8406745.007853176, 4744614.257728589, 38.29 };

            // intermediate result 1 feature (20240):
            var boundingBox3DFeature1 = new BoundingBox3D(-8413063.545175588, 4746417.570850967, 0, -8413036.978556471, 4746442.517736644, 11.06);

            var actualFeature1BoundingVolume = new BoundingBox3D(-6318.537322411314, -38.289999999999885, -1828.2600080547854, -6291.970703294501, -27.229999999999883, -1803.3131223786622);
            var expectedBoundingVolumeTile1 = new double[] { -6303.03, 985.801, -29.87, 223.048, 0, 0, 0, 886.691, 0, 0, 0, 8.42 };
        }
    }
}
