using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerVisualBuilder
{
    public static void Build(Transform root)
    {
        if (root.Find("RuntimePlayerVisual") != null)
        {
            return;
        }

        Transform visual = new GameObject("RuntimePlayerVisual").transform;
        visual.SetParent(root, false);

        CreatePart(visual, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.66f, 0f), new Vector3(0.55f, 0.7f, 0.55f), new Color(0.22f, 0.55f, 0.92f));
        CreatePart(visual, PrimitiveType.Cube, "Helmet", new Vector3(0f, 1.32f, 0f), new Vector3(0.48f, 0.25f, 0.48f), new Color(0.86f, 0.84f, 0.72f));
        CreatePart(visual, PrimitiveType.Cube, "Cape", new Vector3(0f, 0.68f, -0.32f), new Vector3(0.48f, 0.7f, 0.08f), new Color(0.86f, 0.08f, 0.12f));
        CreatePart(visual, PrimitiveType.Cube, "Sword", new Vector3(0.48f, 0.78f, 0.22f), new Vector3(0.08f, 0.75f, 0.08f), new Color(0.88f, 0.86f, 0.74f));
    }

    private static void CreatePart(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = RuntimeMaterials.Get("player_" + name, color);
        Object.Destroy(part.GetComponent<Collider>());
    }
}

