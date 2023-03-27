using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using Wkb2Gltf.Extensions;
using Wkb2Gltf.extensions;
using System.Collections.Generic;
using Wkx;
using SharpGLTF.Scenes;

namespace Wkb2Gltf.Tests.outlines;
public class MeshPrimitiveOutlineTests
{
    [Test]
    public void CreatGltfWithOutlines()
    {
        // arrange
        var dmat = MaterialBuilder.CreateDefault();
        var mesh = new MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>();
        var prim = mesh.UsePrimitive(dmat);

        var p0 = new Point(0, 0, 0);
        var p1 = new Point(1, 0, 0);
        var p2 = new Point(0, 1, 0);
        var t = (p0.ToVector(), p1.ToVector(), p2.ToVector());

        prim.AddTriangleWithBatchId(t, Vector3.One, 101);

        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);
        var model = scene.ToGltf2();


        // act
        model.LogicalMeshes[0].Primitives[0].AddOutlines();
    }
}
