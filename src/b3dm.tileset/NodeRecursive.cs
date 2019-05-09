using System.Collections.Generic;
using Wkb2Gltf;

namespace B3dm.Tileset
{
    public static class NodeRecursive
    {
        public static List<BoundingBox3D> GetBoundingBoxes3D(Node node)
        {
            var bboxes = new List<BoundingBox3D>();
            foreach (var f in node.Features) {
                bboxes.Add(f.BoundingBox3D);
            }

            foreach (var child in node.Children) {
                var newboxes = GetBoundingBoxes3D(child);
                bboxes.AddRange(newboxes);
            }
            return bboxes;
        }

    }
}
