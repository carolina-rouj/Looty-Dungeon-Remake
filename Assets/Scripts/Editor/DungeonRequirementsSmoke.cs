using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonRequirementsSmoke
{
    private const double TimeoutSeconds = 45.0;

    private static int phase;
    private static int framesInPhase;
    private static double startedAt;
    private static double phaseStartedAt;
    private static int floorCountBeforeFall;
    private static int bossFloorCount;
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
            phaseStartedAt = startedAt;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Debug.Log("[DungeonRequirementsSmoke] Entering Play Mode");
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonRequirementsSmoke] FAILED TO START\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[DungeonRequirementsSmoke] Entered Play Mode");
            EditorApplication.update += Tick;
        }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
            {
                throw new InvalidOperationException("Timeout in requirements phase " + phase);
            }

            framesInPhase++;
            DungeonGameRuntime runtime = DungeonGameRuntime.Instance;
            LevelManager manager = LevelManager.Instance;
            if (runtime == null || manager == null)
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
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 0 || FindPlayerCollider() == null) return;
                    if (framesInPhase < 8) return;
                    ValidateRoomBasics(runtime);
                    ValidateCoinPickup(runtime);
                    ValidateSlimeTrailSlow();
                    ValidatePlayerDash();
                    ValidateFloatingTextFeedback();
                    ValidateEnemyProjectile();
                    ValidateEnemyTouchDamage(runtime);
                    NextPhase();
                    break;
                case 2:
                    if (EditorApplication.timeSinceStartup - phaseStartedAt < 0.9) return;
                    runtime.JumpToLevel(5);
                    NextPhase();
                    break;
                case 3:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 5 || CountObjects<RuntimeTrap>() < 3) return;
                    if (framesInPhase < 8) return;
                    ValidateTrapDamage(runtime);
                    ValidateAdvancedTrapEffects(runtime);
                    NextPhase();
                    break;
                case 4:
                    if (EditorApplication.timeSinceStartup - phaseStartedAt < 0.9) return;
                    runtime.DamagePlayer(1, FindPlayer().transform.position + Vector3.left);
                    NextPhase();
                    break;
                case 5:
                    if (EditorApplication.timeSinceStartup - phaseStartedAt < 0.9) return;
                    runtime.DamagePlayer(1, FindPlayer().transform.position + Vector3.right);
                    NextPhase();
                    break;
                case 6:
                    if (runtime.State != DungeonState.GameOver) return;
                    Debug.Log("[DungeonRequirementsSmoke] Game over reached from repeated damage");
                    runtime.JumpToLevel(8);
                    NextPhase();
                    break;
                case 7:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 8) return;
                    if (framesInPhase < 6) return;
                    floorCountBeforeFall = manager.GetActiveFloorCount();
                    if (floorCountBeforeFall <= 0)
                    {
                        throw new InvalidOperationException("No active floor tiles before falling-floor check.");
                    }
                    NextPhase();
                    break;
                case 8:
                    if (EditorApplication.timeSinceStartup - phaseStartedAt < 5.4) return;
                    int floorCountAfterFall = manager.GetActiveFloorCount();
                    if (floorCountAfterFall >= floorCountBeforeFall)
                    {
                        throw new InvalidOperationException("Falling floor did not reduce active floor count. Before=" + floorCountBeforeFall + " after=" + floorCountAfterFall);
                    }
                    Debug.Log("[DungeonRequirementsSmoke] Falling floor reduced tiles " + floorCountBeforeFall + " -> " + floorCountAfterFall);
                    runtime.RespawnPlayerAfterFall();
                    if (!manager.HasFloorAt(FindPlayer().transform.position))
                    {
                        throw new InvalidOperationException("Respawn after falling floor should land on an active tile.");
                    }
                    Debug.Log("[DungeonRequirementsSmoke] Respawn after fall lands on active floor");
                    runtime.JumpToLevel(9);
                    NextPhase();
                    break;
                case 9:
                    if (!runtime.IsPlaying || runtime.CurrentLevelIndex != 9 || UnityEngine.Object.FindAnyObjectByType<Boss>() == null) return;
                    if (framesInPhase < 8) return;
                    bossFloorCount = manager.GetActiveFloorCount();
                    if (bossFloorCount <= 0)
                    {
                        throw new InvalidOperationException("No active floor tiles in boss room.");
                    }
                    NextPhase();
                    break;
                case 10:
                    if (EditorApplication.timeSinceStartup - phaseStartedAt < 5.0) return;
                    if (manager.GetActiveFloorCount() != bossFloorCount)
                    {
                        throw new InvalidOperationException("Boss room floor should not fall.");
                    }
                    Debug.Log("[DungeonRequirementsSmoke] Boss room floor stayed stable");
                    Debug.Log("[DungeonRequirementsSmoke] OK");
                    Finish(0);
                    break;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonRequirementsSmoke] FAILED\n" + exception);
            Finish(1);
        }
    }

    private static void ValidateRoomBasics(DungeonGameRuntime runtime)
    {
        if (CountObjects<CoinPickup>() < 3)
        {
            throw new InvalidOperationException("Level 1 should spawn at least 3 coins.");
        }
        if (CountNamedObjects("Decoration ") < 5)
        {
            throw new InvalidOperationException("Level should spawn at least 5 decorations.");
        }
        if (UnityEngine.Object.FindAnyObjectByType<Slime>() == null)
        {
            throw new InvalidOperationException("Level 1 should spawn a slime.");
        }
        if (CountNamedObjects("Runtime Key Light") < 1 || CountNamedObjects("Runtime Rim Light") < 1)
        {
            throw new InvalidOperationException("Runtime scene lighting was not created.");
        }
        if (CountNamedObjects("Torch Flame") < 1 || CountNamedObjects("Torch Light") < 1)
        {
            throw new InvalidOperationException("Runtime torch flame/light decorations were not created.");
        }
        if (runtime.Health != 3 || runtime.Coins != 0)
        {
            throw new InvalidOperationException("New game should start with 3 health and 0 coins.");
        }
        Debug.Log("[DungeonRequirementsSmoke] Runtime lighting and torch feedback are present");
    }

    private static void ValidateCoinPickup(DungeonGameRuntime runtime)
    {
        CoinPickup coin = UnityEngine.Object.FindAnyObjectByType<CoinPickup>();
        Collider playerCollider = FindPlayerCollider();
        if (coin == null || playerCollider == null)
        {
            throw new InvalidOperationException("Cannot validate coin pickup.");
        }

        int before = runtime.Coins;
        coin.SendMessage("OnTriggerEnter", playerCollider, SendMessageOptions.RequireReceiver);
        if (runtime.Coins != before + 1)
        {
            throw new InvalidOperationException("Coin pickup did not increment coin counter.");
        }
        Debug.Log("[DungeonRequirementsSmoke] Coin pickup increments HUD counter");
    }

    private static void ValidateSlimeTrailSlow()
    {
        GameObject playerObject = FindPlayer();
        Collider playerCollider = FindPlayerCollider();
        PlayerMovement player = playerObject != null ? playerObject.GetComponent<PlayerMovement>() : null;
        if (player == null || playerCollider == null)
        {
            throw new InvalidOperationException("Cannot validate slime trail slow.");
        }

        GameObject trailObject = new GameObject("Smoke Test Slime Trail");
        trailObject.transform.position = playerObject.transform.position;
        RastroSlime trail = trailObject.AddComponent<RastroSlime>();
        trail.slowFactor = 0.5f;
        trail.slowDuration = 2f;
        trail.SendMessage("OnTriggerEnter", playerCollider, SendMessageOptions.RequireReceiver);

        if (!player.IsSlowed || player.CurrentSpeedMultiplier > 0.51f)
        {
            throw new InvalidOperationException("Slime trail did not slow the player. Multiplier=" + player.CurrentSpeedMultiplier);
        }

        player.ResetAt(LevelManager.Instance.GetPlayerSpawnPosition());
        if (player.IsSlowed || player.CurrentSpeedMultiplier != 1f)
        {
            throw new InvalidOperationException("Player reset should clear slime slow.");
        }

        UnityEngine.Object.Destroy(trailObject);
        Debug.Log("[DungeonRequirementsSmoke] Slime trail slows player");
    }

    private static void ValidatePlayerDash()
    {
        GameObject playerObject = FindPlayer();
        PlayerMovement player = playerObject != null ? playerObject.GetComponent<PlayerMovement>() : null;
        if (player == null)
        {
            throw new InvalidOperationException("Cannot validate player dash.");
        }

        bool dashed = player.TryDash(Vector3.forward);
        if (!dashed || !player.IsDashing || player.DashCooldownRemaining <= 0f)
        {
            throw new InvalidOperationException("Player dash did not enter active/cooldown state.");
        }

        player.ResetAt(LevelManager.Instance.GetPlayerSpawnPosition());
        if (player.IsDashing || player.DashCooldownRemaining > 0f)
        {
            throw new InvalidOperationException("Player reset should clear dash state.");
        }

        Debug.Log("[DungeonRequirementsSmoke] Player dash activates and resets cleanly");
    }

    private static void ValidateFloatingTextFeedback()
    {
        int before = CountObjects<FloatingTextFx>();
        RuntimeVfx.FloatingText(Vector3.up * 2f, "+1", Color.yellow);
        int after = CountObjects<FloatingTextFx>();
        if (after <= before)
        {
            throw new InvalidOperationException("Floating text feedback did not spawn.");
        }

        foreach (FloatingTextFx text in UnityEngine.Object.FindObjectsByType<FloatingTextFx>())
        {
            UnityEngine.Object.Destroy(text.gameObject);
        }
        Debug.Log("[DungeonRequirementsSmoke] Floating text feedback can be spawned");
    }

    private static void ValidateEnemyProjectile()
    {
        GameObject player = FindPlayer();
        if (player == null)
        {
            throw new InvalidOperationException("Cannot validate enemy projectile.");
        }

        int before = CountObjects<EnemyProjectile>();
        EnemyProjectile.Launch(player.transform.position + Vector3.forward * 2f + Vector3.up * 0.8f, Vector3.forward, 2.5f, new Color(0.2f, 0.85f, 1f), 0.24f);
        int after = CountObjects<EnemyProjectile>();
        if (after <= before)
        {
            throw new InvalidOperationException("Enemy projectile launch did not create a projectile.");
        }

        foreach (EnemyProjectile projectile in UnityEngine.Object.FindObjectsByType<EnemyProjectile>())
        {
            UnityEngine.Object.Destroy(projectile.gameObject);
        }
        Debug.Log("[DungeonRequirementsSmoke] Enemy projectile attack can be spawned");
    }

    private static void ValidateEnemyTouchDamage(DungeonGameRuntime runtime)
    {
        EnemyTouchDamage enemyDamage = UnityEngine.Object.FindAnyObjectByType<EnemyTouchDamage>();
        Collider playerCollider = FindPlayerCollider();
        if (enemyDamage == null || playerCollider == null)
        {
            throw new InvalidOperationException("Cannot validate enemy touch damage.");
        }

        int before = runtime.Health;
        enemyDamage.SendMessage("OnTriggerStay", playerCollider, SendMessageOptions.RequireReceiver);
        if (runtime.Health != before - 1)
        {
            throw new InvalidOperationException("Enemy touch did not damage player. Before=" + before + " after=" + runtime.Health);
        }
        if (!runtime.IsDamageOverlayActive)
        {
            throw new InvalidOperationException("Damage feedback overlay did not activate after enemy touch damage.");
        }
        if (runtime.RoomProgress01 <= 0f)
        {
            throw new InvalidOperationException("HUD room progress should be positive while playing.");
        }

        Debug.Log("[DungeonRequirementsSmoke] Enemy touch damage decrements health and activates damage UI");
    }

    private static void ValidateTrapDamage(DungeonGameRuntime runtime)
    {
        RuntimeTrap trap = FindTrap("Blade Trap");
        Collider playerCollider = FindPlayerCollider();
        if (trap == null || playerCollider == null)
        {
            throw new InvalidOperationException("Cannot validate trap damage.");
        }

        int before = runtime.Health;
        trap.SendMessage("OnTriggerStay", playerCollider, SendMessageOptions.RequireReceiver);
        if (runtime.Health != before - 1)
        {
            throw new InvalidOperationException("Blade trap did not damage player. Before=" + before + " after=" + runtime.Health);
        }
        Debug.Log("[DungeonRequirementsSmoke] Trap damage decrements health");
    }

    private static void ValidateAdvancedTrapEffects(DungeonGameRuntime runtime)
    {
        GameObject playerObject = FindPlayer();
        Collider playerCollider = FindPlayerCollider();
        PlayerMovement movement = playerObject != null ? playerObject.GetComponent<PlayerMovement>() : null;
        Transform parent = LevelManager.Instance != null ? LevelManager.Instance.transform : null;
        if (playerObject == null || playerCollider == null || movement == null || parent == null)
        {
            throw new InvalidOperationException("Cannot validate advanced trap effects.");
        }

        GameObject webObject = RuntimeTrap.Create("SpiderWebs", playerObject.transform.position + Vector3.right * 0.25f, parent);
        SpiderWebs web = webObject != null ? webObject.GetComponent<SpiderWebs>() : null;
        if (web == null)
        {
            throw new InvalidOperationException("Runtime SpiderWebs fallback was not created.");
        }

        SetPrivateField(web, "currentState", SpiderWebs.WebState.Partial);
        SetPrivateField(web, "playerEffectApplied", false);
        web.SendMessage("OnTriggerStay", playerCollider, SendMessageOptions.RequireReceiver);
        if (!movement.IsSlowed || movement.CurrentSpeedMultiplier > 0.11f)
        {
            throw new InvalidOperationException("SpiderWebs partial state did not slow the player. Multiplier=" + movement.CurrentSpeedMultiplier);
        }

        movement.ResetAt(LevelManager.Instance.GetPlayerSpawnPosition());
        if (movement.IsSlowed || movement.CurrentSpeedMultiplier != 1f)
        {
            throw new InvalidOperationException("Player reset should clear SpiderWebs slow.");
        }
        UnityEngine.Object.Destroy(webObject);

        ClearPlayerInvulnerability();
        int beforeFork = runtime.Health;
        GameObject forkObject = RuntimeTrap.Create("RetractileFork", playerObject.transform.position, parent);
        RetractileFork fork = forkObject != null ? forkObject.GetComponent<RetractileFork>() : null;
        if (fork == null)
        {
            throw new InvalidOperationException("Runtime RetractileFork fallback was not created.");
        }

        SetPrivateField(fork, "state", RetractileFork.ForkState.Extending);
        fork.SendMessage("OnTriggerStay", playerCollider, SendMessageOptions.RequireReceiver);
        if (runtime.Health != beforeFork - 1)
        {
            throw new InvalidOperationException("RetractileFork did not damage player. Before=" + beforeFork + " after=" + runtime.Health);
        }
        UnityEngine.Object.Destroy(forkObject);

        Debug.Log("[DungeonRequirementsSmoke] SpiderWebs slow and RetractileFork damage work");
    }

    private static void ClearPlayerInvulnerability()
    {
        GameObject playerObject = FindPlayer();
        PlayerHealth health = playerObject != null ? playerObject.GetComponent<PlayerHealth>() : null;
        if (health == null)
        {
            throw new InvalidOperationException("Cannot clear player invulnerability for trap validation.");
        }

        SetPrivateField(health, "invulnerableUntil", 0f);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null)
        {
            throw new InvalidOperationException("Cannot set private field on null target: " + fieldName);
        }

        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new InvalidOperationException("Missing private field " + fieldName + " on " + target.GetType().Name);
        }

        field.SetValue(target, value);
    }

    private static RuntimeTrap FindTrap(string name)
    {
        foreach (RuntimeTrap trap in UnityEngine.Object.FindObjectsByType<RuntimeTrap>())
        {
            if (trap.name == name)
            {
                return trap;
            }
        }

        return null;
    }

    private static GameObject FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        return player != null && player.activeInHierarchy ? player : null;
    }

    private static Collider FindPlayerCollider()
    {
        GameObject player = FindPlayer();
        return player != null ? player.GetComponent<Collider>() : null;
    }

    private static int CountObjects<T>() where T : UnityEngine.Object
    {
        return UnityEngine.Object.FindObjectsByType<T>().Length;
    }

    private static int CountNamedObjects(string namePrefix)
    {
        int count = 0;
        foreach (Transform transform in UnityEngine.Object.FindObjectsByType<Transform>())
        {
            if (transform.name.StartsWith(namePrefix, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    private static void NextPhase()
    {
        phase++;
        framesInPhase = 0;
        phaseStartedAt = EditorApplication.timeSinceStartup;
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
