using System;
using glTFLoader.Schema;

namespace Wkb2Gltf
{
    public static class MaterialMaker
    {
        public static Material CreateMaterial(string name, float red, float greeen, float blue, float alpha)
        {
            var m = new Material();
            m.Name = name;
            m.PbrMetallicRoughness = new MaterialPbrMetallicRoughness() {
                BaseColorFactor = new Single[] { red, greeen, blue, alpha },
                MetallicFactor = 0,
                RoughnessFactor = 1
            };
            m.EmissiveFactor = new Single[] { 0, 0, 0 };
            m.AlphaMode = (alpha < 1.0f)
                ? Material.AlphaModeEnum.BLEND
                : Material.AlphaModeEnum.OPAQUE;
            m.AlphaCutoff = 0.5f;
            m.DoubleSided = false;
            return m;
        }
    }
}
