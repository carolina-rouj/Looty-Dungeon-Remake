using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RuntimeSceneLighting
{
    public static void Ensure()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.34f, 0.24f, 0.19f);
        RenderSettings.ambientEquatorColor = new Color(0.16f, 0.09f, 0.08f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.04f, 0.04f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.16f, 0.07f, 0.07f);
        RenderSettings.fogDensity = 0.022f;

        EnsureDirectional("Runtime Key Light", Quaternion.Euler(48f, -34f, 0f), new Color(1f, 0.82f, 0.62f), 1.25f);
        EnsureDirectional("Runtime Rim Light", Quaternion.Euler(34f, 136f, 0f), new Color(0.42f, 0.58f, 1f), 0.45f);
    }

    public static void TintMainLights(Color keyColor, float keyIntensity, Color rimColor, float rimIntensity)
    {
        TintDirectional("Runtime Key Light", keyColor, keyIntensity);
        TintDirectional("Runtime Rim Light", rimColor, rimIntensity);
    }

    private static void TintDirectional(string name, Color color, float intensity)
    {
        GameObject lightObject = GameObject.Find(name);
        if (lightObject == null)
        {
            return;
        }

        Light light = lightObject.GetComponent<Light>();
        if (light == null)
        {
            return;
        }

        light.color = color;
        light.intensity = intensity;
    }

    private static void EnsureDirectional(string name, Quaternion rotation, Color color, float intensity)
    {
        GameObject lightObject = GameObject.Find(name);
        if (lightObject == null)
        {
            lightObject = new GameObject(name);
        }

        Light light = lightObject.GetComponent<Light>();
        if (light == null)
        {
            light = lightObject.AddComponent<Light>();
        }

        light.type = LightType.Directional;
        light.color = color;
        light.intensity = intensity;
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = rotation;
    }
}
