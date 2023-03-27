using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpGLTF.Geometry;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using Wkb2Gltf.extensions;
using Wkb2Gltf.outlines;
using Wkx;

namespace Wkb2Gltf.Tests.outlines;
public  class OutlinesForMeshPrimitivesTesting
{
    [Test]
    public void TestGlbWithMeshPrimitives()
    {
        var model = ModelRoot.Load(@"testfixtures/two_triangles.glb");

        var primitives = model.LogicalMeshes[0].Primitives;
        Assert.IsTrue(primitives.Count == 2);

        model.LogicalMeshes[0].Primitives[0].AddOutlines();
    }
}
