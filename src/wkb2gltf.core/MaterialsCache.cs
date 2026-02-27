using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SharpGLTF.Materials;

namespace Wkb2Gltf;

public class MaterialsCache
{
    private readonly List<MaterialAndShader> materials;
    private readonly Dictionary<string, MaterialBuilder> texturedMaterials;
    public MaterialsCache()
    {
        materials = new List<MaterialAndShader>();
        texturedMaterials = new Dictionary<string, MaterialBuilder>();
    }

    public MaterialBuilder GetMaterialBuilderByShader(Shader shader, bool doubleSided = false, AlphaMode defaultAlphaMode = AlphaMode.OPAQUE, float alphaCutoff = 0.5f)
    {
        var res = (from m in materials where m.Shader.Equals(shader) select m).FirstOrDefault();
        if (res == null) {
            var materialBuilder = MaterialCreator.CreateMaterial(shader, doubleSided, defaultAlphaMode, alphaCutoff);

            res = new MaterialAndShader { Shader = shader, MaterialBuilder = materialBuilder };
            materials.Add(res);

        }
        return res.MaterialBuilder;
    }

    public MaterialBuilder GetMaterialBuilderByTexture(byte[] imageData, bool doubleSided = false, AlphaMode defaultAlphaMode = AlphaMode.OPAQUE, float alphaCutoff = 0.5f)
    {
        var key = Convert.ToHexString(SHA256.HashData(imageData));
        if (!texturedMaterials.TryGetValue(key, out var materialBuilder)) {
            materialBuilder = MaterialCreator.CreateTextureMaterial(imageData, doubleSided, defaultAlphaMode, alphaCutoff);
            texturedMaterials.Add(key, materialBuilder);
        }
        return materialBuilder;
    }
}
