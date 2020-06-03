using System.IO;
using NUnit.Framework;
using Wkx;

namespace Wkb2Gltf.Tests
{
    public class GeometryRecordTests
    {
        [Test]
        public void GeometryRecordFirstTest()
        {
            // arrange
            var geometryRecord = new GeometryRecord(0);
            var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var polyhedralsurface = ((PolyhedralSurface)g);

            geometryRecord.Geometry = polyhedralsurface;

            // act
            var triangles = geometryRecord.GetTriangles();

            // assert
            Assert.IsTrue(g != null);

            // there are 262 geometries... 
            Assert.IsTrue(polyhedralsurface.Geometries.Count == 262);
            // there are 51 degenerated triangles in this geometry...
            Assert.IsTrue(triangles.Count == polyhedralsurface.Geometries.Count - 51);
        }
    }
}
