using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DungeonScenePrep
{
    private const string ScenePath = "Assets/Scenes/LevelScene.unity";

    public static void Run()
    {
        try
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            LevelManager manager = UnityEngine.Object.FindAnyObjectByType<LevelManager>();
            if (manager == null)
            {
                Debug.LogError("[DungeonScenePrep] No LevelManager in scene.");
                EditorApplication.Exit(1);
                return;
            }

            bool changed = false;
            changed |= AssignIfNull(ref manager.floorTorchPrefab, "Assets/Prefabs/floorTorch.prefab", "floorTorchPrefab");
            changed |= AssignIfNull(ref manager.thronePrefab, "Assets/Prefabs/throne.prefab", "thronePrefab");
            changed |= AssignIfNull(ref manager.calizPrefab, "Assets/Prefabs/caliz.prefab", "calizPrefab");

            EnsureEditorPreview(manager, ref changed);

            if (changed)
            {
                EditorUtility.SetDirty(manager);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[DungeonScenePrep] Scene updated.");
            }
            else
            {
                Debug.Log("[DungeonScenePrep] Scene already up to date.");
            }

            Debug.Log("[DungeonScenePrep] OK");
            EditorApplication.Exit(0);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("[DungeonScenePrep] FAILED\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static bool AssignIfNull(ref GameObject field, string path, string label)
    {
        if (field != null)
        {
            return false;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning("[DungeonScenePrep] Missing prefab " + path);
            return false;
        }

        field = prefab;
        Debug.Log("[DungeonScenePrep] Assigned " + label + " -> " + path);
        return true;
    }

    private static void EnsureEditorPreview(LevelManager manager, ref bool changed)
    {
        Transform existing = manager.transform.Find("EditorPreview");
        if (existing != null)
        {
            return;
        }

        GameObject root = new GameObject("EditorPreview");
        root.transform.SetParent(manager.transform, false);
        root.transform.localPosition = Vector3.zero;

        if (manager.floorPrefab != null)
        {
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(manager.floorPrefab, root.transform);
                    tile.name = "Preview Floor " + x + "_" + z;
                    tile.transform.localPosition = new Vector3(x, 0f, z);
                }
            }
        }

        if (manager.thronePrefab != null)
        {
            GameObject throne = (GameObject)PrefabUtility.InstantiatePrefab(manager.thronePrefab, root.transform);
            throne.name = "Preview Throne";
            throne.transform.localPosition = new Vector3(0f, 0f, 2f);
            throne.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (manager.floorTorchPrefab != null)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject torch = (GameObject)PrefabUtility.InstantiatePrefab(manager.floorTorchPrefab, root.transform);
                torch.name = "Preview Torch " + side;
                torch.transform.localPosition = new Vector3(2f * side, 0f, -1.5f);
            }
        }

        if (manager.calizPrefab != null)
        {
            GameObject caliz = (GameObject)PrefabUtility.InstantiatePrefab(manager.calizPrefab, root.transform);
            caliz.name = "Preview Caliz";
            caliz.transform.localPosition = new Vector3(0f, 0.4f, 0.5f);
        }

        GameObject hint = new GameObject("Preview Hint");
        hint.transform.SetParent(root.transform, false);
        hint.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        TextMesh text = hint.AddComponent<TextMesh>();
        text.text = "PULSA PLAY";
        text.fontSize = 64;
        text.color = new Color(1f, 0.85f, 0.18f);
        text.alignment = TextAlignment.Center;
        text.anchor = TextAnchor.MiddleCenter;
        text.characterSize = 0.12f;

        Debug.Log("[DungeonScenePrep] Editor preview created.");
        changed = true;
    }
}
