using System.Collections.Generic;
using Wkb2Gltf;

namespace B3dm.Tileset
{
    public class Node
    {
        public Node()
        {
            Children = new List<Node>();
            Features = new List<Feature>();
        }
        public List<Node> Children { get; set; }
        public List<Feature> Features { get; set; }

        public int Id {get;set;}

        public BoundingBox3D CalculateBoundingBox3D()
        {
            var bboxes = NodeRecursive.GetBoundingBoxes3D(this);
            var bbox = BoundingBoxCalculator.GetBoundingBox(bboxes);
            return bbox;
        }
    }
}
