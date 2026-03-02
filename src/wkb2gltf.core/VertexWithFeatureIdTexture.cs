using System;
using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;

namespace SharpGLTF.Geometry;

[System.Diagnostics.DebuggerDisplay("FeatureId:{BatchId} UV:{TexCoord}")]
public struct VertexWithFeatureIdTexture : IVertexCustom
{
    public VertexWithFeatureIdTexture(float batchId, Vector2 texCoord)
    {
        BatchId = batchId;
        TexCoord = texCoord;
    }

    public const string CUSTOMATTRIBUTENAME = "_FEATURE_ID_0";

    public float BatchId;

    public Vector2 TexCoord;

    IEnumerable<KeyValuePair<string, AttributeFormat>> IVertexReflection.GetEncodingAttributes()
    {
        yield return new KeyValuePair<string, AttributeFormat>(CUSTOMATTRIBUTENAME, new AttributeFormat(DimensionType.SCALAR));
        yield return new KeyValuePair<string, AttributeFormat>("TEXCOORD_0", new AttributeFormat(DimensionType.VEC2));
    }

    public int MaxColors => 0;

    public int MaxTextCoords => 1;

    public IEnumerable<string> CustomAttributes
    {
        get
        {
            yield return CUSTOMATTRIBUTENAME;
        }
    }

    public void SetColor(int setIndex, Vector4 color) { }

    public void SetTexCoord(int setIndex, Vector2 coord)
    {
        if (setIndex == 0) {
            TexCoord = coord;
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(setIndex));
    }

    public Vector4 GetColor(int index) { throw new ArgumentOutOfRangeException(nameof(index)); }

    public Vector2 GetTexCoord(int index)
    {
        if (index == 0) {
            return TexCoord;
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public void Validate() { }

    public object GetCustomAttribute(string attributeName)
    {
        return attributeName == CUSTOMATTRIBUTENAME ? (Object)BatchId : null;
    }

    public bool TryGetCustomAttribute(string attribute, out object value)
    {
        if (attribute != CUSTOMATTRIBUTENAME) { value = null; return false; }
        value = BatchId; return true;
    }

    public void SetCustomAttribute(string attributeName, object value)
    {
        if (attributeName == CUSTOMATTRIBUTENAME) {
            BatchId = Convert.ToSingle(value);
        }
        else {
            throw new ArgumentException($"Unknown attribute: {attributeName}");
        }
    }

    public VertexMaterialDelta Subtract(IVertexMaterial baseValue)
    {
        return default;
    }

    public void Add(in VertexMaterialDelta delta)
    {
    }
}
