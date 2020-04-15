using Newtonsoft.Json;
using NUnit.Framework;

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
    }
}
