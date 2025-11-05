using System.Collections.Generic;
using System.Linq;
using SharpGLTF.Geometry;
using SharpGLTF.Schema2;
using Wkb2Gltf.extensions;

namespace Wkb2Gltf.outlines;
public static class OutlineDetection
{
    /// <summary>
    /// Get outlines of a meshPrimitive
    /// </summary>
    public static List<uint> GetOutlines(MeshPrimitive meshPrimitive, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var indices = meshPrimitive.GetIndices().ToArray();
        var outlines = new List<uint>();

        var tris = Toolkit.EvaluateTriangles(meshPrimitive).ToList();
        var triangles = GetTriangles(tris);
        var outline = GetOutlines2(triangles, normalTolerance: normalTolerance, distanceTolerance);
        outlines.AddRange(outline);

        var res = new List<uint>();
        foreach (var l in outlines) {
            res.Add(indices[l]);
        }

        return res;
    }

    public static List<uint> GetOutlines2(List<Triangle> triangles, double normalTolerance = 0.01, double distanceTolerance = 0.01)
    {
        var outlines = new List<uint>();
        
        // Build global adjacency map to detect all edge connections
        var adjacency = Adjacency.GetAdjacencyList(triangles, distanceTolerance);
        
        // Find edges that should be outlined:
        // 1. Boundary edges (no adjacent triangle)
        // 2. Crease edges (adjacent triangle with significantly different normal)
        
        var creaseAngleThreshold = 0.707; // ~45 degrees - edges with larger angle difference are creases
        
        for (var i = 0; i < triangles.Count; i++) {
            var triangle = triangles[i];
            var triangleNormal = triangle.GetNormal();
            
            // Check each edge of the triangle
            var edges = new List<(int from, int to)> { (0, 1), (1, 2), (2, 0) };
            
            foreach (var (from, to) in edges) {
                var isOutline = true;
                
                // Check if this edge has an adjacent triangle
                if (adjacency.TryGetValue(i, out var adjacentEdges)) {
                    // Check if this specific edge is in the adjacency list
                    var hasAdjacentOnThisEdge = adjacentEdges.Any(e => 
                        (e.from == from && e.to == to) || (e.from == to && e.to == from));
                    
                    if (hasAdjacentOnThisEdge) {
                        // Edge has an adjacent triangle - check if it's a crease
                        // Find the adjacent triangle
                        for (var j = 0; j < triangles.Count; j++) {
                            if (i == j) continue;
                            
                            var sharedPoints = BoundaryDetection.GetSharedPoints(triangle, triangles[j], distanceTolerance);
                            if (sharedPoints.first.Count == 2) {
                                // Check if this is the edge we're looking at
                                if ((sharedPoints.first.Contains(from) && sharedPoints.first.Contains(to))) {
                                    // This is the adjacent triangle for this edge
                                    var adjacentNormal = triangles[j].GetNormal();
                                    var dotProduct = System.Numerics.Vector3.Dot(triangleNormal, adjacentNormal);
                                    
                                    // If normals are similar, this is NOT a crease edge
                                    if (dotProduct > creaseAngleThreshold) {
                                        isOutline = false;
                                    }
                                    // If dotProduct <= creaseAngleThreshold, it's a crease - keep as outline
                                    break;
                                }
                            }
                        }
                    }
                }
                // If no adjacent triangle, it's a boundary edge - keep as outline
                
                if (isOutline) {
                    var offset = (uint)(i * 3);
                    outlines.Add(offset + (uint)from);
                    outlines.Add(offset + (uint)to);
                }
            }
        }
        
        return outlines;
    }

    private static List<Triangle> GetTriangles(List<(IVertexBuilder A, IVertexBuilder B, IVertexBuilder C, Material Material)> tris)
    {
        var res = new List<Triangle>();
        foreach (var tri in tris) {
            var p0 = tri.A.GetGeometry().GetPosition().ToPoint();
            var p1 = tri.B.GetGeometry().GetPosition().ToPoint();
            var p2 = tri.C.GetGeometry().GetPosition().ToPoint();

            var t = new Triangle(p0, p1, p2, 0);
            res.Add(t);
        }

        return res;
    }

}