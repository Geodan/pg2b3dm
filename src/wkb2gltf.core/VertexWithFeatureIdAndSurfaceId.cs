using System;
using System.Collections.Generic;
using System.Numerics;

using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;

namespace SharpGLTF.Geometry;

// Used when the --surfaces option is active: carries two independent glTF EXT_mesh_features
// feature-id sets per vertex - _FEATURE_ID_0 (existing per-feature/pand batch id) and
// _FEATURE_ID_1 (new per-polygon surface type id, e.g. 0=Ground, 1=Roof, 2=OuterWall, 3=InnerWall).
[System.Diagnostics.DebuggerDisplay("𝐅𝐈𝐃𝟎:{FeatureId} 𝐅𝐈𝐃𝟏:{SurfaceId}")]
public struct VertexWithFeatureIdAndSurfaceId : IVertexCustom
{
    public VertexWithFeatureIdAndSurfaceId(float featureId, float surfaceId)
    {
        FeatureId = featureId;
        SurfaceId = surfaceId;
    }

    public const string FEATUREID_ATTRIBUTENAME = "_FEATURE_ID_0";
    public const string SURFACEID_ATTRIBUTENAME = "_FEATURE_ID_1";

    public float FeatureId;

    public float SurfaceId;

    IEnumerable<KeyValuePair<string, AttributeFormat>> IVertexReflection.GetEncodingAttributes()
    {
        yield return new KeyValuePair<string, AttributeFormat>(FEATUREID_ATTRIBUTENAME, new AttributeFormat(DimensionType.SCALAR));
        yield return new KeyValuePair<string, AttributeFormat>(SURFACEID_ATTRIBUTENAME, new AttributeFormat(DimensionType.SCALAR));
    }

    public int MaxColors => 0;

    public int MaxTextCoords => 0;

    public IEnumerable<string> CustomAttributes
    {
        get
        {
            yield return FEATUREID_ATTRIBUTENAME;
            yield return SURFACEID_ATTRIBUTENAME;
        }
    }

    public void SetColor(int setIndex, Vector4 color) { }

    public void SetTexCoord(int setIndex, Vector2 coord) { }

    public Vector4 GetColor(int index) { throw new ArgumentOutOfRangeException(nameof(index)); }

    public Vector2 GetTexCoord(int index) { throw new ArgumentOutOfRangeException(nameof(index)); }

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
