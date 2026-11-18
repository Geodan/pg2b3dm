using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace B3dm.Tileset.Tests;

public class ThreeTzWriterTests
{
    [Test]
    public void CreateArchiveTest()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(folder, "content"));
        File.WriteAllText(Path.Combine(folder, "tileset.json"), "{}");
        File.WriteAllText(Path.Combine(folder, "content", "0_0_0.glb"), "glbdata");
        var archive = folder + ".3tz";
        try {
            ThreeTzWriter.Create(folder, archive);

            using var zip = ZipFile.OpenRead(archive);
            Assert.That(zip.Entries.Select(e => e.FullName).ToList(),
                Is.EqualTo(new[] { "tileset.json", "content/0_0_0.glb", "@3dtilesIndex1@" }));
            Assert.That(zip.Entries.All(e => e.CompressedLength == e.Length));

            var index = zip.Entries.Last();
            using var ms = new MemoryStream();
            using (var s = index.Open()) { s.CopyTo(ms); }
            var bytes = ms.ToArray();
            Assert.That(bytes.Length, Is.EqualTo(48));

            // the first (lowest) hash must belong to one of the files and sorting must hold
            var hash1 = bytes.AsSpan(0, 16).ToArray();
            var hash2 = bytes.AsSpan(24, 16).ToArray();
            Assert.That(BitConverter.ToUInt64(hash1, 0) <= BitConverter.ToUInt64(hash2, 0));

            var tilesetHash = MD5.HashData(Encoding.UTF8.GetBytes("tileset.json"));
            var found = Enumerable.Range(0, 2).Any(i => bytes.AsSpan(i * 24, 16).SequenceEqual(tilesetHash) && BitConverter.ToUInt64(bytes, i * 24 + 16) == 0);
            Assert.That(found);
        }
        finally {
            Directory.Delete(folder, true);
            File.Delete(archive);
        }
    }

    [Test]
    public void NormalizePathTest()
    {
        Assert.That(ThreeTzWriter.NormalizePath("\\content\\a.glb"), Is.EqualTo("content/a.glb"));
    }
}
