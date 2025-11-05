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
        // 2. Crease edges (adjacent triangles with fundamentally different orientations)
        
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
                                    
                                    // Check if normals are in similar orientation
                                    // Use a permissive threshold that allows variation within the same general direction
                                    // but separates truly different faces
                                    var dotProduct = System.Numerics.Vector3.Dot(triangleNormal, adjacentNormal);
                                    
                                    // Categorize surfaces by orientation:
                                    // - Horizontal (floor/roof): |Z| > 0.7
                                    // - Vertical (walls): |Z| < 0.7
                                    var isHorizontal1 = System.Math.Abs(triangleNormal.Z) > 0.7f;
                                    var isHorizontal2 = System.Math.Abs(adjacentNormal.Z) > 0.7f;
                                    
                                    // Only mark as crease if surfaces have DIFFERENT orientations
                                    // (horizontal vs vertical, like floor-wall or wall-roof)
                                    // Walls with different normals should NOT be creases
                                    if (isHorizontal1 == isHorizontal2) {
                                        // Same orientation - not a crease
                                        isOutline = false;
                                    }
                                    // Different orientation - it's a crease
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
        
        // Deduplicate edges - an edge might be added from both triangles that share it
        var uniqueEdges = new HashSet<string>();
        var deduplicatedOutlines = new List<uint>();
        
        for (var i = 0; i < outlines.Count; i += 2) {
            var v1 = outlines[i];
            var v2 = outlines[i + 1];
            var edgeKey = v1 < v2 ? $"{v1}-{v2}" : $"{v2}-{v1}";
            
            if (uniqueEdges.Add(edgeKey)) {
                // This edge hasn't been seen before, add it
                deduplicatedOutlines.Add(v1);
                deduplicatedOutlines.Add(v2);
            }
        }
        
        return deduplicatedOutlines;
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