using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SharpGLTF.Materials;
using System.Linq;

namespace Wkb2Gltf
{

    public class MaterialAndColor
    {
        public string Color { get; set; }
        public MaterialBuilder MaterialBuilder { get; set; }

    }

    public class MaterialCache
    {
        private List<MaterialAndColor> materials;
        public MaterialCache(List<string> colors)
        {
            materials = new List<MaterialAndColor>();
            foreach(var c in colors) {
                var color = ColorTranslator.FromHtml(c);
                var mat = CreateMaterial(color.R, color.G, color.B);
                var matandcolor = new MaterialAndColor() { Color = c, MaterialBuilder = mat };
                materials.Add(matandcolor);
            }
        }

        public MaterialBuilder GetMaterialBuilderByColor(string color)
        {
            var res = (from m in materials where m.Color == color select m).FirstOrDefault();
            return res.MaterialBuilder;
        }


        public static MaterialBuilder CreateMaterial(float r, float g, float b)
        {
            var material = new MaterialBuilder().
                WithDoubleSide(true).
                WithMetallicRoughnessShader().
                WithChannelParam(KnownChannels.BaseColor, ColorToVector4(r, g, b));
            return material;
        }

        public static Vector4 ColorToVector4(float r, float g, float b)
        {
            return new Vector4(r / 255, g / 255, b / 255, 1);
        }


    }
}
