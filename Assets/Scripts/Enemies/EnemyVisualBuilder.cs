using UnityEngine;

// Construye un visual procedural (primitivas) para los enemigos de Carolina, al estilo
// del PlayerVisualBuilder que a ella le gusto. Asi Bat/Wizard/Gnome/Boss tienen un look
// sin necesidad de modelar prefabs a mano. La logica de cada enemigo es la de Carolina.
public static class EnemyVisualBuilder
{
    public static void Build(string type, Transform root)
    {
        switch (type)
        {
            case "Bat":    BuildBat(root);    break;
            case "Wizard": BuildWizard(root); break;
            case "Gnome":  BuildGnome(root);  break;
            case "Boss":   BuildBoss(root);   break;
            case "Slime":  BuildSlime(root);  break;
            default:       BuildGnome(root);  break;
        }
    }

    private static Transform CreateVisualRoot(Transform root, float bobAmplitude, float bobSpeed)
    {
        Transform visualRoot = new GameObject("RuntimeEnemyVisual").transform;
        visualRoot.SetParent(root, false);
        RuntimeEnemyVisualAnimator animator = root.gameObject.AddComponent<RuntimeEnemyVisualAnimator>();
        animator.Initialize(visualRoot, bobAmplitude, bobSpeed);
        return visualRoot;
    }

    private static void BuildSlime(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.10f, 5f);
        CreatePart(visual, PrimitiveType.Sphere, "Slime Body", new Vector3(0f, 0.42f, 0f), new Vector3(0.85f, 0.65f, 0.85f), new Color(0.32f, 0.82f, 0.28f));
        CreatePart(visual, PrimitiveType.Sphere, "Slime Drop", new Vector3(0f, 0.78f, 0f), new Vector3(0.38f, 0.30f, 0.38f), new Color(0.50f, 0.94f, 0.40f));
        CreatePart(visual, PrimitiveType.Sphere, "Slime Eye L", new Vector3(-0.14f, 0.52f, 0.32f), Vector3.one * 0.10f, new Color(0.04f, 0.05f, 0.06f));
        CreatePart(visual, PrimitiveType.Sphere, "Slime Eye R", new Vector3(0.14f, 0.52f, 0.32f), Vector3.one * 0.10f, new Color(0.04f, 0.05f, 0.06f));
        CreatePart(visual, PrimitiveType.Cube, "Slime Mouth", new Vector3(0f, 0.34f, 0.36f), new Vector3(0.22f, 0.06f, 0.04f), new Color(0.10f, 0.20f, 0.10f));
    }

    private static void BuildBat(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.12f, 6f);
        CreatePart(visual, PrimitiveType.Capsule, "Bat Body", new Vector3(0f, 0.62f, 0f), new Vector3(0.46f, 0.38f, 0.46f), new Color(0.34f, 0.16f, 0.48f));
        CreatePart(visual, PrimitiveType.Cube, "Bat Wing L", new Vector3(-0.48f, 0.62f, 0f), new Vector3(0.7f, 0.08f, 0.28f), new Color(0.18f, 0.08f, 0.28f));
        CreatePart(visual, PrimitiveType.Cube, "Bat Wing R", new Vector3(0.48f, 0.62f, 0f), new Vector3(0.7f, 0.08f, 0.28f), new Color(0.18f, 0.08f, 0.28f));
        CreatePart(visual, PrimitiveType.Sphere, "Bat Eye L", new Vector3(-0.11f, 0.72f, 0.27f), Vector3.one * 0.09f, new Color(1f, 0.18f, 0.08f));
        CreatePart(visual, PrimitiveType.Sphere, "Bat Eye R", new Vector3(0.11f, 0.72f, 0.27f), Vector3.one * 0.09f, new Color(1f, 0.18f, 0.08f));
    }

    private static void BuildWizard(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.05f, 3.5f);
        CreatePart(visual, PrimitiveType.Capsule, "Wizard Robe", new Vector3(0f, 0.72f, 0f), new Vector3(0.62f, 0.82f, 0.62f), new Color(0.38f, 0.12f, 0.72f));
        CreatePart(visual, PrimitiveType.Cylinder, "Wizard Brim", new Vector3(0f, 1.24f, 0f), new Vector3(0.62f, 0.08f, 0.62f), new Color(0.13f, 0.07f, 0.22f));
        CreatePart(visual, PrimitiveType.Cylinder, "Wizard Hat", new Vector3(0f, 1.48f, 0f), new Vector3(0.28f, 0.36f, 0.28f), new Color(0.22f, 0.08f, 0.42f));
        CreatePart(visual, PrimitiveType.Cube, "Wizard Staff", new Vector3(0.42f, 0.72f, 0.08f), new Vector3(0.07f, 0.95f, 0.07f), new Color(0.48f, 0.24f, 0.08f));
        CreatePart(visual, PrimitiveType.Sphere, "Wizard Orb", new Vector3(0.42f, 1.26f, 0.08f), Vector3.one * 0.18f, new Color(0.28f, 0.88f, 1f));
    }

    private static void BuildGnome(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.04f, 4.2f);
        CreatePart(visual, PrimitiveType.Capsule, "Gnome Body", new Vector3(0f, 0.58f, 0f), new Vector3(0.55f, 0.58f, 0.55f), new Color(0.82f, 0.28f, 0.12f));
        CreatePart(visual, PrimitiveType.Sphere, "Gnome Nose", new Vector3(0f, 0.72f, 0.32f), Vector3.one * 0.14f, new Color(0.95f, 0.58f, 0.4f));
        CreatePart(visual, PrimitiveType.Cylinder, "Gnome Hat", new Vector3(0f, 1.02f, 0f), new Vector3(0.28f, 0.38f, 0.28f), new Color(0.05f, 0.34f, 0.2f));
        CreatePart(visual, PrimitiveType.Cube, "Gnome Axe", new Vector3(-0.42f, 0.62f, 0.08f), new Vector3(0.08f, 0.72f, 0.08f), new Color(0.5f, 0.27f, 0.1f));
        CreatePart(visual, PrimitiveType.Cube, "Gnome Axe Head", new Vector3(-0.42f, 0.98f, 0.08f), new Vector3(0.34f, 0.16f, 0.08f), new Color(0.82f, 0.78f, 0.68f));
    }

    private static void BuildBoss(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.03f, 2.4f);
        CreatePart(visual, PrimitiveType.Capsule, "Boss Body", new Vector3(0f, 1.02f, 0f), new Vector3(1.25f, 1.32f, 1.25f), new Color(0.12f, 0.06f, 0.06f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Crown", new Vector3(0f, 1.92f, 0f), new Vector3(0.78f, 0.18f, 0.78f), new Color(0.96f, 0.66f, 0.08f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Horn L", new Vector3(-0.48f, 1.92f, 0.05f), new Vector3(0.14f, 0.46f, 0.14f), new Color(0.9f, 0.86f, 0.72f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Horn R", new Vector3(0.48f, 1.92f, 0.05f), new Vector3(0.14f, 0.46f, 0.14f), new Color(0.9f, 0.86f, 0.72f));
        CreatePart(visual, PrimitiveType.Sphere, "Boss Eye L", new Vector3(-0.22f, 1.18f, 0.62f), Vector3.one * 0.16f, new Color(1f, 0.08f, 0.04f));
        CreatePart(visual, PrimitiveType.Sphere, "Boss Eye R", new Vector3(0.22f, 1.18f, 0.62f), Vector3.one * 0.16f, new Color(1f, 0.08f, 0.04f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Shoulder L", new Vector3(-0.8f, 1.1f, 0f), new Vector3(0.38f, 0.32f, 0.62f), new Color(0.34f, 0.08f, 0.07f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Shoulder R", new Vector3(0.8f, 1.1f, 0f), new Vector3(0.38f, 0.32f, 0.62f), new Color(0.34f, 0.08f, 0.07f));
    }

    private static void CreatePart(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = RuntimeMaterials.Get("enemy_visual_" + name, color);
        Object.Destroy(part.GetComponent<Collider>());
    }
}

// Pequeno balanceo del visual del enemigo para darle vida (no toca la logica de movimiento).
public class RuntimeEnemyVisualAnimator : MonoBehaviour
{
    private Transform visualRoot;
    private float amplitude;
    private float speed;
    private float seed;

    public void Initialize(Transform root, float bobAmplitude, float bobSpeed)
    {
        visualRoot = root;
        amplitude = bobAmplitude;
        speed = bobSpeed;
        seed = Random.value * 10f;
    }

    private void Update()
    {
        if (visualRoot == null || !EnemyMovementUtility.IsGameplayActive())
        {
            return;
        }

        float wave = Mathf.Sin(Time.time * speed + seed);
        visualRoot.localPosition = Vector3.up * (wave * amplitude);
        visualRoot.localRotation = Quaternion.Euler(0f, 0f, wave * 2.5f);
    }
}
