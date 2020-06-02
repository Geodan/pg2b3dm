using System;
using System.Collections.Generic;

namespace Wkb2Gltf
{
    public class Shader
    {
        public List<string> EmissiveColors { get; set; }
        public PbrSpecularGlossiness PbrSpecularGlossiness { get; set; }
        public PbrSpecularGlossiness PbrMetallicRoughness { get; set; }
    }

    public class PbrSpecularGlossiness
    {
        public List<string> DiffuseColors { get; set; }
        public List<string> SpecularGlossiness { get; set; }
    }

    public class PbrMetallicRoughness
    {
        public List<string> MetallicRoughness { get; set; }
        public List<string> BaseColors { get; set; }
    }

}
