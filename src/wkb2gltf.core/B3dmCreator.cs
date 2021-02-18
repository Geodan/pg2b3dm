using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wkb2Gltf
{
    public static class B3dmCreator
    {
        public static B3dm.Tile.B3dm GetB3dm(string attributesColumn, List<object> attributes, List<Triangle> triangleCollection, bool compress = false)
        {
            var bytes = GlbCreator.GetGlb(triangleCollection, compress);
            var b3dm = new B3dm.Tile.B3dm(bytes);

            if (attributes.Count > 0) {
                var featureTable = new FeatureTable {
                    BATCH_LENGTH = attributes.Count
                };
                b3dm.FeatureTableJson = JsonConvert.SerializeObject(featureTable);


                if (attributesColumn != string.Empty) {
                    var batchtable = new BatchTable();

                    var item = new BatchTableItem {
                        Name = attributesColumn,
                        Values = attributes.ToArray()
                    };
                    batchtable.BatchTableItems.Add(item);
                    var json = JsonConvert.SerializeObject(batchtable, new BatchTableJsonConverter(typeof(BatchTable)));
                    b3dm.BatchTableJson = json;
                }
            }
            return b3dm;
        }
    }
}
