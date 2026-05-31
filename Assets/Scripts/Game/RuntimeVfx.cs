using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RuntimeVfx
{
    public static void Burst(Vector3 position, Color color, int count, float lifetime)
    {
        GameObject fx = new GameObject("Burst FX");
        fx.transform.position = position;
        ParticleSystem particles = fx.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startColor = color;
        main.startLifetime = lifetime;
        main.startSpeed = 2.7f;
        main.startSize = 0.12f;
        main.maxParticles = count;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        particles.Play();
        Object.Destroy(fx, lifetime + 0.5f);
    }

    public static void Slash(Vector3 position, Vector3 direction, Color color)
    {
        GameObject slash = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slash.name = "Slash FX";
        slash.transform.position = position;
        slash.transform.rotation = Quaternion.LookRotation(direction == Vector3.zero ? Vector3.forward : direction);
        slash.transform.localScale = new Vector3(1f, 0.06f, 0.36f);
        slash.GetComponent<Renderer>().material = RuntimeMaterials.Get("slash_" + color, color);
        Object.Destroy(slash.GetComponent<Collider>());
        Object.Destroy(slash, 0.12f);
    }

    public static void FloatingText(Vector3 position, string value, Color color)
    {
        GameObject textObject = new GameObject("Floating Text FX");
        textObject.transform.position = position;
        FloatingTextFx floatingText = textObject.AddComponent<FloatingTextFx>();
        floatingText.Initialize(value, color);
    }
}

public class FloatingTextFx : MonoBehaviour
{
    private TextMesh textMesh;
    private Color baseColor;
    private float createdAt;
    private float lifetime = 0.72f;
    private Vector3 drift;

    public void Initialize(string value, Color color)
    {
        baseColor = color;
        createdAt = Time.time;
        drift = new Vector3(0f, 0.85f, 0f);

        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.text = value;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.fontSize = 64;
        textMesh.characterSize = 0.055f;
        textMesh.color = color;

        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");
            renderer.material = new Material(shader);
            renderer.material.color = color;
        }
    }

    private void Update()
    {
        float age = Time.time - createdAt;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += drift * Time.deltaTime;
        transform.localScale = Vector3.one * Mathf.Lerp(1.15f, 0.85f, age / lifetime);

        Camera camera = Camera.main;
        if (camera != null)
        {
            transform.rotation = camera.transform.rotation;
        }

        Color color = baseColor;
        color.a = Mathf.Lerp(1f, 0f, age / lifetime);
        if (textMesh != null)
        {
            textMesh.color = color;
        }
    }
}

