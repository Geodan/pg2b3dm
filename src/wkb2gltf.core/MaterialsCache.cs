using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpGLTF.Materials;

namespace Wkb2Gltf
{
    public class MaterialsCache
    {
        private readonly List<MaterialAndShader> materials;
        public MaterialsCache()
        {
            materials = new List<MaterialAndShader>();
        }

        public MaterialBuilder GetMaterialBuilderByShader(Shader shader)
        {
            var res = (from m in materials where m.Shader.Equals(shader) select m).FirstOrDefault();
            if (res == null) {
                var materialBuilder = MaterialCreator.CreateMaterial(shader);

                res = new MaterialAndShader{ Shader = shader, MaterialBuilder = materialBuilder };
                materials.Add(res);

            }
            return res.MaterialBuilder;
        }
    }
}