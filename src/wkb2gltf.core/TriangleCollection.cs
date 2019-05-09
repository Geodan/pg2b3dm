using System.Collections.Generic;
using System.Numerics;

namespace Wkb2Gltf
{
    public class TriangleCollection: List<Triangle>
    {

        public List<Vector3> GetNormals()
        {
            var normals = new List<Vector3>();
            foreach (var triangle in this)
            {
                normals.Add(triangle.GetNormal());
            }
            return normals;
        }

        public List<Vector3> GetFaces(List<Vector3> normals)
        {
            var faces = new List<Vector3>();

            foreach (var normal in normals)
            {
                // heuh???
                faces.Add(normal);
                faces.Add(normal);
                faces.Add(normal);
            }

            return faces;
        }

    }
}
