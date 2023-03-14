using System;
using System.Collections.Generic;
using Wkx;

namespace Wkb2Gltf;

public static class Triangulator
{
    public static List<Triangle> GetTriangles(PolyhedralSurface polyhedralsurface, int batchId, ShaderColors shadercolors =null, double areaTolerence = 0.01)
    {
        var degenerated_triangles = 0;
        var allTriangles = new List<Triangle>();
        for(var i=0;i<polyhedralsurface.Geometries.Count;i++) {
            var geometry = polyhedralsurface.Geometries[i];
            var triangle = GetTriangle(geometry, batchId);

            if (triangle!=null && shadercolors != null && triangle.Area() > areaTolerence) {
                shadercolors.Validate(polyhedralsurface.Geometries.Count);
                triangle.Shader = shadercolors.ToShader(i);
            }

            if (triangle != null) {
                allTriangles.Add(triangle);
            }
            else {
                degenerated_triangles++;
            }
        }

        return allTriangles;
    }


    public static Triangle GetTriangle(Polygon geometry, int batchId)
    {
        var triangle = ToTriangle(geometry, batchId);

        if (!triangle.IsDegenerated()) {
            return triangle;
        }
        return null;
    }

    private static Triangle ToTriangle(Polygon geometry, int batchId)
    {
        var pnts = geometry.ExteriorRing.Points;
        if (pnts.Count != 4) {
            throw new ArgumentOutOfRangeException($"Expected number of vertices in triangles: 4, actual: {pnts.Count}");
        }

        var triangle = new Triangle(pnts[0], pnts[1], pnts[2], batchId);
        return triangle;
    }
}
