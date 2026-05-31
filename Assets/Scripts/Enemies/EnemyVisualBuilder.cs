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

    // Boss = esqueleto rey con corona, escudo y espada (estilo del jefe final del Looty
    // Dungeon original). Todo procedural con primitivas, igual que el player y el resto de
    // enemigos. Mira hacia +Z (hacia la sala).
    private static void BuildBoss(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.025f, 1.9f);

        Color bone     = new Color(0.93f, 0.91f, 0.81f);
        Color boneDark = new Color(0.78f, 0.75f, 0.64f);
        Color gold     = new Color(0.97f, 0.74f, 0.12f);
        Color eye      = new Color(0.98f, 0.30f, 0.08f);   // cuencas brillantes
        Color shieldFace = new Color(0.30f, 0.34f, 0.46f);
        Color metal    = new Color(0.82f, 0.85f, 0.92f);
        Color wood     = new Color(0.42f, 0.27f, 0.13f);

        // --- Piernas y cadera ---
        CreatePart(visual, PrimitiveType.Cube, "Boss Leg L", new Vector3(-0.26f, 0.36f, 0f), new Vector3(0.26f, 0.72f, 0.26f), bone);
        CreatePart(visual, PrimitiveType.Cube, "Boss Leg R", new Vector3( 0.26f, 0.36f, 0f), new Vector3(0.26f, 0.72f, 0.26f), bone);
        CreatePart(visual, PrimitiveType.Cube, "Boss Pelvis", new Vector3(0f, 0.80f, 0f), new Vector3(0.72f, 0.28f, 0.44f), boneDark);

        // --- Torso / caja toracica ---
        CreatePart(visual, PrimitiveType.Capsule, "Boss Ribcage", new Vector3(0f, 1.22f, 0f), new Vector3(0.80f, 0.58f, 0.62f), bone);
        CreatePart(visual, PrimitiveType.Cube, "Boss Rib 1", new Vector3(0f, 1.06f, 0.30f), new Vector3(0.62f, 0.05f, 0.06f), boneDark);
        CreatePart(visual, PrimitiveType.Cube, "Boss Rib 2", new Vector3(0f, 1.22f, 0.31f), new Vector3(0.66f, 0.05f, 0.06f), boneDark);
        CreatePart(visual, PrimitiveType.Cube, "Boss Rib 3", new Vector3(0f, 1.38f, 0.30f), new Vector3(0.60f, 0.05f, 0.06f), boneDark);
        CreatePart(visual, PrimitiveType.Cube, "Boss Spine", new Vector3(0f, 1.22f, 0.0f), new Vector3(0.10f, 0.6f, 0.10f), boneDark);

        // --- Hombros y cuello ---
        CreatePart(visual, PrimitiveType.Cube, "Boss Shoulder L", new Vector3(-0.56f, 1.48f, 0f), new Vector3(0.32f, 0.22f, 0.42f), boneDark);
        CreatePart(visual, PrimitiveType.Cube, "Boss Shoulder R", new Vector3( 0.56f, 1.48f, 0f), new Vector3(0.32f, 0.22f, 0.42f), boneDark);
        CreatePart(visual, PrimitiveType.Cube, "Boss Neck", new Vector3(0f, 1.66f, 0f), new Vector3(0.18f, 0.18f, 0.18f), bone);

        // --- Craneo ---
        CreatePart(visual, PrimitiveType.Sphere, "Boss Skull", new Vector3(0f, 1.90f, 0.02f), new Vector3(0.52f, 0.56f, 0.54f), bone);
        CreatePart(visual, PrimitiveType.Cube, "Boss Jaw", new Vector3(0f, 1.70f, 0.16f), new Vector3(0.34f, 0.13f, 0.30f), boneDark);
        CreatePart(visual, PrimitiveType.Sphere, "Boss Eye L", new Vector3(-0.14f, 1.94f, 0.34f), Vector3.one * 0.13f, eye);
        CreatePart(visual, PrimitiveType.Sphere, "Boss Eye R", new Vector3( 0.14f, 1.94f, 0.34f), Vector3.one * 0.13f, eye);
        CreatePart(visual, PrimitiveType.Cube, "Boss Nose", new Vector3(0f, 1.84f, 0.40f), new Vector3(0.07f, 0.10f, 0.06f), boneDark);

        // --- Corona dorada (aro + picos) ---
        CreatePart(visual, PrimitiveType.Cube, "Boss Crown Band", new Vector3(0f, 2.22f, 0f), new Vector3(0.60f, 0.18f, 0.60f), gold);
        CreatePart(visual, PrimitiveType.Cube, "Boss Crown Spike C",  new Vector3(0f,    2.42f, 0.26f), new Vector3(0.12f, 0.24f, 0.10f), gold);
        CreatePart(visual, PrimitiveType.Cube, "Boss Crown Spike L",  new Vector3(-0.26f, 2.40f, 0.0f),  new Vector3(0.11f, 0.20f, 0.11f), gold);
        CreatePart(visual, PrimitiveType.Cube, "Boss Crown Spike R",  new Vector3( 0.26f, 2.40f, 0.0f),  new Vector3(0.11f, 0.20f, 0.11f), gold);
        CreatePart(visual, PrimitiveType.Cube, "Boss Crown Spike B",  new Vector3(0f,    2.40f, -0.26f), new Vector3(0.11f, 0.20f, 0.11f), gold);

        // --- Brazo izquierdo + ESCUDO ---
        CreatePart(visual, PrimitiveType.Cube, "Boss Arm L", new Vector3(-0.62f, 1.18f, 0.12f), new Vector3(0.18f, 0.55f, 0.18f), bone);
        CreatePart(visual, PrimitiveType.Cube, "Boss Shield", new Vector3(-0.80f, 1.02f, 0.34f), new Vector3(0.12f, 0.74f, 0.62f), shieldFace);
        CreatePart(visual, PrimitiveType.Cube, "Boss Shield Trim", new Vector3(-0.78f, 1.02f, 0.34f), new Vector3(0.14f, 0.82f, 0.10f), metal);   // barra vertical (emblema)
        CreatePart(visual, PrimitiveType.Cube, "Boss Shield Stud", new Vector3(-0.74f, 1.02f, 0.34f), new Vector3(0.10f, 0.18f, 0.18f), gold);    // tachon central

        // --- Brazo derecho + ESPADA (alzada) ---
        CreatePart(visual, PrimitiveType.Cube, "Boss Arm R", new Vector3(0.62f, 1.20f, 0.14f), new Vector3(0.18f, 0.55f, 0.18f), bone);
        CreatePart(visual, PrimitiveType.Cube, "Boss Sword Grip",  new Vector3(0.76f, 1.42f, 0.38f), new Vector3(0.09f, 0.24f, 0.09f), wood);
        CreatePart(visual, PrimitiveType.Cube, "Boss Sword Guard", new Vector3(0.76f, 1.58f, 0.38f), new Vector3(0.36f, 0.09f, 0.12f), gold);
        CreatePart(visual, PrimitiveType.Cube, "Boss Sword Blade", new Vector3(0.76f, 2.06f, 0.38f), new Vector3(0.13f, 0.86f, 0.06f), metal);
        CreatePart(visual, PrimitiveType.Cube, "Boss Sword Tip",   new Vector3(0.76f, 2.52f, 0.38f), new Vector3(0.13f, 0.12f, 0.06f), metal);
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
