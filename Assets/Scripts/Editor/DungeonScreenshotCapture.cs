using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonScreenshotCapture
{
    private const int Width = 1600;
    private const int Height = 900;
    private const double TimeoutSeconds = 320.0;
    private const string OutputDirectory = "Memoria/captures";

    private static readonly string[] FileNames =
    {
        "lvl01.png", "lvl02.png", "lvl03.png", "lvl04.png", "lvl05.png",
        "lvl06.png", "lvl07.png", "lvl08.png", "lvl09.png", "lvl10_boss.png"
    };

    private static int level;
    private static int framesInLevel;
    private static double startedAt;
    private static double levelStartedAt;
    private static bool requested;
    private static bool originalEnterPlayModeOptionsEnabled;
    private static EnterPlayModeOptions originalEnterPlayModeOptions;

    public static void Run()
    {
        try
        {
            Directory.CreateDirectory(OutputDirectory);
            EditorSceneManager.OpenScene("Assets/Scenes/LevelScene.unity");
            originalEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            originalEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

            level = 0;
            framesInLevel = 0;
            requested = false;
            startedAt = EditorApplication.timeSinceStartup;
            levelStartedAt = startedAt;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Debug.Log("[Capture] Entering Play Mode");
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogError("[Capture] FAILED TO START\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[Capture] Entered Play Mode");
            EditorApplication.update += Tick;
        }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
            {
                throw new InvalidOperationException("Capture timeout at level " + (level + 1));
            }

            DungeonGameRuntime runtime = DungeonGameRuntime.Instance;
            if (runtime == null || LevelManager.Instance == null)
            {
                return;
            }

            // Asegura estar en la sala objetivo.
            if (!requested)
            {
                if (level == 0) runtime.StartNewGame();
                else runtime.JumpToLevel(level);
                requested = true;
                framesInLevel = 0;
                levelStartedAt = EditorApplication.timeSinceStartup;
                return;
            }

            framesInLevel++;

            bool ready = runtime.IsPlaying
                         && runtime.CurrentLevelIndex == level
                         && GameObject.FindWithTag("Player") != null
                         && GameObject.FindWithTag("Player").activeInHierarchy;
            bool bossOk = level != 9 || UnityEngine.Object.FindAnyObjectByType<Boss>() != null;

            // Espera unos frames para que el visual/camara se asienten.
            if (!ready || !bossOk || framesInLevel < 14)
            {
                return;
            }

            // En la sala del boss capturamos pronto (antes de que empiece a caer el suelo),
            // tras unos frames extra para que el boss y su visual esten montados.
            if (level == 9 && framesInLevel < 26)
            {
                return;
            }

            Capture(FileNames[level]);
            level++;
            requested = false;

            if (level >= FileNames.Length)
            {
                Debug.Log("[Capture] OK");
                Finish(0);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[Capture] FAILED\n" + exception);
            Finish(1);
        }
    }

    private static void Capture(string fileName)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            throw new InvalidOperationException("No Main Camera for screenshot.");
        }

        string path = Path.Combine(OutputDirectory, fileName);
        RenderTexture previousTarget = camera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture renderTexture = new RenderTexture(Width, Height, 24);
        Texture2D texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);

        try
        {
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Debug.Log("[Capture] Wrote " + path);
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    private static void Finish(int exitCode)
    {
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorSettings.enterPlayModeOptionsEnabled = originalEnterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptions = originalEnterPlayModeOptions;
        if (EditorApplication.isPlaying)
        {
            EditorApplication.ExitPlaymode();
        }
        EditorApplication.Exit(exitCode);
    }
}
