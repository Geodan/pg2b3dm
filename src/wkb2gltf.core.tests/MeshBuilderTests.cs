using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Schema2;
using SharpGLTF.Materials;
using Wkb2Gltf.Extensions;
using System.IO;
using SharpGLTF.Memory;

namespace Wkb2Gltf.Tests
{
    public class MeshBuilderTests
    {
        [Test]
        public void CreateMeshWithCustomVertexAttribute()
        {
            var dmat = MaterialBuilder.CreateDefault();
            var mesh = new MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>();
            var prim = mesh.UsePrimitive(dmat);

            var triangle =
            (
            new Vector3(4373.192624189425f, 5522.678275192156f, -359.8238015332605f),
            new Vector3(4370.978060142137f, 5522.723320999183f, -359.89184701762827f),
            new Vector3(4364.615741107147f, 5511.510615546256f, -359.08922455413233f)
            );

            prim.AddTriangleWithBatchId(triangle, Vector3.One, 101);

            var model = ModelRoot.CreateModel();
            var dstMesh = model.CreateMesh(mesh);

            model.UseScene("Default").CreateNode().WithMesh(model.LogicalMeshes[0]);

            var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "building_sharp1.glb");
            model.SaveGLB(fileName);

            var batchId = dstMesh.Primitives[0].GetVertexAccessor(VertexWithBatchId.CUSTOMATTRIBUTENAME).AsScalarArray();

            // CollectionAssert.AreEqual(new float[] { 101, 101, 101 }, batchId);
            //CollectionAssert.AreEqual(new ScalarArray[] { 101, 101f, 101 }, batchId);
            Assert.IsTrue(batchId[0] == (float)101);
        }
    }
}
