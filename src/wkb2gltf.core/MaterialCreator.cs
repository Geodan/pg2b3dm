﻿using System;
using System.Drawing;
using System.Numerics;
using SharpGLTF.Materials;

namespace Wkb2Gltf
{
    public class MaterialCreator
    {
        public static MaterialBuilder GetDefaultMaterial(string color)
        {
            var rgb = ColorTranslator.FromHtml(color);

            var material = new MaterialBuilder().
            WithDoubleSide(true).
            WithMetallicRoughnessShader().
            WithAlpha(AlphaMode.BLEND).
            WithChannelParam(KnownChannel.BaseColor, ColorToVector4(rgb));
            return material;
        }

        public static MaterialBuilder CreateMaterial(Shader shader)
        {
            var material = new MaterialBuilder().
                WithDoubleSide(true).
                WithAlpha(AlphaMode.OPAQUE);

            if (shader.EmissiveColor != null) {
                material.WithEmissive(ColorToVector3(ColorTranslator.FromHtml(shader.EmissiveColor)));
            }
            if (shader.PbrSpecularGlossiness != null) {
                material.WithSpecularGlossinessShader();

                if (shader.PbrSpecularGlossiness.DiffuseColor != null) {
                    material.WithChannelParam(KnownChannel.Diffuse, ColorToVector4(ColorTranslator.FromHtml(shader.PbrSpecularGlossiness.DiffuseColor)));
                }
                if (shader.PbrSpecularGlossiness.SpecularGlossiness != null) {
                    material.WithChannelParam(KnownChannel.SpecularGlossiness, ColorToVector4(ColorTranslator.FromHtml(shader.PbrSpecularGlossiness.SpecularGlossiness)));
                }
            }
            else if (shader.PbrMetallicRoughness != null) {
                material.WithMetallicRoughnessShader();
                if (shader.PbrMetallicRoughness.BaseColor != null) {
                    material.WithChannelParam(KnownChannel.BaseColor, ColorToVector4(ColorTranslator.FromHtml(shader.PbrMetallicRoughness.BaseColor)));
                }
                if (shader.PbrMetallicRoughness.MetallicRoughness != null) {
                    material.WithChannelParam(KnownChannel.MetallicRoughness, ColorToVector4(ColorTranslator.FromHtml(shader.PbrMetallicRoughness.MetallicRoughness)));
                }
            }

            // todo: implement 'fallback' method (when both PbrSpecularGlossiness and PbrMetallicRoughness are implemented

            return material;
        }

        private static Vector4 ColorToVector4(Color c)
        {
            var v = new Vector4((float)c.R / 255, (float)c.G / 255, (float)c.B / 255, (float)c.A / 255);
            return v;
        }
        private static Vector3 ColorToVector3(Color c)
        {
            var v = new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
            return v;
        }

/*         private static Vector3 CreateMaterialColor3(string color)
        {
            var c = ColorTranslator.FromHtml(color);
            var v = new Vector3(SRGBToLinear((float)c.R / 255), SRGBToLinear((float)c.G / 255), SRGBToLinear((float)c.B / 255));
            return v;
        }

        private static Vector4 CreateMaterialColor4(string color)
        {
            var c = ColorTranslator.FromHtml(color);
            var v = new Vector4(SRGBToLinear((float)c.R / 255), SRGBToLinear((float)c.G / 255), SRGBToLinear((float)c.B / 255), (float)c.A / 255);
            return v;
        }

        private static float SRGBToLinear(float c)
        {
            return (c < 0.04045) ? (float)(c * 0.0773993808) : (float)(Math.Pow(c * 0.9478672986 + 0.0521327014, 2.4));
        } */
    }
}
