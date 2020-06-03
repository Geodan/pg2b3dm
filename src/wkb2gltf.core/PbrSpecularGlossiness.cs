namespace Wkb2Gltf
{
    public class PbrSpecularGlossiness
    {
        public string DiffuseColor { get; set; }
        public string SpecularGlossiness { get; set; }

        public override bool Equals(object other)
        {
            var otherObject = (PbrSpecularGlossiness)other;
            if(DiffuseColor==otherObject.DiffuseColor && SpecularGlossiness == otherObject.SpecularGlossiness) {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
