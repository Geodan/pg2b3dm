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
        // When there are no triangles, the model is empty, return null
        if (model.LogicalNodes.Count == 0) {
            return null;
        }

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
                else if (type == typeof(string)) {
                    property = property.WithStringType();
                    var array = ToTypedArray<string>(objects);
                    propertyTable.UseProperty(property).SetValues(array);
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
                else if (type == typeof(decimal)) {
                    property = property.WithFloat32Type();
                    var list = objects.ConvertAll(x => Convert.ToSingle(x)).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(double)) {
                    property = property.WithFloat64Type();
                    var list = objects.ConvertAll(x => (double)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(bool[])) {
                    var p = objects.Cast<bool[]>().Select(x => x.ToList()).ToList();
                    property = property.WithBooleanArrayType();
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(string[])) {
                    var p = objects.Cast<string[]>().Select(x => x.ToList()).ToList();
                    property = property.WithStringArrayType();
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(short[])) {
                    var p = objects.Cast<short[]>().Select(x => x.ToList()).ToList();
                    property = property.WithInt16ArrayType();
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(int[])) {
                    var p = objects.Cast<int[]>().Select(x => x.ToList()).ToList();
                    property = property.WithInt32ArrayType();
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(long[])) {
                    var p = objects.Cast<long[]>().Select(x => x.ToList()).ToList();
                    property = property.WithInt64ArrayType();
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(float[])) {
                    var p = objects.Cast<float[]>().Select(x => x.ToList()).ToList();
                    property = property.WithFloat32ArrayType();
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(double[])) {
                    var p = objects.Cast<double[]>().Select(x => x.ToList()).ToList();
                    property = property.WithFloat64ArrayType();
                    propertyTable.UseProperty(property).SetArrayValues(p);
                }
                else if (type == typeof(decimal[,])) {
                    if (IsConstantArray(objects, 3)) {
                        var result = objects.Cast<decimal[,]>()
                            .Select(
                                obj1 => Enumerable.Range(0, obj1.GetLength(0))
                                .Select(j => new Vector3(Convert.ToSingle(obj1[j, 0]), Convert.ToSingle(obj1[j, 1]), Convert.ToSingle(obj1[j, 2])))
                                .ToList()
                                ).ToList();
                        property = property.WithVector3ArrayType();
                        propertyTable.UseProperty(property).SetArrayValues(result);
                    }
                    else if (IsConstantArray(objects, 16)) {
                        var result = objects.Cast<decimal[,]>()
                            .Select(
                                obj1 => Enumerable.Range(0, obj1.GetLength(0))
                                .Select(j => ToMatrix4x4(obj1, j))
                                .ToList()
                                ).ToList();
                        property = property.WithMatrix4x4ArrayType();
                        propertyTable.UseProperty(property).SetArrayValues(result);
                    }
                    else {
                        // TODO: this dumps all values into one array, but we need to split it up into multiple arrays
                        var result = new List<List<float>>();

                        foreach(var obj in objects) {
                            var array = (decimal[,])obj;
                            var p = new List<float>();
                            var items = array.GetLength(0);
                            for (var j = 0; j < items; j++) {
                                for (var k = 0; k < array.GetLength(1); k++) {
                                    p.Add(Convert.ToSingle(array[j, k]));
                                }
                            }
                            result.Add(p);
                        }

                        property = property.WithFloat32ArrayType();
                        propertyTable.UseProperty(property).SetArrayValues(result);
                    }
                }
                else if (type == typeof(decimal[])) {
                    if (IsFixed(objects, 3)) {
                        var list = attribute.Value.Select(item => {
                            var array = (decimal[])item;
                            return new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
                        }).ToList();

                        property = property.WithVector3Type();
                        propertyTable.UseProperty(property).SetValues(list.ToArray());
                    }
                    else if (IsFixed(objects, 16)) {
                        var list = objects.Select(item => ToMatrix4x4(item)).ToList();
                        property = property.WithMatrix4x4Type();
                        propertyTable.UseProperty(property).SetValues(list.ToArray());
                    }
                    else {
                        var result = objects.Cast<decimal[]>()
                            .Select(arr => arr.Select(Convert.ToSingle).ToList())
                            .ToList();
                        property = property.WithFloat32ArrayType();
                        propertyTable.UseProperty(property).SetArrayValues(result);
                    }
                }
                else {
                    throw new NotSupportedException($"Type {type} not supported as metadata");
                }
            }
        }

        var bytes = model.WriteGLB().Array;
        return bytes;
    }

    private static Matrix4x4 ToMatrix4x4(decimal[,] obj1, int j)
    {
        return new Matrix4x4(
            Convert.ToSingle(obj1[j, 0]), Convert.ToSingle(obj1[j, 4]), Convert.ToSingle(obj1[j, 8]), Convert.ToSingle(obj1[j, 12]),
            Convert.ToSingle(obj1[j, 1]), Convert.ToSingle(obj1[j, 5]), Convert.ToSingle(obj1[j, 9]), Convert.ToSingle(obj1[j, 13]),
            Convert.ToSingle(obj1[j, 2]), Convert.ToSingle(obj1[j, 6]), Convert.ToSingle(obj1[j, 10]), Convert.ToSingle(obj1[j, 14]),
            Convert.ToSingle(obj1[j, 3]), Convert.ToSingle(obj1[j, 7]), Convert.ToSingle(obj1[j, 11]), Convert.ToSingle(obj1[j, 15])
        );
    }

    private static Matrix4x4 ToMatrix4x4(object item)
    {
        return new Matrix4x4(
            Convert.ToSingle(((decimal[])item)[0]), Convert.ToSingle(((decimal[])item)[4]), Convert.ToSingle(((decimal[])item)[8]), Convert.ToSingle(((decimal[])item)[12]),
            Convert.ToSingle(((decimal[])item)[1]), Convert.ToSingle(((decimal[])item)[5]), Convert.ToSingle(((decimal[])item)[9]), Convert.ToSingle(((decimal[])item)[13]),
            Convert.ToSingle(((decimal[])item)[2]), Convert.ToSingle(((decimal[])item)[6]), Convert.ToSingle(((decimal[])item)[10]), Convert.ToSingle(((decimal[])item)[14]),
            Convert.ToSingle(((decimal[])item)[3]), Convert.ToSingle(((decimal[])item)[7]), Convert.ToSingle(((decimal[])item)[11]), Convert.ToSingle(((decimal[])item)[15])
        );
    }

    private static bool IsConstantArray(List<object> objects, int check)
    {
        return objects.All(item => ((decimal[,])item).GetLength(1) == check);
    }

    private static bool IsFixed(List<object> objects, int check)
    {
        return objects.All(x => ((decimal[])x).Count() == check);
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
