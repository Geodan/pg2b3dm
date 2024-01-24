using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Schema2.Tiles3D;
using Wkb2Gltf.extensions;
using Wkb2Gltf.Extensions;

namespace Wkb2Gltf;

public static class GlbCreator
{
    public static byte[] GetGlb(List<List<Triangle>> triangles, string copyright = "", bool addOutlines = false, string defaultColor = "#FFFFFF", string defaultMetallicRoughness = "#008000", bool defaultDoubleSided = true, Dictionary<string, List<object>> attributes = null, bool createGltf = false, bool doubleSided = false)
    {
        var materialCache = new MaterialsCache();
        var shader = new Shader();
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = defaultColor, MetallicRoughness = defaultMetallicRoughness };
        var defaultMaterial = MaterialCreator.CreateMaterial(shader, defaultDoubleSided);

        var meshBatchId = new MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>("mesh");
        var meshFeatureIds = new MeshBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty>("mesh");

        foreach (var tri in triangles) {
            foreach (var triangle in tri) {
                MaterialBuilder material;

                if (triangle.Shader != null) {
                    material = materialCache.GetMaterialBuilderByShader(triangle.Shader, doubleSided);
                }
                else {
                    material = defaultMaterial;
                }

                if (createGltf) {
                    DrawTriangleWithFeatureId(triangle, material, meshFeatureIds);
                }
                else {
                    DrawTriangleWithBatchId(triangle, material, meshBatchId);
                }
            }
        }
        var scene = new SceneBuilder();
        if (createGltf) {
            scene.AddRigidMesh(meshFeatureIds, Matrix4x4.Identity);
        }
        else {
            scene.AddRigidMesh(meshBatchId, Matrix4x4.Identity);
        }
        var model = scene.ToGltf2();
        model.Asset.Copyright = copyright;
        model.Asset.Generator = $"pg2b3dm {Assembly.GetEntryAssembly().GetName().Version}";

        var localTransform = new Matrix4x4(
1, 0, 0, 0,
0, 0, -1, 0,
0, 1, 0, 0,
0, 0, 0, 1);
        model.LogicalNodes.First().LocalTransform = new SharpGLTF.Transforms.AffineTransform(localTransform);

        if (addOutlines) {
            foreach (var primitive in model.LogicalMeshes[0].Primitives) {
                primitive.AddOutlines();
            }
        }

        if (createGltf && attributes != null && attributes.Count > 0) {

            var rootMetadata = model.UseStructuralMetadata();
            var schema = rootMetadata.UseEmbeddedSchema("schema");
            var schemaClass = schema.UseClassMetadata("propertyTable");
            var propertyTable = schemaClass.AddPropertyTable(attributes.First().Value.Count);

            foreach (var primitive in model.LogicalMeshes[0].Primitives) {
                var featureIdAttribute = new FeatureIDBuilder(attributes.First().Value.Count, 0, propertyTable);
                primitive.AddMeshFeatureIds(featureIdAttribute);
            }

            foreach (var attribute in attributes) {
                var type = attribute.Value.FirstOrDefault().GetType();
                var objects = attribute.Value;
                var property = schemaClass.UseProperty(attribute.Key);

                // sbyte not available in postgres
                if (type == typeof(bool)) {
                    property = property.WithBooleanType();
                    var list = objects.ConvertAll(x => (bool)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(sbyte)) {
                    property = property.WithInt8Type();
                    var array = ToTypedArray<sbyte>(objects);
                    propertyTable.UseProperty(property).SetValues(array);
                }
                // byte not available in postgres
                else if (type == typeof(byte)) {
                    property = property.WithUInt8Type();
                    var array = ToTypedArray<byte>(objects);
                    propertyTable.UseProperty(property).SetValues(array);
                }
                else if (type == typeof(short)) {
                    property = property.WithInt16Type();
                    var array = ToTypedArray<short>(objects);
                    propertyTable.UseProperty(property).SetValues(array);
                }
                // ushort not available in postgres
                else if (type == typeof(ushort)) {
                    property = property.WithUInt16Type();
                    var array = ToTypedArray<ushort>(objects);
                    propertyTable.UseProperty(property).SetValues(array);
                }
                else if (type == typeof(int)) {
                    property = property.WithInt32Type();
                    var list = objects.ConvertAll(x => (int)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                // uint not available in postgres
                else if (type == typeof(uint)) {
                    property = property.WithUInt32Type();
                    var list = objects.ConvertAll(x => (uint)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(long)) {
                    property = property.WithInt64Type();
                    var list = objects.ConvertAll(x => (long)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                // ulong not available in postgres
                else if (type == typeof(ulong)) {
                    property = property.WithUInt64Type();
                    var list = objects.ConvertAll(x => (ulong)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(float)) {
                    property = property.WithFloat32Type();
                    var list = objects.ConvertAll(x => (float)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(double)) {
                    property = property.WithFloat64Type();
                    var list = objects.ConvertAll(x => (double)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(bool[])) {
                    var p = objects.Cast<bool[]>().Select(x => x.ToList()).ToList();
                    property = property.WithArrayType(ElementType.BOOLEAN);
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(short[])) {
                    var p = objects.Cast<short[]>().Select(x => x.ToList()).ToList();
                    property = property.WithArrayType(ElementType.SCALAR,DataType.INT16);
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(int[])) {
                    var p = objects.Cast<int[]>().Select(x => x.ToList()).ToList();
                    property = property.WithArrayType(ElementType.SCALAR, DataType.INT32);
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(long[])) {
                    var p = objects.Cast<long[]>().Select(x => x.ToList()).ToList();
                    property = property.WithArrayType(ElementType.SCALAR, DataType.INT64);
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(decimal[])) {
                    var count = ((decimal[])objects.FirstOrDefault()).Count();
                    if (count == 3) {
                        var list = new List<Vector3>();
                        foreach (var item in attribute.Value) {
                            var array = (decimal[])item;
                            list.Add(new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2])));
                        }

                        property = property.WithVector3Type();
                        propertyTable.UseProperty(property).SetValues(list.ToArray());
                    }
                    else if (count == 16) {
                        var list = new List<Matrix4x4>();
                        foreach (var item in objects) {
                            var array = (decimal[])item;
                            list.Add(new Matrix4x4(
                                 Convert.ToSingle(array[0]), Convert.ToSingle(array[4]), Convert.ToSingle(array[8]), Convert.ToSingle(array[12]),
                                 Convert.ToSingle(array[1]), Convert.ToSingle(array[5]), Convert.ToSingle(array[9]), Convert.ToSingle(array[13]),
                                 Convert.ToSingle(array[2]), Convert.ToSingle(array[6]), Convert.ToSingle(array[10]), Convert.ToSingle(array[14]),
                                 Convert.ToSingle(array[3]), Convert.ToSingle(array[7]), Convert.ToSingle(array[11]), Convert.ToSingle(array[15])
                            ));
                        }
                        property = property.WithMatrix4x4Type();
                        propertyTable.UseProperty(property).SetValues(list.ToArray());
                    }
                    else {
                        // todo add regular decimal
                    }
                }
                else {
                    property = property.WithStringType();
                    var list = objects.ConvertAll(x => x.ToString()).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                    // todo add other types?
                }
            }
        }


        var bytes = model.WriteGLB().Array;
        return bytes;

    }

    private static T[] ToTypedArray<T>(List<object> objects)
    {
        return objects.ConvertAll(x => (T)x).ToArray();
    }

    private static bool DrawTriangleWithBatchId(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty> mesh)
    {
        var normal = triangle.GetNormal();
        var prim = mesh.UsePrimitive(material);
        var vectors = triangle.ToVectors();
        var indices = prim.AddTriangleWithBatchId(vectors, normal, triangle.GetBatchId());
        return indices.Item1 > 0;
    }

    private static bool DrawTriangleWithFeatureId(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty> mesh)
    {
        var normal = triangle.GetNormal();
        var prim = mesh.UsePrimitive(material);
        var vectors = triangle.ToVectors();
        var indices = prim.AddTriangleWithFeatureId(vectors, normal, triangle.GetBatchId());
        return indices.Item1 > 0;
    }

}
