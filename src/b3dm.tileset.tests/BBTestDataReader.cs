using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace B3dm.Tileset.Tests
{
    public static class BBTestDataReader
    {
        public static List<BoundingBox3D> GetTestData(string file)
        {
            var fileStream = new FileStream(file, FileMode.Open);
            var bboxes = new List<BoundingBox3D>();
            var reader = new StreamReader(fileStream);
            string line;
            while ((line = reader.ReadLine()) != null) {
                var splitted = line.Split(",");
                var bbox = new BoundingBox3D();
                bbox.XMin = Double.Parse(splitted[0], CultureInfo.InvariantCulture);
                bbox.YMin = Double.Parse(splitted[1], CultureInfo.InvariantCulture);
                bbox.ZMin = Double.Parse(splitted[2], CultureInfo.InvariantCulture);
                bbox.XMax = Double.Parse(splitted[3], CultureInfo.InvariantCulture);
                bbox.YMax = Double.Parse(splitted[4], CultureInfo.InvariantCulture);
                bbox.ZMax = Double.Parse(splitted[5], CultureInfo.InvariantCulture);
                bboxes.Add(bbox);
            }
            fileStream.Close();
            return bboxes;
        }

    }
}
