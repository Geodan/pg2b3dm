﻿using System;
using System.Collections.Generic;
using System.Numerics;

using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Schema2;

namespace SharpGLTF.Geometry;

[System.Diagnostics.DebuggerDisplay("𝐂:{Color} 𝐔𝐕:{TexCoord}")]
public struct VertexWithFeatureId : IVertexCustom
{
    public static implicit operator VertexWithFeatureId(float batchId)
    {
        return new VertexWithFeatureId(batchId);
    }

    public VertexWithFeatureId(float batchId)
    {
        BatchId = batchId;
    }

    public const string CUSTOMATTRIBUTENAME = "_FEATURE_ID_0";

    [VertexAttribute(CUSTOMATTRIBUTENAME, EncodingType.FLOAT, false)]
    public float BatchId;

    public int MaxColors => 0;

    public int MaxTextCoords => 0;

    public IEnumerable<string> CustomAttributes => throw new NotImplementedException();

    public void SetColor(int setIndex, Vector4 color) { }

    public void SetTexCoord(int setIndex, Vector2 coord) { }

    public Vector4 GetColor(int index) { throw new ArgumentOutOfRangeException(nameof(index)); }

    public Vector2 GetTexCoord(int index) { throw new ArgumentOutOfRangeException(nameof(index)); }

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
        throw new NotImplementedException();
    }

    public VertexMaterialDelta Subtract(IVertexMaterial baseValue)
    {
        throw new NotImplementedException();
    }

    public void Add(in VertexMaterialDelta delta)
    {
        throw new NotImplementedException();
    }
}
