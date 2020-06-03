namespace Wkb2Gltf
{
    public class PbrMetallicRoughness
    {
        public string MetallicRoughness { get; set; }

        public string BaseColor { get; set; }


        public override bool Equals(object other)
        {
            var otherObject = (PbrMetallicRoughness)other;
            if (MetallicRoughness == otherObject.MetallicRoughness && BaseColor == otherObject.BaseColor) {
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
