using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DungeonBatchValidator
{
    private static readonly HashSet<string> ValidEnemyTypes = new HashSet<string>
    {
        "Slime",
        "Bat",
        "Wizard",
        "Gnome",
        "Boss"
    };

    private static readonly HashSet<string> ValidTrapTypes = new HashSet<string>
    {
        "Spikes",
        "Flame",
        "Blade",
        "RastroSlime",
        "SpiderWebs",
        "RetractileFork"
    };

    private static readonly string[] LevelFiles =
    {
        "level1",
        "level2",
        "level3",
        "level4",
        "level5",
        "level6",
        "level7",
        "level8",
        "level9",
        "final_level"
    };

    public static void Run()
    {
        try
        {
            ValidateSceneAndSettings();
            ValidateRuntimeContracts();
            ValidateLevels();
            Debug.Log("[DungeonBatchValidator] OK");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonBatchValidator] FAILED\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void ValidateSceneAndSettings()
    {
        string scenePath = "Assets/Scenes/LevelScene.unity";
        if (!File.Exists(scenePath))
        {
            throw new InvalidOperationException("Missing LevelScene.unity");
        }

        bool sceneInBuild = false;
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && scene.path == scenePath)
            {
                sceneInBuild = true;
                break;
            }
        }
        if (!sceneInBuild)
        {
            throw new InvalidOperationException("LevelScene.unity is not enabled in build settings.");
        }

        EditorSceneManager.OpenScene(scenePath);
        LevelManager manager = UnityEngine.Object.FindAnyObjectByType<LevelManager>();
        if (manager == null)
        {
            throw new InvalidOperationException("LevelScene has no LevelManager.");
        }
        if (manager.levelFileName != "level1")
        {
            throw new InvalidOperationException("LevelScene LevelManager should start at level1, found: " + manager.levelFileName);
        }
        if (manager.floorPrefab == null || manager.floorDarkPrefab == null || manager.floorBluePrefab == null)
        {
            throw new InvalidOperationException("LevelManager floor prefabs are incomplete.");
        }
        if (manager.slimePrefab == null || manager.rastroSlimePrefab == null)
        {
            throw new InvalidOperationException("LevelManager slime/rastro prefabs are incomplete.");
        }
        Slime slime = manager.slimePrefab.GetComponent<Slime>();
        if (slime == null || slime.rastroSlimePrefab == null)
        {
            throw new InvalidOperationException("Slime prefab must include Slime script and rastro prefab reference.");
        }
        if (manager.rastroSlimePrefab.GetComponent<RastroSlime>() == null)
        {
            throw new InvalidOperationException("Rastro prefab must include RastroSlime script.");
        }
    }

    private static void ValidateRuntimeContracts()
    {
        Require(typeof(DungeonGameRuntime) != null, "Missing DungeonGameRuntime.");
        Require(typeof(PlayerMovement) != null, "Missing PlayerMovement.");
        Require(typeof(PlayerCombat) != null, "Missing PlayerCombat.");
        Require(typeof(PlayerHealth) != null, "Missing PlayerHealth.");
        Require(typeof(EnemyTouchDamage) != null, "Missing EnemyTouchDamage.");
        Require(typeof(EnemyProjectile) != null, "Missing EnemyProjectile.");
        Require(typeof(EnemyHitFeedback) != null, "Missing EnemyHitFeedback.");
        Require(typeof(FloatingTextFx) != null, "Missing FloatingTextFx.");
        Require(typeof(EnemyMovementUtility) != null, "Missing EnemyMovementUtility.");
        Require(typeof(CoinPickup) != null, "Missing CoinPickup.");
        Require(typeof(DungeonDoor) != null, "Missing DungeonDoor.");
        Require(typeof(RuntimeTrap) != null, "Missing RuntimeTrap.");
        Require(typeof(Arrow) != null, "Missing Arrow trap helper.");
        Require(typeof(ArrowTrap) != null, "Missing ArrowTrap.");
        Require(typeof(Diana) != null, "Missing Diana trigger.");
        Require(typeof(RetractileFork) != null, "Missing RetractileFork.");
        Require(typeof(SpiderWebs) != null, "Missing SpiderWebs.");
        Require(typeof(RuntimeEnemyVisualAnimator) != null, "Missing RuntimeEnemyVisualAnimator.");
        Require(typeof(RuntimeDungeonAudio) != null, "Missing RuntimeDungeonAudio.");
        Require(typeof(RuntimeSceneLighting) != null, "Missing RuntimeSceneLighting.");
        Require(typeof(DungeonScreenshotCapture) != null, "Missing DungeonScreenshotCapture.");
        Require(typeof(DungeonBuildSmoke) != null, "Missing DungeonBuildSmoke.");
        Require(typeof(DungeonDeliveryValidator) != null, "Missing DungeonDeliveryValidator.");
        Require(typeof(DungeonRequirementsSmoke) != null, "Missing DungeonRequirementsSmoke.");
        Require(typeof(DungeonMenuSmoke) != null, "Missing DungeonMenuSmoke.");
        Require(typeof(DungeonPauseSmoke) != null, "Missing DungeonPauseSmoke.");
        Require(typeof(DungeonAllLevelsSmoke) != null, "Missing DungeonAllLevelsSmoke.");

        string[] expectedSfx =
        {
            "Attack",
            "Hit",
            "Dash",
            "Coin",
            "Door",
            "PlayerDamage",
            "Cast",
            "Trap",
            "FloorFall",
            "GameOver",
            "Victory"
        };
        foreach (string sfx in expectedSfx)
        {
            Require(Enum.IsDefined(typeof(RuntimeSfx), sfx), "Missing runtime SFX: " + sfx);
        }
    }

    private static void ValidateLevels()
    {
        HashSet<string> enemyTypes = new HashSet<string>();
        HashSet<string> nonBossEnemyTypes = new HashSet<string>();
        HashSet<string> trapTypes = new HashSet<string>();
        int firstLevelThreat = 0;
        int lastRegularLevelThreat = 0;

        for (int index = 0; index < LevelFiles.Length; index++)
        {
            string fileName = LevelFiles[index];
            string path = "Assets/Resources/Levels/" + fileName + ".json";
            if (!File.Exists(path))
            {
                throw new InvalidOperationException("Missing level json: " + fileName);
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Empty level json: " + fileName);
            }

            LevelData data = JsonUtility.FromJson<LevelData>(json);
            if (data == null)
            {
                throw new InvalidOperationException("Could not parse level json: " + fileName);
            }

            ValidateGrid(fileName, data);
            ValidateSpawns(fileName, data);
            ValidateFallingFloorFlag(fileName, index, data);

            if (data.enemies != null)
            {
                foreach (EnemySpawn enemy in data.enemies)
                {
                    Require(ValidEnemyTypes.Contains(enemy.type), fileName + " has unknown enemy type: " + enemy.type);
                    enemyTypes.Add(enemy.type);
                    if (enemy.type != "Boss")
                    {
                        nonBossEnemyTypes.Add(enemy.type);
                    }
                }
            }

            if (data.traps != null)
            {
                foreach (TrapSpawn trap in data.traps)
                {
                    Require(ValidTrapTypes.Contains(trap.type), fileName + " has unknown trap type: " + trap.type);
                    trapTypes.Add(trap.type);
                }
            }

            if (index < LevelFiles.Length - 1 && (data.enemies == null || data.enemies.Length == 0))
            {
                throw new InvalidOperationException(fileName + " must contain at least one enemy.");
            }

            int threat = ThreatScore(data);
            if (index == 0)
            {
                firstLevelThreat = threat;
            }
            if (index == LevelFiles.Length - 2)
            {
                lastRegularLevelThreat = threat;
            }
        }

        Require(enemyTypes.Contains("Slime"), "Missing Slime enemy.");
        Require(nonBossEnemyTypes.Count >= 3, "Expected at least 3 non-boss enemy types, found: " + nonBossEnemyTypes.Count);
        Require(enemyTypes.Contains("Boss"), "Missing Boss enemy.");
        Require(trapTypes.Contains("Spikes"), "Missing Spikes trap.");
        Require(trapTypes.Contains("Flame"), "Missing Flame trap.");
        Require(trapTypes.Contains("Blade"), "Missing Blade trap.");
        Require(trapTypes.Contains("SpiderWebs"), "Missing SpiderWebs trap.");
        Require(trapTypes.Contains("RetractileFork"), "Missing RetractileFork trap.");
        Require(lastRegularLevelThreat > firstLevelThreat, "Late levels should be more threatening than level 1.");
    }

    private static void ValidateFallingFloorFlag(string fileName, int index, LevelData data)
    {
        bool shouldFall = index >= 2 && index < LevelFiles.Length - 1;
        Require(data.fallingFloor == shouldFall, fileName + " fallingFloor should be " + shouldFall + ".");
    }

    private static void ValidateGrid(string fileName, LevelData data)
    {
        if (data.grid == null || data.grid.Length == 0)
        {
            throw new InvalidOperationException(fileName + " has no grid.");
        }

        int width = 0;
        int walkable = 0;
        foreach (string row in data.grid)
        {
            if (string.IsNullOrEmpty(row))
            {
                throw new InvalidOperationException(fileName + " contains an empty grid row.");
            }
            width = Mathf.Max(width, row.Length);
            foreach (char cell in row)
            {
                if (IsWalkable(cell))
                {
                    walkable++;
                }
            }
        }

        if (width < 5 || data.grid.Length < 7)
        {
            throw new InvalidOperationException(fileName + " grid is too small.");
        }
        if (walkable < 18)
        {
            throw new InvalidOperationException(fileName + " has too few walkable cells.");
        }
    }

    private static void ValidateSpawns(string fileName, LevelData data)
    {
        if (data.enemies != null)
        {
            foreach (EnemySpawn enemy in data.enemies)
            {
                ValidateCell(fileName, data, enemy.col, enemy.row, "enemy " + enemy.type);
            }
        }

        if (data.traps != null)
        {
            foreach (TrapSpawn trap in data.traps)
            {
                ValidateCell(fileName, data, trap.col, trap.row, "trap " + trap.type);
            }
        }

        if (fileName == "final_level")
        {
            bool hasBoss = false;
            if (data.enemies != null)
            {
                foreach (EnemySpawn enemy in data.enemies)
                {
                    hasBoss |= enemy.type == "Boss";
                }
            }

            Require(hasBoss, "final_level must contain Boss.");
        }
    }

    private static int ThreatScore(LevelData data)
    {
        int score = 0;
        if (data.enemies != null)
        {
            foreach (EnemySpawn enemy in data.enemies)
            {
                score += enemy.type switch
                {
                    "Boss" => 8,
                    "Wizard" => 3,
                    "Bat" => 2,
                    "Gnome" => 2,
                    "Slime" => 1,
                    _ => 0
                };
            }
        }

        if (data.traps != null)
        {
            score += data.traps.Length;
        }

        return score;
    }

    private static void ValidateCell(string fileName, LevelData data, int col, int row, string label)
    {
        if (row < 0 || row >= data.grid.Length)
        {
            throw new InvalidOperationException(fileName + " " + label + " row out of range.");
        }

        string rowText = data.grid[row];
        if (col < 0 || col >= rowText.Length)
        {
            throw new InvalidOperationException(fileName + " " + label + " col out of range.");
        }

        if (!IsWalkable(rowText[col]))
        {
            throw new InvalidOperationException(fileName + " " + label + " is not on walkable floor.");
        }
    }

    private static bool IsWalkable(char cell)
    {
        return cell == '#' || cell == 'D' || cell == 'B' || cell == 'c' || cell == 'C';
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
