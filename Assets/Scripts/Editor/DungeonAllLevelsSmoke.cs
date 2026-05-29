using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonAllLevelsSmoke
{
    private const double TimeoutSeconds = 120.0;

    private static int phase;
    private static int expectedLevel;
    private static int clearedLevel;
    private static int framesInLevel;
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
            expectedLevel = 0;
            clearedLevel = -1;
            framesInLevel = 0;
            startedAt = EditorApplication.timeSinceStartup;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Debug.Log("[DungeonAllLevelsSmoke] Entering Play Mode");
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonAllLevelsSmoke] FAILED TO START\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[DungeonAllLevelsSmoke] Entered Play Mode");
            EditorApplication.update += Tick;
        }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
            {
                throw new InvalidOperationException("Timeout in all-levels smoke phase " + phase + " expectedLevel=" + expectedLevel);
            }

            DungeonGameRuntime runtime = DungeonGameRuntime.Instance;
            if (runtime == null || LevelManager.Instance == null)
            {
                return;
            }

            switch (phase)
            {
                case 0:
                    if (Time.frameCount < 3) return;
                    runtime.StartNewGame();
                    phase = 1;
                    break;
                case 1:
                    ClearCurrentLevel(runtime);
                    break;
                case 2:
                    if (runtime.State != DungeonState.Victory)
                    {
                        runtime.DoorEntered();
                        return;
                    }
                    if (runtime.RoomsCleared != 10)
                    {
                        throw new InvalidOperationException("Victory should mark 10 cleared rooms, got " + runtime.RoomsCleared);
                    }
                    Debug.Log("[DungeonAllLevelsSmoke] OK");
                    Finish(0);
                    break;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonAllLevelsSmoke] FAILED\n" + exception);
            Finish(1);
        }
    }

    private static void ClearCurrentLevel(DungeonGameRuntime runtime)
    {
        if (runtime.IsPlaying && runtime.CurrentLevelIndex == expectedLevel + 1)
        {
            expectedLevel++;
            clearedLevel = -1;
            framesInLevel = 0;
            return;
        }

        if (!runtime.IsPlaying || runtime.CurrentLevelIndex != expectedLevel || FindPlayer() == null)
        {
            return;
        }

        framesInLevel++;
        if (framesInLevel < 3)
        {
            return;
        }

        DungeonDoor door = UnityEngine.Object.FindAnyObjectByType<DungeonDoor>();
        if (door == null)
        {
            return;
        }

        if (clearedLevel != expectedLevel)
        {
            if (door.IsOpen)
            {
                throw new InvalidOperationException("Door should start closed in level " + expectedLevel);
            }

            int enemies = DefeatAllEnemiesWithPlayerAttack(12);
            if (enemies < 1)
            {
                throw new InvalidOperationException("Level " + expectedLevel + " spawned no enemies.");
            }

            ValidateDefeatedEnemiesAreHarmless();
            clearedLevel = expectedLevel;
            Debug.Log("[DungeonAllLevelsSmoke] Cleared level " + (expectedLevel + 1) + " enemies=" + enemies);
            return;
        }

        if (!door.IsOpen)
        {
            return;
        }

        runtime.DoorEntered();
        if (expectedLevel >= 9)
        {
            phase = 2;
        }
    }

    private static int DefeatAllEnemiesWithPlayerAttack(int hitsPerEnemy)
    {
        HashSet<GameObject> enemies = FindEnemyObjects();
        PlayerMovement movement = FindPlayer().GetComponent<PlayerMovement>();
        PlayerCombat combat = FindPlayer().GetComponent<PlayerCombat>();
        if (movement == null || combat == null)
        {
            throw new InvalidOperationException("Player is missing movement or combat component.");
        }

        foreach (GameObject enemy in enemies)
        {
            movement.ResetAt(enemy.transform.position + Vector3.back * 1.05f);
            Physics.SyncTransforms();
            for (int hit = 0; hit < hitsPerEnemy; hit++)
            {
                combat.Attack();
            }
        }

        return enemies.Count;
    }

    private static void ValidateDefeatedEnemiesAreHarmless()
    {
        foreach (GameObject enemy in FindEnemyObjects())
        {
            foreach (EnemyTouchDamage damage in enemy.GetComponents<EnemyTouchDamage>())
            {
                if (damage.enabled)
                {
                    throw new InvalidOperationException(enemy.name + " kept EnemyTouchDamage enabled after death.");
                }
            }

            foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
            {
                if (collider.enabled)
                {
                    throw new InvalidOperationException(enemy.name + " kept collider enabled after death: " + collider.name);
                }
            }
        }
    }

    private static HashSet<GameObject> FindEnemyObjects()
    {
        HashSet<GameObject> enemies = new HashSet<GameObject>();
        foreach (Slime enemy in UnityEngine.Object.FindObjectsByType<Slime>()) enemies.Add(enemy.gameObject);
        foreach (Bat enemy in UnityEngine.Object.FindObjectsByType<Bat>()) enemies.Add(enemy.gameObject);
        foreach (Wizard enemy in UnityEngine.Object.FindObjectsByType<Wizard>()) enemies.Add(enemy.gameObject);
        foreach (Gnome enemy in UnityEngine.Object.FindObjectsByType<Gnome>()) enemies.Add(enemy.gameObject);
        foreach (Boss enemy in UnityEngine.Object.FindObjectsByType<Boss>()) enemies.Add(enemy.gameObject);
        return enemies;
    }

    private static GameObject FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        return player != null && player.activeInHierarchy ? player : null;
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
