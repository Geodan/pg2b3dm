using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace B3dm.Tileset;

/// <summary>
/// Writes a 3D Tiles Archive (3tz, spec v1.4): a ZIP with stored (uncompressed) entries
/// and a trailing '@3dtilesIndex1@' index file.
/// </summary>
public static class ThreeTzWriter
{
    public const string IndexFileName = "@3dtilesIndex1@";
    public const string Extension = ".3tz";

    private class Entry
    {
        public string Name { get; set; }
        public byte[] NameBytes { get; set; }
        public uint Crc { get; set; }
        public uint Size { get; set; }
        public uint Offset { get; set; }
        public ushort Time { get; set; }
        public ushort Date { get; set; }
    }

    public static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    public static List<string> GetFiles(string folder)
    {
        var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
            .Select(f => NormalizePath(Path.GetRelativePath(folder, f)))
            .OrderBy(f => f != "tileset.json")
            .ThenBy(f => f, StringComparer.Ordinal)
            .ToList();
        return files;
    }

    public static void Create(string folder, string archivePath)
    {
        var files = GetFiles(folder);
        if (!files.Contains("tileset.json")) {
            throw new InvalidOperationException($"No tileset.json found in {folder}, cannot create 3tz.");
        }
        foreach (var f in files) {
            if (f.Contains(".3tz", StringComparison.OrdinalIgnoreCase) || f.Contains(".3dtiles.zip", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException($"File name '{f}' contains '.3tz' or '.3dtiles.zip', which is not allowed in a 3tz archive.");
            }
        }

        var entries = new List<Entry>();
        using var fs = new FileStream(archivePath, FileMode.Create, FileAccess.ReadWrite);
        using var bw = new BinaryWriter(fs, Encoding.UTF8, true);

        var now = DateTime.Now;
        var (time, date) = ToDosDateTime(now);

        foreach (var file in files) {
            var data = File.ReadAllBytes(Path.Combine(folder, file.Replace('/', Path.DirectorySeparatorChar)));
            if (data.LongLength > uint.MaxValue || fs.Position > uint.MaxValue) {
                throw new InvalidOperationException("3tz archives do not support files larger than 4 GB.");
            }
            var entry = new Entry {
                Name = file,
                NameBytes = Encoding.UTF8.GetBytes(file),
                Crc = Crc32.HashToUInt32(data),
                Size = (uint)data.Length,
                Offset = (uint)fs.Position,
                Time = time,
                Date = date
            };
            WriteLocalHeader(bw, entry);
            bw.Write(data);
            entries.Add(entry);
        }

        var index = BuildIndex(entries.Select(e => (e.Name, (ulong)e.Offset)));
        var indexEntry = new Entry {
            Name = IndexFileName,
            NameBytes = Encoding.UTF8.GetBytes(IndexFileName),
            Crc = Crc32.HashToUInt32(index),
            Size = (uint)index.Length,
            Offset = (uint)fs.Position,
            Time = time,
            Date = date
        };
        WriteLocalHeader(bw, indexEntry);
        bw.Write(index);
        entries.Add(indexEntry);

        var cdStart = (uint)fs.Position;
        foreach (var e in entries) {
            bw.Write(0x02014b50u);
            bw.Write((ushort)20); // version made by
            bw.Write((ushort)20); // version needed
            bw.Write((ushort)0x0800); // UTF-8 names
            bw.Write((ushort)0); // stored
            bw.Write(e.Time);
            bw.Write(e.Date);
            bw.Write(e.Crc);
            bw.Write(e.Size);
            bw.Write(e.Size);
            bw.Write((ushort)e.NameBytes.Length);
            bw.Write((ushort)0); // extra
            bw.Write((ushort)0); // comment
            bw.Write((ushort)0); // disk
            bw.Write((ushort)0); // internal attrs
            bw.Write(0u); // external attrs
            bw.Write(e.Offset);
            bw.Write(e.NameBytes);
        }
        var cdSize = (uint)fs.Position - cdStart;

        bw.Write(0x06054b50u);
        bw.Write((ushort)0);
        bw.Write((ushort)0);
        bw.Write((ushort)entries.Count);
        bw.Write((ushort)entries.Count);
        bw.Write(cdSize);
        bw.Write(cdStart);
        bw.Write((ushort)0);
    }

    private static void WriteLocalHeader(BinaryWriter bw, Entry e)
    {
        bw.Write(0x04034b50u);
        bw.Write((ushort)20);
        bw.Write((ushort)0x0800);
        bw.Write((ushort)0); // stored
        bw.Write(e.Time);
        bw.Write(e.Date);
        bw.Write(e.Crc);
        bw.Write(e.Size);
        bw.Write(e.Size);
        bw.Write((ushort)e.NameBytes.Length);
        bw.Write((ushort)0);
        bw.Write(e.NameBytes);
    }

    public static byte[] BuildIndex(IEnumerable<(string name, ulong offset)> files)
    {
        var items = files.Select(f => {
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(NormalizePath(f.name)));
            return (hash, f.offset);
        })
        .OrderBy(i => BitConverter.ToUInt64(i.hash, 0))
        .ThenBy(i => BitConverter.ToUInt64(i.hash, 8))
        .ToList();

        var result = new byte[items.Count * 24];
        for (var i = 0; i < items.Count; i++) {
            Buffer.BlockCopy(items[i].hash, 0, result, i * 24, 16);
            BitConverter.TryWriteBytes(result.AsSpan(i * 24 + 16, 8), items[i].offset);
        }
        return result;
    }

    private static (ushort time, ushort date) ToDosDateTime(DateTime dt)
    {
        var year = Math.Max(dt.Year, 1980);
        var date = (ushort)(((year - 1980) << 9) | (dt.Month << 5) | dt.Day);
        var time = (ushort)((dt.Hour << 11) | (dt.Minute << 5) | (dt.Second / 2));
        return (time, date);
    }
}
