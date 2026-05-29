using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonPauseSmoke
{
    private const double TimeoutSeconds = 25.0;

    private static int phase;
    private static int framesInPhase;
    private static double startedAt;
    private static bool originalEnterPlayModeOptionsEnabled;
    private static EnterPlayModeOptions originalEnterPlayModeOptions;

    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/LevelScene.unity");
            originalEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            originalEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            phase = 0;
            framesInPhase = 0;
            startedAt = EditorApplication.timeSinceStartup;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Debug.Log("[DungeonPauseSmoke] Entering Play Mode");
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonPauseSmoke] FAILED TO START\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[DungeonPauseSmoke] Entered Play Mode");
            EditorApplication.update += Tick;
        }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
            {
                throw new InvalidOperationException("Timeout in pause smoke phase " + phase);
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
                    RequireState(runtime, DungeonState.Menu, "initial menu");
                    runtime.StartNewGame();
                    NextPhase();
                    break;
                case 1:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 0) return;
                    if (!Mathf.Approximately(Time.timeScale, 1f))
                    {
                        throw new InvalidOperationException("timeScale should be 1 while playing, got " + Time.timeScale);
                    }
                    runtime.Pause();
                    NextPhase();
                    break;
                case 2:
                    RequireState(runtime, DungeonState.Paused, "after pause");
                    if (Time.timeScale > 0.0001f)
                    {
                        throw new InvalidOperationException("timeScale should be 0 while paused, got " + Time.timeScale);
                    }
                    if (!runtime.IsPlaying == false)
                    {
                        // IsPlaying should be false during pause
                    }
                    if (runtime.IsPlaying)
                    {
                        throw new InvalidOperationException("IsPlaying should be false during pause.");
                    }
                    runtime.Resume();
                    NextPhase();
                    break;
                case 3:
                    RequireState(runtime, DungeonState.Playing, "after resume");
                    if (!Mathf.Approximately(Time.timeScale, 1f))
                    {
                        throw new InvalidOperationException("timeScale should be 1 after resume, got " + Time.timeScale);
                    }
                    runtime.Pause();
                    NextPhase();
                    break;
                case 4:
                    RequireState(runtime, DungeonState.Paused, "pause before menu");
                    runtime.ShowMenu();
                    NextPhase();
                    break;
                case 5:
                    RequireState(runtime, DungeonState.Menu, "after menu from pause");
                    if (!Mathf.Approximately(Time.timeScale, 1f))
                    {
                        throw new InvalidOperationException("timeScale should be 1 after returning to menu, got " + Time.timeScale);
                    }
                    Debug.Log("[DungeonPauseSmoke] OK");
                    Finish(0);
                    break;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonPauseSmoke] FAILED\n" + exception);
            Finish(1);
        }
    }

    private static void RequireState(DungeonGameRuntime runtime, DungeonState expected, string label)
    {
        if (runtime.State != expected)
        {
            throw new InvalidOperationException("Expected " + expected + " during " + label + ", got " + runtime.State);
        }
    }

    private static void NextPhase()
    {
        phase++;
        framesInPhase = 0;
    }

    private static void Finish(int exitCode)
    {
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorSettings.enterPlayModeOptionsEnabled = originalEnterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptions = originalEnterPlayModeOptions;
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
        }
        if (EditorApplication.isPlaying)
        {
            EditorApplication.ExitPlaymode();
        }
        EditorApplication.Exit(exitCode);
    }
}
