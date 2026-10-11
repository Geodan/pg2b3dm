using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Newtonsoft.Json;
using Npgsql;
using SharpGLTF.Materials;
using Wkb2Gltf;
using Wkx;
using WkbTriangle = Wkb2Gltf.Triangle;

namespace GltfExport;

class Program
{
    static void Main(string[] args)
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        Console.WriteLine($"Tool: GltfExport {version}");

        Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
        {
            Console.WriteLine($"Table: {o.Table}");
            Console.WriteLine($"Geometry column: {o.GeometryColumn}");
            Console.WriteLine($"Id column: {o.IdColumn}");
            Console.WriteLine($"Shaders column: {(string.IsNullOrEmpty(o.ShadersColumn) ? "-" : o.ShadersColumn)}");
            Console.WriteLine($"Output directory: {o.Output}");
            Console.WriteLine($"Default color: {o.DefaultColor}");
            Console.WriteLine($"Default metallic roughness: {o.DefaultMetallicRoughness}");
            Console.WriteLine($"Double sided: {o.DoubleSided}");
            Console.WriteLine($"Default alpha mode: {o.DefaultAlphaMode}");
            Console.WriteLine($"Alpha cutoff: {o.AlphaCutoff}");

            Directory.CreateDirectory(o.Output);

            var sql = BuildQuery(o.Table, o.GeometryColumn, o.IdColumn, o.ShadersColumn);

            using var conn = new NpgsqlConnection(o.Connection);
            conn.Open();

            var cmd = new NpgsqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            var written = 0;
            var skipped = 0;

            while (reader.Read())
            {
                var id = reader.GetFieldValue<object>(0).ToString()!;
                var safeId = SanitizeFileName(id);

                byte[]? glbBytes = null;
                try
                {
                    var stream = reader.GetStream(1);
                    var geometry = Geometry.Deserialize<WkbSerializer>(stream);

                    ShaderColors? shaderColors = null;
                    if (!string.IsNullOrEmpty(o.ShadersColumn))
                    {
                        var json = reader.IsDBNull(2) ? null : reader.GetString(2);
                        if (json != null)
                        {
                            shaderColors = JsonConvert.DeserializeObject<ShaderColors>(json);
                        }
                    }

                    var record = new GeometryRecord(0) { Geometry = geometry, Shader = shaderColors };
                    var triangles = record.GetTriangles(translation: null);

                    glbBytes = GlbCreator.GetGlb(
                        triangles: new List<List<WkbTriangle>> { triangles },
                        createGltf: true,
                        defaultColor: o.DefaultColor,
                        defaultMetallicRoughness: o.DefaultMetallicRoughness,
                        defaultDoubleSided: o.DoubleSided,
                        defaultAlphaMode: o.DefaultAlphaMode,
                        alphaCutoff: o.AlphaCutoff
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: skipping id '{id}': {ex.Message}");
                    skipped++;
                    continue;
                }

                if (glbBytes == null)
                {
                    Console.WriteLine($"Warning: skipping id '{id}': no geometry produced.");
                    skipped++;
                    continue;
                }

                var outputFile = Path.Combine(o.Output, $"{safeId}.glb");
                File.WriteAllBytes(outputFile, glbBytes);
                written++;
            }

            reader.Close();
            conn.Close();

            Console.WriteLine($"Done. Written: {written}, Skipped: {skipped}.");
        });
    }

    private static string BuildQuery(string table, string geomColumn, string idColumn, string shadersColumn)
    {
        var select = $"SELECT {idColumn}::text, ST_AsBinary({geomColumn})";
        if (!string.IsNullOrEmpty(shadersColumn))
        {
            select += $", {shadersColumn}";
        }
        return $"{select} FROM {table}";
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}
