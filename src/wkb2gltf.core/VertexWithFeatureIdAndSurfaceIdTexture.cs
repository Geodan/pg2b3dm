using System;
using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;

namespace SharpGLTF.Geometry;

// Texture-pipeline variant of VertexWithFeatureIdAndSurfaceId: adds TEXCOORD_0 alongside the
// two feature-id attributes (_FEATURE_ID_0 = pand/feature id, _FEATURE_ID_1 = surface type id).
[System.Diagnostics.DebuggerDisplay("𝐅𝐈𝐃𝟎:{FeatureId} 𝐅𝐈𝐃𝟏:{SurfaceId} 𝐔𝐕:{TexCoord}")]
public struct VertexWithFeatureIdAndSurfaceIdTexture : IVertexCustom
{
    public VertexWithFeatureIdAndSurfaceIdTexture(float featureId, float surfaceId, Vector2 texCoord)
    {
        FeatureId = featureId;
        SurfaceId = surfaceId;
        TexCoord = texCoord;
    }

    public const string FEATUREID_ATTRIBUTENAME = "_FEATURE_ID_0";
    public const string SURFACEID_ATTRIBUTENAME = "_FEATURE_ID_1";

    public float FeatureId;

    public float SurfaceId;

    public Vector2 TexCoord;

    IEnumerable<KeyValuePair<string, AttributeFormat>> IVertexReflection.GetEncodingAttributes()
    {
        yield return new KeyValuePair<string, AttributeFormat>(FEATUREID_ATTRIBUTENAME, new AttributeFormat(DimensionType.SCALAR));
        yield return new KeyValuePair<string, AttributeFormat>(SURFACEID_ATTRIBUTENAME, new AttributeFormat(DimensionType.SCALAR));
        yield return new KeyValuePair<string, AttributeFormat>("TEXCOORD_0", new AttributeFormat(DimensionType.VEC2));
    }

    public int MaxColors => 0;

    public int MaxTextCoords => 1;

    public IEnumerable<string> CustomAttributes
    {
        get
        {
            yield return FEATUREID_ATTRIBUTENAME;
            yield return SURFACEID_ATTRIBUTENAME;
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
        if (attributeName == FEATUREID_ATTRIBUTENAME) { return (object)FeatureId; }
        if (attributeName == SURFACEID_ATTRIBUTENAME) { return (object)SurfaceId; }
        return null;
    }

    public bool TryGetCustomAttribute(string attribute, out object value)
    {
        if (attribute == FEATUREID_ATTRIBUTENAME) { value = FeatureId; return true; }
        if (attribute == SURFACEID_ATTRIBUTENAME) { value = SurfaceId; return true; }
        value = null;
        return false;
    }

    public void SetCustomAttribute(string attributeName, object value)
    {
        if (attributeName == FEATUREID_ATTRIBUTENAME) {
            FeatureId = Convert.ToSingle(value);
        }
        else if (attributeName == SURFACEID_ATTRIBUTENAME) {
            SurfaceId = Convert.ToSingle(value);
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
