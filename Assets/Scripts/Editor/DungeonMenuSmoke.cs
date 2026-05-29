using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonMenuSmoke
{
    private const double TimeoutSeconds = 20.0;

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
            Debug.Log("[DungeonMenuSmoke] Entering Play Mode");
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonMenuSmoke] FAILED TO START\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[DungeonMenuSmoke] Entered Play Mode");
            EditorApplication.update += Tick;
        }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
            {
                throw new InvalidOperationException("Timeout in menu smoke phase " + phase);
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
                    RequirePlayerActive(false);
                    runtime.ShowCredits();
                    NextPhase();
                    break;
                case 1:
                    RequireState(runtime, DungeonState.Credits, "credits");
                    RequirePlayerActive(false);
                    runtime.ShowMenu();
                    NextPhase();
                    break;
                case 2:
                    RequireState(runtime, DungeonState.Menu, "menu after credits");
                    runtime.StartNewGame();
                    NextPhase();
                    break;
                case 3:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 0) return;
                    RequirePlayerActive(true);
                    runtime.ShowMenu();
                    NextPhase();
                    break;
                case 4:
                    RequireState(runtime, DungeonState.Menu, "menu after leaving game");
                    RequirePlayerActive(false);
                    runtime.JumpToLevel(9);
                    NextPhase();
                    break;
                case 5:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 9 || UnityEngine.Object.FindAnyObjectByType<Boss>() == null) return;
                    RequirePlayerActive(true);
                    if (runtime.Health != 3)
                    {
                        throw new InvalidOperationException("Debug jump should reset health to 3.");
                    }
                    if (runtime.RoomsCleared != 9)
                    {
                        throw new InvalidOperationException("Debug jump to boss should mark 9 rooms cleared, got " + runtime.RoomsCleared);
                    }
                    Debug.Log("[DungeonMenuSmoke] OK");
                    Finish(0);
                    break;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonMenuSmoke] FAILED\n" + exception);
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

    private static void RequirePlayerActive(bool expected)
    {
        GameObject player = GameObject.FindWithTag("Player");
        bool active = player != null && player.activeInHierarchy;
        if (active != expected)
        {
            throw new InvalidOperationException("Expected player active=" + expected + ", got " + active);
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
        if (EditorApplication.isPlaying)
        {
            EditorApplication.ExitPlaymode();
        }
        EditorApplication.Exit(exitCode);
    }
}
