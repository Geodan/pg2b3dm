using System;
using System.Numerics;

using SharpGLTF.Geometry.VertexTypes;

namespace SharpGLTF.Geometry
{
    [System.Diagnostics.DebuggerDisplay("𝐂:{Color} 𝐔𝐕:{TexCoord}")]
    public struct VertexWithBatchId : IVertexMaterial
    {
        public VertexWithBatchId(Vector4 color, Vector2 tex, Single batchId)
        {
            Color = color;
            TexCoord = tex;
            BatchId = batchId;
        }

        public static implicit operator VertexWithBatchId((Vector4 color, Vector2 tex, Single batchId) tuple)
        {
            return new VertexWithBatchId(tuple.color, tuple.tex, tuple.batchId);
        }

        public const string CUSTOMATTRIBUTENAME = "_BATCHID";

        [VertexAttribute(CUSTOMATTRIBUTENAME, Schema2.EncodingType.FLOAT, false)]
        public Single BatchId;

        [VertexAttribute("COLOR_0", Schema2.EncodingType.UNSIGNED_BYTE, true)]
        public Vector4 Color;

        [VertexAttribute("TEXCOORD_0")]
        public Vector2 TexCoord;

        public int MaxColors => 1;

        public int MaxTextCoords => 1;

        void IVertexMaterial.SetColor(int setIndex, Vector4 color) { if (setIndex == 0) this.Color = color; }

        void IVertexMaterial.SetTexCoord(int setIndex, Vector2 coord) { if (setIndex == 0) this.TexCoord = coord; }

        public Vector4 GetColor(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return Color;
        }

        public Vector2 GetTexCoord(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return TexCoord;
        }

        public void Validate() { FragmentPreprocessors.ValidateVertexMaterial(this); }

        public object GetCustomAttribute(string attributeName)
        {
            return attributeName == CUSTOMATTRIBUTENAME ? (Object)BatchId : null;
        }
    }
}
