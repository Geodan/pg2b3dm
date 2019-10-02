using System.Collections.Generic;
using Wkx;

namespace Wkb2Gltf
{
    public class GeometryRecord
    {
        public GeometryRecord(int batchId)
        {
            BatchId = batchId;
            HexColors = new string[0];
            Attributes = new string[0];
        }
        public string Id { get; set; }
        public Geometry Geometry { get; set; }

        public string[] HexColors { get; set; }

        public int BatchId { get; set; }

        public object[] Attributes { get; set; }
    }
}
