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
using AlphaMode = SharpGLTF.Materials.AlphaMode;

namespace Wkb2Gltf;

public static class GlbCreator
{
    public static byte[] GetGlb(List<List<Triangle>> triangles, string copyright = "", bool addOutlines = false, string defaultColor = "#FFFFFF", string defaultMetallicRoughness = "#008000", bool defaultDoubleSided = true, Dictionary<string, List<object>> attributes = null, bool createGltf = false, AlphaMode defaultAlphaMode = AlphaMode.OPAQUE, bool doubleSided = false, bool YAxisUp = true)
    {
        var materialCache = new MaterialsCache();
        var shader = new Shader();
        shader.PbrMetallicRoughness = new PbrMetallicRoughness() { BaseColor = defaultColor, MetallicRoughness = defaultMetallicRoughness };
        var defaultMaterial = MaterialCreator.CreateMaterial(shader, defaultDoubleSided, defaultAlphaMode);

        var meshBatchId = new MeshBuilder<VertexPositionNormal, VertexWithBatchId, VertexEmpty>("mesh");
        var meshFeatureIds = new MeshBuilder<VertexPositionNormal, VertexWithFeatureId, VertexEmpty>("mesh");

        foreach (var tri in triangles) {
            foreach (var triangle in tri) {
                MaterialBuilder material;

                if (triangle.Shader != null) {
                    material = materialCache.GetMaterialBuilderByShader(triangle.Shader, doubleSided, defaultAlphaMode);
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

        // When there are no triangles, the model is empty, return null
        if (model.LogicalNodes.Count == 0) {
            return null;
        }
        if(YAxisUp) {
            var localTransform = new Matrix4x4(
                1, 0, 0, 0,
                0, 0, -1, 0,
                0, 1, 0, 0,
                0, 0, 0, 1);
            model.LogicalNodes.First().LocalTransform = new SharpGLTF.Transforms.AffineTransform(localTransform);
        }

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
                // the 3d tiles metadata spec does not allow for null values (but only 'nodata' values), so we need to determine the
                // nodata value for each type. 
                var stringNodata = "";
                var floatNodata = (float)uint.MinValue; // bit of a workaround, to get around the fact that we can't use float.MinValue as nodata
                var shortNodata = short.MinValue;
                var intNodata = int.MinValue;
                var sbyteNodata = sbyte.MinValue;
                var byteNodata = byte.MaxValue; // let's take the byte max value (255) as nodata
                var ushortNodata = ushort.MaxValue; // let's take the ushort max value (65535) as nodata
                var uintNodata = uint.MaxValue; // let's take the uint max value as nodata
                var longNodata = long.MinValue;
                var ulongNodata = ulong.MaxValue; // let's take the ulong max value as nodata
                var doubleNodata = double.MinValue;

                // in the attribute dictionary, find the type o the first value that is not dbnull
                var firstType = attribute.Value.Where(x => x != DBNull.Value).FirstOrDefault();
                if(firstType == null) {
                    throw(new Exception($"All values of attribute '{attribute.Key}' are Null, can't determine type"));
                }
                var type = firstType.GetType();
                var objects = attribute.Value;
                var property = schemaClass.UseProperty(attribute.Key);

                var nullCount = objects.OfType<DBNull>().Count();
                // sbyte not available in postgres
                if (type == typeof(bool)) {
                    property = property.WithBooleanType();
                    var list = objects.ConvertAll(x => (bool)x).ToArray();
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(string)) {
                    var array = objects.Select(x => x ==  DBNull.Value ? stringNodata : x.ToString()).ToArray();
                    property = property.WithStringType(nullCount > 0 ? stringNodata : null);
                    propertyTable.UseProperty(property).SetValues(array);
                }
                else if (type == typeof(sbyte)) {
                    var list = objects.Select(item => item is sbyte value ? value : sbyteNodata).ToArray();
                    property = property.WithInt8Type(nullCount > 0 ? sbyteNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                // byte not available in postgres
                else if (type == typeof(byte)) {
                    var list = objects.Select(item => item is byte value ? value : byteNodata).ToArray();
                    property = property.WithUInt8Type(nullCount > 0 ? byteNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(short)) {
                    var list = objects.Select(item => item is short value ? value : shortNodata).ToArray();
                    property = property.WithInt16Type(nullCount > 0 ? shortNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                // ushort not available in postgres
                else if (type == typeof(ushort)) {
                    var list = objects.Select(item => item is ushort value ? value : ushortNodata).ToArray();
                    property = property.WithUInt16Type(nullCount > 0 ? ushortNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(int)) {
                    var list = objects.Select(item => item is int value ? value : intNodata).ToArray();
                    property = property.WithInt32Type(nullCount > 0 ? intNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                // uint not available in postgres
                else if (type == typeof(uint)) {
                    var list = objects.Select(item => item is uint value ? value : uintNodata).ToArray();
                    property = property.WithUInt32Type(nullCount > 0 ? uintNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(long)) {
                    var list = objects.Select(item => item is long value ? value : longNodata).ToArray();
                    property = property.WithInt64Type(nullCount > 0 ? longNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                // ulong not available in postgres
                else if (type == typeof(ulong)) {
                    var list = objects.Select(item => item is ulong value ? value : ulongNodata).ToArray();
                    property = property.WithUInt64Type(nullCount > 0 ? ulongNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(float)) {
                    var list = objects.Select(item => item is float value ? value : floatNodata).ToArray();
                    property = property.WithFloat32Type(nullCount > 0 ? floatNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(decimal)) {
                    var list = objects.Select(item => item is decimal value ? (float)value : floatNodata).ToArray();
                    property = property.WithFloat32Type(nullCount > 0 ? floatNodata : null);
                    propertyTable.UseProperty(property).SetValues(list);
                }
                else if (type == typeof(double)) {
                    var list = objects.Select(item => item is double value ? (float)value : doubleNodata).ToArray();
                    property = property.WithFloat64Type(nullCount > 0 ? doubleNodata : null);
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

                        foreach (var obj in objects) {
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
                    if (HasNull(objects)) {
                        throw new NotSupportedException("Null values are not supported in arrays");
                    }
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

    private static bool HasNull(List<object> objects)
    {
        return objects.Any(x => x == DBNull.Value);
    }

    private static bool IsFixed(List<object> objects, int check)
    {
        return objects.All(x => ((decimal[])x).Count() == check);
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
