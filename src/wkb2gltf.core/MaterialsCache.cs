using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpGLTF.Materials;

namespace Wkb2Gltf
{
    public class MaterialsCache
    {
        private readonly List<MaterialAndColor> materials;
        public MaterialsCache()
        {
            materials = new List<MaterialAndColor>();
        }

        public MaterialBuilder GetMaterialBuilderByColor(string color)
        {
            var res = (from m in materials where m.Color == color select m).FirstOrDefault();
            if (res == null) {
                // create and add it to List
                var rgb = ColorTranslator.FromHtml(color);
                var materialBuilder = MaterialCreator.CreateMaterial(rgb);

                res = new MaterialAndColor{ Color = color, MaterialBuilder = materialBuilder };
                materials.Add(res);

            }
            return res.MaterialBuilder;
        }
    }
}