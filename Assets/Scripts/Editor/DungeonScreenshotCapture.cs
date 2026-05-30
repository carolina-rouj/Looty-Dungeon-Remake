using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonScreenshotCapture
{
    private const int Width = 1280;
    private const int Height = 720;
    private const double TimeoutSeconds = 40.0;
    private const string OutputDirectory = "Docs/Captures";

    private static int phase;
    private static int framesInPhase;
    private static double startedAt;
    private static double phaseStartedAt;
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

            phase = 0;
            framesInPhase = 0;
            startedAt = EditorApplication.timeSinceStartup;
            phaseStartedAt = startedAt;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Debug.Log("[DungeonScreenshotCapture] Entering Play Mode");
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonScreenshotCapture] FAILED TO START\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[DungeonScreenshotCapture] Entered Play Mode");
            EditorApplication.update += Tick;
        }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
            {
                throw new InvalidOperationException("Screenshot capture timeout in phase " + phase);
            }

            framesInPhase++;
            DungeonGameRuntime runtime = DungeonGameRuntime.Instance;
            if (runtime == null || LevelManager.Instance == null)
            {
                return;
            }

            switch (phase)
            {
                case 0:
                    if (framesInPhase < 3) return;
                    runtime.StartNewGame();
                    NextPhase();
                    break;
                case 1:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 0 || FindPlayer() == null) return;
                    if (framesInPhase < 8) return;
                    Capture("01_level1_start.png");
                    runtime.JumpToLevel(5);
                    NextPhase();
                    break;
                case 2:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 5 || CountObjects<RuntimeTrap>() < 3) return;
                    if (framesInPhase < 10) return;
                    Capture("02_level6_traps.png");
                    runtime.JumpToLevel(8);
                    NextPhase();
                    break;
                case 3:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 8) return;
                    if (EditorApplication.timeSinceStartup - phaseStartedAt < 5.8) return;
                    Capture("03_level9_falling_floor.png");
                    runtime.JumpToLevel(9);
                    NextPhase();
                    break;
                case 4:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 9 || UnityEngine.Object.FindAnyObjectByType<Boss>() == null) return;
                    if (framesInPhase < 10) return;
                    Capture("04_boss_room.png");
                    Debug.Log("[DungeonScreenshotCapture] OK");
                    Finish(0);
                    break;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonScreenshotCapture] FAILED\n" + exception);
            Finish(1);
        }
    }

    private static void NextPhase()
    {
        phase++;
        framesInPhase = 0;
        phaseStartedAt = EditorApplication.timeSinceStartup;
    }

    private static GameObject FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        return player != null && player.activeInHierarchy ? player : null;
    }

    private static int CountObjects<T>() where T : UnityEngine.Object
    {
        return UnityEngine.Object.FindObjectsByType<T>().Length;
    }

    private static void Capture(string fileName)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            throw new InvalidOperationException("No Main Camera available for screenshot.");
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

            ValidateImage(texture, fileName);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Debug.Log("[DungeonScreenshotCapture] Captured " + path);
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    private static void ValidateImage(Texture2D texture, string fileName)
    {
        Color32[] pixels = texture.GetPixels32();
        long total = 0;
        byte min = byte.MaxValue;
        byte max = byte.MinValue;

        for (int i = 0; i < pixels.Length; i += 64)
        {
            byte luminance = (byte)((pixels[i].r + pixels[i].g + pixels[i].b) / 3);
            total += luminance;
            min = Math.Min(min, luminance);
            max = Math.Max(max, luminance);
        }

        float average = total / (pixels.Length / 64f);
        int range = max - min;
        if (average < 2f || range < 8)
        {
            throw new InvalidOperationException(fileName + " looks blank. Average=" + average + " range=" + range);
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
