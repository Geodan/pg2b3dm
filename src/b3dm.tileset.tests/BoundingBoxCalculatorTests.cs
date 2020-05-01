using System;
using NUnit.Framework;

namespace B3dm.Tileset.Tests
{
    public class BoundingBoxCalculatorTests
    {
        [Test]
        public static void CalculateBoundingVolume()
        {
            // values for table delaware_buildings_4978
            var translation = new double[] { 1238070.0029833354, -4795867.9075041208, 4006102.3617460253};
            var bbox3d = new BoundingBox3D(1231256.4091099831, -4800453.896456448, 4000024.663498499, 1244883.5968566877, -4791281.918551793, 4012180.059993551);
            var boundingboxAllFeatures = BoundingBoxCalculator.RotateTranslateTransform(bbox3d, translation, Math.PI / 2);
            var box = boundingboxAllFeatures.GetBox();
            var expectedBox = new double[] { 0.0, 0.0, 0.0, 6813.593873352278, 0.0, 0.0, 0.0, 4585.98895, 0.0, 0.0, 0.0, 6077.69825 };
            for(var i = 0; i < box.Length; i++) {
                Assert.IsTrue(box[i].Equals(expectedBox[i]));
            }


        }

        [Test]
        public void CalculcateBoundingBoxAllFeaturesFromBoundingVolumeBoxAndTransform()
        {
            var bbox3d = new BoundingBox3D(1231256.4091099831, -4800453.896456448, 4000024.663498499, 1244883.5968566877, -4791281.918551793, 4012180.059993551);
            var center = bbox3d.GetCenter();
            var expectedVolumeBox = new double[] { (double)center.X, (double)center.Y, (double)center.Z, (bbox3d.ExtentX() / 2), 0, 0, 0, bbox3d.ExtentY() / 2, 0, 0, 0, bbox3d.ExtentZ() / 2 };

            var actualTransform = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1238070.0029833354, -4795867.9075041208, 4006102.3617460253, 1 };
            var actualTilesetBox = new double[] { 0.0, 0.0, 0.0, 6813.594, 0.0, 0.0, 0.0, 4585.989, 0.0, 0.0, 0.0, 6077.698 };

            var actualVolumeBox = GetVolumeBox(actualTilesetBox, actualTransform);
            Assert.IsTrue(actualVolumeBox.Length == expectedVolumeBox.Length);
            for (var i = 0; i < actualVolumeBox.Length; i++) {
                Assert.IsTrue(actualVolumeBox[i].Equals(expectedVolumeBox[i]));
            }

        }

        private double[] GetVolumeBox(double[] actualTilesetBox, double[] actualTransform)
        {
            var bb = new BoundingBox3D(actualTilesetBox);
            var transformback = bb.TransformZToY();
            return new double[]{ 1,2,3};
        }
    }
}
