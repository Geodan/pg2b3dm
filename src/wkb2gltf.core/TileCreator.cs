using System.Collections.Generic;
using System.Linq;
using B3dmCore;
using Newtonsoft.Json;
using SharpGLTF.Materials;

namespace Wkb2Gltf;

public static class TileCreator
{
    public static byte[] GetTile(Dictionary<string, List<object>> attributes, List<List<Triangle>> triangleCollection, string copyright = "", bool addOutlines = false, string defaultColor = "#FFFFFF", string defaultMetallicRoughness = "#008000", bool doubleSided = true, AlphaMode defaultAlphaMode = AlphaMode.OPAQUE, bool createGltf = false, bool YAxisUp = true)
    {
        var bytes = GlbCreator.GetGlb(triangleCollection, copyright, addOutlines, defaultColor, defaultMetallicRoughness, doubleSided, attributes, createGltf, defaultAlphaMode, doubleSided, YAxisUp);

        if(bytes== null) {
            return null;
        }

        if(createGltf) {
            return bytes;
        }

        var b3dm = new B3dm(bytes);

        if (attributes.Count > 0) {
            var featureTable = new FeatureTable {
                BATCH_LENGTH = attributes.First().Value.Count
            };
            b3dm.FeatureTableJson = JsonConvert.SerializeObject(featureTable);

            if (attributes.Count>0) {
                var batchtable = new BatchTable();

                foreach(var attribute in attributes) {

                    var item = new BatchTableItem {
                        Name = attribute.Key,
                        Values = attribute.Value.ToArray()
                    };
                    batchtable.BatchTableItems.Add(item);
                }
                var json = JsonConvert.SerializeObject(batchtable, new BatchTableJsonConverter(typeof(BatchTable)));
                b3dm.BatchTableJson = json;
            }
        }
        var tileBytes = b3dm.ToBytes();

        return tileBytes;
    }
}
