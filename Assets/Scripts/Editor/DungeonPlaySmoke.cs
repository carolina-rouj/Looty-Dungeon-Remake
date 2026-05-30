using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonPlaySmoke
{
    private const double PhaseTimeoutSeconds = 18.0;

    private static int phase;
    private static double phaseStartedAt;
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
            phaseStartedAt = EditorApplication.timeSinceStartup;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Debug.Log("[DungeonPlaySmoke] Entering Play Mode");
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonPlaySmoke] FAILED TO START\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[DungeonPlaySmoke] Entered Play Mode");
            EditorApplication.update += Tick;
            phaseStartedAt = EditorApplication.timeSinceStartup;
        }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - phaseStartedAt > PhaseTimeoutSeconds)
            {
                throw new InvalidOperationException("Timeout in smoke phase " + phase);
            }

            DungeonGameRuntime runtime = DungeonGameRuntime.Instance;
            if (runtime == null)
            {
                return;
            }

            switch (phase)
            {
                case 0:
                    if (Time.frameCount < 3 || LevelManager.Instance == null) return;
                    Debug.Log("[DungeonPlaySmoke] Starting new game");
                    runtime.StartNewGame();
                    NextPhase();
                    break;
                case 1:
                    if (!runtime.IsPlaying || FindPlayer() == null || FindDoor() == null) return;
                    if (FindDoor().IsOpen)
                    {
                        throw new InvalidOperationException("First room door should start closed.");
                    }
                    int firstRoomEnemies = DefeatAllEnemiesWithPlayerAttack(12);
                    if (firstRoomEnemies < 1)
                    {
                        throw new InvalidOperationException("First room spawned no enemies.");
                    }
                    ValidateDefeatedEnemiesAreHarmless();
                    Debug.Log("[DungeonPlaySmoke] First room enemies defeated: " + firstRoomEnemies);
                    NextPhase();
                    break;
                case 2:
                    DungeonDoor firstDoor = FindDoor();
                    if (firstDoor == null || !firstDoor.IsOpen) return;
                    Debug.Log("[DungeonPlaySmoke] Entering second room");
                    runtime.DoorEntered();
                    NextPhase();
                    break;
                case 3:
                    if (runtime.CurrentLevelIndex != 1 || !runtime.IsPlaying) return;
                    Debug.Log("[DungeonPlaySmoke] Jumping to final room");
                    runtime.JumpToLevel(9);
                    NextPhase();
                    break;
                case 4:
                    if (runtime.CurrentLevelIndex != 9 || !runtime.IsPlaying || UnityEngine.Object.FindAnyObjectByType<Boss>() == null) return;
                    if (FindDoor() == null || FindDoor().IsOpen)
                    {
                        throw new InvalidOperationException("Final room door should start closed.");
                    }
                    int finalRoomEnemies = DefeatAllEnemiesWithPlayerAttack(12);
                    if (finalRoomEnemies < 1)
                    {
                        throw new InvalidOperationException("Final room spawned no enemies.");
                    }
                    ValidateDefeatedEnemiesAreHarmless();
                    Debug.Log("[DungeonPlaySmoke] Final room enemies defeated: " + finalRoomEnemies);
                    NextPhase();
                    break;
                case 5:
                    DungeonDoor finalDoor = FindDoor();
                    if (finalDoor == null || !finalDoor.IsOpen) return;
                    Debug.Log("[DungeonPlaySmoke] Entering victory");
                    runtime.DoorEntered();
                    NextPhase();
                    break;
                case 6:
                    if (runtime.State != DungeonState.Victory)
                    {
                        runtime.DoorEntered();
                        return;
                    }
                    if (runtime.RoomsCleared != 10)
                    {
                        throw new InvalidOperationException("Victory should mark 10 cleared rooms, got " + runtime.RoomsCleared);
                    }
                    Debug.Log("[DungeonPlaySmoke] OK");
                    Finish(0);
                    break;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonPlaySmoke] FAILED\n" + exception);
            Finish(1);
        }
    }

    private static void NextPhase()
    {
        phase++;
        phaseStartedAt = EditorApplication.timeSinceStartup;
    }

    private static GameObject FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        return player != null && player.activeInHierarchy ? player : null;
    }

    private static DungeonDoor FindDoor()
    {
        return UnityEngine.Object.FindAnyObjectByType<DungeonDoor>();
    }

    private static int DefeatAllEnemiesWithPlayerAttack(int hitsPerEnemy)
    {
        HashSet<GameObject> enemies = new HashSet<GameObject>();
        foreach (Slime enemy in UnityEngine.Object.FindObjectsByType<Slime>()) enemies.Add(enemy.gameObject);
        foreach (Bat enemy in UnityEngine.Object.FindObjectsByType<Bat>()) enemies.Add(enemy.gameObject);
        foreach (Wizard enemy in UnityEngine.Object.FindObjectsByType<Wizard>()) enemies.Add(enemy.gameObject);
        foreach (Gnome enemy in UnityEngine.Object.FindObjectsByType<Gnome>()) enemies.Add(enemy.gameObject);
        foreach (Boss enemy in UnityEngine.Object.FindObjectsByType<Boss>()) enemies.Add(enemy.gameObject);

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

    private static IEnumerable<GameObject> FindEnemyObjects()
    {
        HashSet<GameObject> enemies = new HashSet<GameObject>();
        foreach (Slime enemy in UnityEngine.Object.FindObjectsByType<Slime>()) enemies.Add(enemy.gameObject);
        foreach (Bat enemy in UnityEngine.Object.FindObjectsByType<Bat>()) enemies.Add(enemy.gameObject);
        foreach (Wizard enemy in UnityEngine.Object.FindObjectsByType<Wizard>()) enemies.Add(enemy.gameObject);
        foreach (Gnome enemy in UnityEngine.Object.FindObjectsByType<Gnome>()) enemies.Add(enemy.gameObject);
        foreach (Boss enemy in UnityEngine.Object.FindObjectsByType<Boss>()) enemies.Add(enemy.gameObject);
        return enemies;
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
