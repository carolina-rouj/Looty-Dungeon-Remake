using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EntityCapture
{
    private const int Size = 360;
    private static int frames;
    private static readonly string OutputDirectory =
        Path.Combine(Directory.GetCurrentDirectory(), "Memoria", "captures");

    private static readonly (string type, string file)[] Characters =
    {
        ("__player", "entity_player"),
        ("Slime", "entity_slime"),
        ("Gnome", "entity_gnome"),
        ("Wizard", "entity_wizard"),
        ("Boss", "entity_boss"),
    };

    private static readonly (string prefab, string file)[] Prefabs =
    {
        ("Assets/Prefabs/Barrel.prefab", "entity_barrel"),
        ("Assets/Prefabs/BookshelfA.prefab", "entity_bookshelf"),
        ("Assets/Prefabs/Cauldron.prefab", "entity_cauldron"),
        ("Assets/Prefabs/caliz.prefab", "entity_caliz"),
        ("Assets/Prefabs/throne.prefab", "entity_throne"),
        ("Assets/Prefabs/floorTorch.prefab", "entity_torch"),
        ("Assets/Prefabs/CarpetRectangleRed.prefab", "entity_carpet"),
        ("Assets/Prefabs/arrowShoot.prefab", "entity_arrowshoot"),
        ("Assets/Prefabs/diana.prefab", "entity_diana"),
        ("Assets/Prefabs/fork.prefab", "entity_fork"),
        ("Assets/Prefabs/tela1.prefab", "entity_spiderwebs"),
        ("Assets/Prefabs/Arrow.prefab", "entity_arrow"),
    };

    public static void Run()
    {
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        frames = 0;
        EditorApplication.playModeStateChanged += OnState;
        EditorApplication.EnterPlaymode();
    }

    private static void OnState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.update += Tick;
        }
    }

    private static void Tick()
    {
        frames++;
        if (frames < 4) return;
        EditorApplication.update -= Tick;
        try { CaptureAll(); }
        finally { EditorApplication.Exit(0); }
    }

    private static void CaptureAll()
    {
        Directory.CreateDirectory(OutputDirectory);
        Vector3 origin = new Vector3(500f, 0f, 500f);

        Camera camera = new GameObject("EntityCam").AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camera.transform.rotation = Quaternion.Euler(28f, 45f, 0f);

        Light key = new GameObject("Key").AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 1.25f;
        key.transform.rotation = Quaternion.Euler(45f, 35f, 0f);

        Light fill = new GameObject("Fill").AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.55f;
        fill.transform.rotation = Quaternion.Euler(20f, 215f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.66f);

        foreach (var c in Characters)
        {
            GameObject go = new GameObject("E");
            go.transform.position = origin;
            if (c.type == "__player") PlayerVisualBuilder.Build(go.transform);
            else EnemyVisualBuilder.Build(c.type, go.transform);
            CaptureObject(camera, go, c.file);
            Object.DestroyImmediate(go);
        }

        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.transform.position = origin;
        coin.transform.localScale = new Vector3(0.6f, 0.12f, 0.6f);
        coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        coin.GetComponent<Renderer>().material = RuntimeMaterials.Get("coin_capture", new Color(1f, 0.78f, 0.08f));
        CaptureObject(camera, coin, "entity_coin");
        Object.DestroyImmediate(coin);

        foreach (var p in Prefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p.prefab);
            if (prefab == null) { Debug.LogWarning("[EntityCapture] No prefab: " + p.prefab); continue; }
            GameObject go = Object.Instantiate(prefab);
            go.transform.position = origin;
            go.transform.rotation = Quaternion.identity;
            CaptureObject(camera, go, p.file);
            Object.DestroyImmediate(go);
        }

        Debug.Log("[EntityCapture] OK");
    }

    private static void CaptureObject(Camera camera, GameObject go, string file)
    {
        Bounds b = ComputeBounds(go);
        float radius = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
        if (radius < 0.05f) radius = 0.5f;
        camera.orthographicSize = radius * 1.45f + 0.15f;
        Vector3 dir = camera.transform.rotation * Vector3.forward;
        camera.transform.position = b.center - dir * 30f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 200f;

        RenderTexture rt = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32);
        RenderTexture prevActive = RenderTexture.active;
        camera.targetTexture = rt;
        camera.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(Size, Size, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(OutputDirectory, file + ".png"), tex.EncodeToPNG());
        Debug.Log("[EntityCapture] Wrote " + file + ".png");

        camera.targetTexture = null;
        RenderTexture.active = prevActive;
        Object.Destroy(tex);
        rt.Release();
        Object.Destroy(rt);
    }

    private static Bounds ComputeBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }
}
