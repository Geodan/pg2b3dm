using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Wkb2Gltf
{
    public static class B3dmCreator
    {
        public static B3dm.Tile.B3dm GetB3dm(Dictionary<string, List<object>> attributes, List<Triangle> triangleCollection, string copyright="")
        {
            var bytes = GlbCreator.GetGlb(triangleCollection, copyright);
            var b3dm = new B3dm.Tile.B3dm(bytes);

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
            return b3dm;
        }
    }
}
