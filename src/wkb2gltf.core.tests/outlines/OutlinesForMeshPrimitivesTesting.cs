using NUnit.Framework;
using SharpGLTF.Schema2;
using Wkb2Gltf.extensions;

namespace Wkb2Gltf.Tests.outlines;
public  class OutlinesForMeshPrimitivesTesting
{
    [Test]
    public void TestGlbWithMeshPrimitives()
    {
        var model = ModelRoot.Load(@"testfixtures/two_triangles.glb");

        var primitives = model.LogicalMeshes[0].Primitives;
        Assert.That(primitives.Count == 2, Is.True);

       // model.LogicalMeshes[0].Primitives[0].AddOutlines();
    }
}
