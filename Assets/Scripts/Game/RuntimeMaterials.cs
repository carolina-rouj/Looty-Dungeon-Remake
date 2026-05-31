using System.Collections.Generic;
using UnityEngine;

public static class RuntimeMaterials
{
    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public static Material Get(string key, Color color)
    {
        if (Materials.TryGetValue(key, out Material material))
        {
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        Materials[key] = material;
        return material;
    }

    public static Material GetEmissive(string key, Color color, float intensity)
    {
        Material material = Get(key, color);
        Color emission = color * Mathf.Max(0f, intensity);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }
        return material;
    }
}

