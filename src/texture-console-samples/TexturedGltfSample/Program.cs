using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Wkb2Gltf;
using WkbTriangle = Wkb2Gltf.Triangle;
using WkxPoint = Wkx.Point;

var triangle = new WkbTriangle(
    new WkxPoint(0, 0, 0),
    new WkxPoint(1, 0, 0),
    new WkxPoint(0, 1, 0),
    0
) {
    TextureImageData = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII="),
    TextureCoordinates = (new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1))
};

var glb = GlbCreator.GetGlb(
    triangles: new List<List<WkbTriangle>>() { new List<WkbTriangle>() { triangle } },
    createGltf: true,
    useTexturePipeline: true
);

var outputFile = Path.Combine(AppContext.BaseDirectory, "textured_sample.glb");
File.WriteAllBytes(outputFile, glb);
Console.WriteLine($"Created {outputFile}");
