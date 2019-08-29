using System;
using System.Numerics;

using SharpGLTF.Geometry.VertexTypes;

namespace SharpGLTF.Geometry
{
    [System.Diagnostics.DebuggerDisplay("𝐂:{Color} 𝐔𝐕:{TexCoord}")]
    public struct VertexWithBatchId : IVertexMaterial
    {
        public VertexWithBatchId(float batchId) { BatchId = batchId; }

        public static implicit operator VertexWithBatchId(float batchId)
        {
            return new VertexWithBatchId(batchId);
        }

        public const string CUSTOMATTRIBUTENAME = "_BATCHID";

        [VertexAttribute(CUSTOMATTRIBUTENAME, Schema2.EncodingType.FLOAT, false)]
        public float BatchId;

        public int MaxColors => 0;

        public int MaxTextCoords => 0;

        public void SetColor(int setIndex, Vector4 color) { }

        public void SetTexCoord(int setIndex, Vector2 coord) { }

        public Vector4 GetColor(int index) { throw new ArgumentOutOfRangeException(nameof(index)); }

        public Vector2 GetTexCoord(int index) { throw new ArgumentOutOfRangeException(nameof(index)); }

        public void Validate() { }

        public object GetCustomAttribute(string attributeName)
        {
            return attributeName == CUSTOMATTRIBUTENAME ? (Object)BatchId : null;
        }
    }
}
