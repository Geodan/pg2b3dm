using NUnit.Framework;

namespace Wkb2Gltf.Tests;

public class ShaderTests
{
    [Test]
    public void TestShaderOpaque()
    {
        var pbr = new PbrMetallicRoughness() {
            BaseColor = "#ff0000ff"
        };

        Assert.That(pbr.IsBaseColorOpaque(), Is.True);

        pbr.BaseColor = "#ff0000";
        Assert.That(pbr.IsBaseColorOpaque(), Is.True);

        pbr.BaseColor = "#ff000053";
        Assert.That(pbr.IsBaseColorOpaque(), Is.False);



    }


    [Test]

    public void ShaderEqualsTest()
    {
        var sg = new PbrSpecularGlossiness();
        sg.DiffuseColor = "fdfd";

        var shaderWithSpecularGlossiness = new Shader();
        shaderWithSpecularGlossiness.PbrSpecularGlossiness = sg;
        shaderWithSpecularGlossiness.EmissiveColor = "#bb3333";

        var otherShader = new Shader();
        otherShader.EmissiveColor = "#bb3333";
        otherShader.PbrSpecularGlossiness = sg;

        var res = shaderWithSpecularGlossiness.Equals(otherShader);
        Assert.That(res, Is.True);
    }
}

