using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DungeonState
{
    Menu,
    Credits,
    Playing,
    Paused,
    GameOver,
    Victory
}

[DefaultExecutionOrder(-1000)]
public class DungeonGameRuntime : MonoBehaviour
{
    public static DungeonGameRuntime Instance { get; private set; }

    public DungeonState State { get; private set; } = DungeonState.Menu;
    public int CurrentLevelIndex { get; private set; }
    public int RoomsCleared { get; private set; }
    public int Coins { get; private set; }
    public int Health { get; private set; } = 3;
    public int EnemiesDefeated { get; private set; }
    public int EnemiesAlive => aliveEnemies.Count;
    public float RunSeconds { get; private set; }
    public int BestCoins { get; private set; }
    public float BestVictorySeconds { get; private set; }
    public bool IsPlaying => State == DungeonState.Playing;
    public bool IsDamageOverlayActive => Time.time < damageOverlayUntil;
    public float RoomProgress01
    {
        get
        {
            if (State == DungeonState.Victory) return 1f;
            if (State != DungeonState.Playing && State != DungeonState.GameOver) return 0f;
            return Mathf.Clamp01((CurrentLevelIndex + 1f) / levelNames.Length);
        }
    }

    private readonly string[] levelNames =
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

    private LevelManager levelManager;
    private PlayerMovement player;
    private PlayerHealth playerHealth;
    private DungeonDoor door;
    private CameraFollowDungeon cameraFollow;
    private RuntimeDungeonAudio runtimeAudio;
    private readonly HashSet<GameObject> aliveEnemies = new HashSet<GameObject>();
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle hudStyle;
    private GUIStyle buttonStyle;
    private float guiScale = -1f;
    private float messageUntil;
    private string message = "";
    private float transitionLockUntil;
    private float damageOverlayUntil;
    private bool standaloneSmokeStarted;
    private float runStartedAt;
    private bool runTimerActive;
    private const string PrefBestCoinsKey = "vj3d.best_coins";
    private const string PrefBestVictorySecondsKey = "vj3d.best_victory_seconds";
    private float sectionBannerUntil;
    private string sectionBanner = "";
    private int lastBannerSection = -1;
    private float transitionStartedAt = -1f;
    private float transitionFinishedAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindAnyObjectByType<DungeonGameRuntime>() != null)
        {
            return;
        }

        GameObject runtime = new GameObject("Dungeon Game Runtime");
        runtime.AddComponent<DungeonGameRuntime>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        runtimeAudio = gameObject.AddComponent<RuntimeDungeonAudio>();
        LoadBestRun();
        CreatePlayer();
        ConfigureCamera();
    }

    private void Start()
    {
        levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.levelFileName = levelNames[0];
        }

        ShowMenu();
        if (ShouldRunStandaloneSmoke())
        {
            standaloneSmokeStarted = true;
            StartCoroutine(RunStandaloneSmoke());
        }
    }

    private void Update()
    {
        if (runTimerActive && State == DungeonState.Playing)
        {
            RunSeconds += Time.unscaledDeltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == DungeonState.Playing)
            {
                Pause();
            }
            else if (State == DungeonState.Paused)
            {
                Resume();
            }
            else if (State == DungeonState.Credits || State == DungeonState.GameOver || State == DungeonState.Victory)
            {
                ShowMenu();
            }
        }

        if (State == DungeonState.Menu)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                StartNewGame();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                ShowCredits();
            }
        }
        else if (State == DungeonState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.P))
            {
                Resume();
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                ShowMenu();
            }
        }
        else if (State == DungeonState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))
            {
                LoadLevelIndex(CurrentLevelIndex, true);
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                ShowMenu();
            }
        }
        else if (State == DungeonState.Victory)
        {
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Return))
            {
                ShowMenu();
            }
        }
        else if (State == DungeonState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Pause();
            }
        }

        for (int i = 0; i < levelNames.Length; i++)
        {
            if (WasLevelKeyPressed(i))
            {
                JumpToLevel(i);
                break;
            }
        }
    }

    public void ShowMenu()
    {
        State = DungeonState.Menu;
        Time.timeScale = 1f;
        aliveEnemies.Clear();
        door = null;
        if (levelManager != null)
        {
            levelManager.StopFallingRows();
        }
        SetPlayerActive(false);
        runtimeAudio.PlayMenuMusic();
        ShowMessage("");
        sectionBannerUntil = 0f;
        lastBannerSection = -1;
        transitionStartedAt = -1f;
    }

    public void Pause()
    {
        if (State != DungeonState.Playing)
        {
            return;
        }

        State = DungeonState.Paused;
        Time.timeScale = 0f;
    }

    private void LoadBestRun()
    {
        BestCoins = PlayerPrefs.GetInt(PrefBestCoinsKey, 0);
        BestVictorySeconds = PlayerPrefs.GetFloat(PrefBestVictorySecondsKey, 0f);
    }

    private void UpdateBestRun()
    {
        bool changed = false;
        if (Coins > BestCoins)
        {
            BestCoins = Coins;
            PlayerPrefs.SetInt(PrefBestCoinsKey, BestCoins);
            changed = true;
        }
        if (State == DungeonState.Victory && (BestVictorySeconds <= 0f || RunSeconds < BestVictorySeconds))
        {
            BestVictorySeconds = RunSeconds;
            PlayerPrefs.SetFloat(PrefBestVictorySecondsKey, BestVictorySeconds);
            changed = true;
        }
        if (changed)
        {
            PlayerPrefs.Save();
        }
    }

    public static string FormatSeconds(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remaining = Mathf.FloorToInt(seconds - minutes * 60f);
        return minutes.ToString("00") + ":" + remaining.ToString("00");
    }

    public void Resume()
    {
        if (State != DungeonState.Paused)
        {
            return;
        }

        State = DungeonState.Playing;
        Time.timeScale = 1f;
    }

    public void ShowCredits()
    {
        State = DungeonState.Credits;
        Time.timeScale = 1f;
        if (levelManager != null)
        {
            levelManager.StopFallingRows();
        }
        SetPlayerActive(false);
        runtimeAudio.PlayMenuMusic();
    }

    public void StartNewGame()
    {
        Health = 3;
        Coins = 0;
        RoomsCleared = 0;
        EnemiesDefeated = 0;
        RunSeconds = 0f;
        runTimerActive = true;
        LoadLevelIndex(0, true);
    }

    public void JumpToLevel(int index)
    {
        Health = 3;
        RoomsCleared = Mathf.Clamp(index, 0, levelNames.Length - 1);
        if (!runTimerActive)
        {
            EnemiesDefeated = 0;
            RunSeconds = 0f;
            runTimerActive = true;
        }
        LoadLevelIndex(index, true);
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy != null)
        {
            aliveEnemies.Add(enemy);
            EnemyTouchDamage.Ensure(enemy);
            EnemyHitFeedback.Ensure(enemy);
        }
    }

    public void NotifyLevelBuilt(LevelManager manager)
    {
        levelManager = manager;

        if (State != DungeonState.Playing)
        {
            SetPlayerActive(false);
            return;
        }

        SetPlayerActive(true);
        Vector3 spawn = manager.GetPlayerSpawnPosition();
        player.ResetAt(spawn);
        playerHealth.RefreshVisuals();
        cameraFollow.SetStaticFocus(manager.GetCameraFocusPosition());
        CreateDoor(manager.GetExitPosition());
        manager.StartFallingRowsIfNeeded(CurrentLevelIndex);
        ApplySectionAmbience(CurrentLevelIndex);
        float duration = (CurrentLevelIndex == 0 || CurrentLevelIndex == 2 || CurrentLevelIndex == levelNames.Length - 1) ? 2.8f : 1.6f;
        ShowMessage(BuildRoomEnterMessage(CurrentLevelIndex), duration);
        TriggerSectionBannerIfNeeded(CurrentLevelIndex);
        if (CurrentLevelIndex == levelNames.Length - 1)
        {
            PlayBossEntryDrama(spawn);
        }
        transitionFinishedAt = Time.time;
    }

    private int GetSectionIndex(int level)
    {
        if (level <= 2) return 0;
        if (level <= 5) return 1;
        if (level <= 8) return 2;
        return 3;
    }

    private string GetSectionTitle(int section)
    {
        return section switch
        {
            0 => "Atrios de la mazmorra",
            1 => "Galerias profundas",
            2 => "Catacumbas oscuras",
            _ => "Salon del boss"
        };
    }

    private void TriggerSectionBannerIfNeeded(int level)
    {
        int section = GetSectionIndex(level);
        if (section == lastBannerSection)
        {
            return;
        }

        lastBannerSection = section;
        sectionBanner = GetSectionTitle(section);
        sectionBannerUntil = Time.time + 2.8f;
    }

    private void ApplySectionAmbience(int index)
    {
        Color background;
        Color fogColor;
        float fogDensity;
        Color ambientSky;
        Color ambientEquator;
        Color ambientGround;
        Color keyColor;
        float keyIntensity;
        Color rimColor;
        float rimIntensity;

        if (index <= 2)
        {
            background = new Color(0.18f, 0.08f, 0.08f);
            fogColor = new Color(0.16f, 0.07f, 0.07f);
            fogDensity = 0.022f;
            ambientSky = new Color(0.34f, 0.24f, 0.19f);
            ambientEquator = new Color(0.16f, 0.09f, 0.08f);
            ambientGround = new Color(0.08f, 0.04f, 0.04f);
            keyColor = new Color(1f, 0.82f, 0.62f);
            keyIntensity = 1.25f;
            rimColor = new Color(0.42f, 0.58f, 1f);
            rimIntensity = 0.45f;
        }
        else if (index <= 5)
        {
            background = new Color(0.16f, 0.09f, 0.06f);
            fogColor = new Color(0.18f, 0.09f, 0.05f);
            fogDensity = 0.026f;
            ambientSky = new Color(0.36f, 0.22f, 0.13f);
            ambientEquator = new Color(0.18f, 0.10f, 0.06f);
            ambientGround = new Color(0.08f, 0.04f, 0.02f);
            keyColor = new Color(1f, 0.74f, 0.46f);
            keyIntensity = 1.15f;
            rimColor = new Color(0.62f, 0.42f, 1f);
            rimIntensity = 0.5f;
        }
        else if (index <= 8)
        {
            background = new Color(0.09f, 0.06f, 0.12f);
            fogColor = new Color(0.10f, 0.06f, 0.13f);
            fogDensity = 0.030f;
            ambientSky = new Color(0.22f, 0.18f, 0.34f);
            ambientEquator = new Color(0.12f, 0.08f, 0.18f);
            ambientGround = new Color(0.05f, 0.03f, 0.08f);
            keyColor = new Color(0.84f, 0.74f, 1f);
            keyIntensity = 1.05f;
            rimColor = new Color(0.42f, 1f, 0.78f);
            rimIntensity = 0.6f;
        }
        else
        {
            background = new Color(0.13f, 0.03f, 0.04f);
            fogColor = new Color(0.16f, 0.04f, 0.04f);
            fogDensity = 0.036f;
            ambientSky = new Color(0.45f, 0.18f, 0.10f);
            ambientEquator = new Color(0.20f, 0.06f, 0.05f);
            ambientGround = new Color(0.08f, 0.02f, 0.02f);
            keyColor = new Color(1f, 0.42f, 0.18f);
            keyIntensity = 1.45f;
            rimColor = new Color(1f, 0.18f, 0.08f);
            rimIntensity = 0.8f;
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.backgroundColor = background;
        }

        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.ambientSkyColor = ambientSky;
        RenderSettings.ambientEquatorColor = ambientEquator;
        RenderSettings.ambientGroundColor = ambientGround;

        RuntimeSceneLighting.TintMainLights(keyColor, keyIntensity, rimColor, rimIntensity);
    }

    private void PlayBossEntryDrama(Vector3 origin)
    {
        runtimeAudio.PlaySfx(RuntimeSfx.GameOver);
        cameraFollow.Shake(0.32f, 0.42f);
        RuntimeVfx.Burst(origin + Vector3.up * 1.2f, new Color(1f, 0.35f, 0.1f), 60, 1.2f);
        RuntimeVfx.FloatingText(origin + Vector3.up * 2.2f, "BOSS", new Color(1f, 0.25f, 0.12f));
    }

    private string BuildRoomEnterMessage(int index)
    {
        if (index == 0)
        {
            return "Sala 1 - Mata enemigos para abrir la puerta";
        }
        if (index == 2)
        {
            return "Sala 3 - El suelo empezara a caer";
        }
        if (index == levelNames.Length - 1)
        {
            return "Sala Boss - Aguanta y golpea";
        }
        return "Sala " + (index + 1);
    }

    public void NotifyEnemyDefeated(GameObject enemy)
    {
        if (enemy != null)
        {
            if (aliveEnemies.Remove(enemy))
            {
                EnemiesDefeated++;
                TryDropHeart(enemy.transform.position);
            }
        }

        if (State == DungeonState.Playing && aliveEnemies.Count == 0 && door != null && !door.IsOpen)
        {
            door.Open();
            runtimeAudio.PlaySfx(RuntimeSfx.Door);
            RuntimeVfx.Burst(door.transform.position + Vector3.up, Color.yellow, 28, 0.7f);
            RuntimeVfx.FloatingText(door.transform.position + Vector3.up * 1.55f, "OPEN", new Color(1f, 0.78f, 0.08f));
            ShowMessage(CurrentLevelIndex == levelNames.Length - 1 ? "Boss derrotado" : "Puerta abierta");
        }
    }

    public void DamagePlayer(int amount, Vector3 sourcePosition)
    {
        if (State != DungeonState.Playing || playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(amount, sourcePosition);
    }

    public void CommitPlayerDamage(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
        damageOverlayUntil = Time.time + 0.42f;
        runtimeAudio.PlaySfx(RuntimeSfx.PlayerDamage);
        cameraFollow.Shake(0.22f, 0.26f);
        if (player != null)
        {
            RuntimeVfx.FloatingText(player.transform.position + Vector3.up * 1.55f, "-" + amount, new Color(1f, 0.16f, 0.12f));
        }
        if (Health <= 0)
        {
            State = DungeonState.GameOver;
            runTimerActive = false;
            UpdateBestRun();
            if (levelManager != null)
            {
                levelManager.StopFallingRows();
            }
            runtimeAudio.PlaySfx(RuntimeSfx.GameOver);
            RuntimeVfx.Burst(player.transform.position + Vector3.up, Color.red, 42, 1f);
        }
    }

    public void AddCoin(Vector3 position)
    {
        Coins++;
        runtimeAudio.PlaySfx(RuntimeSfx.Coin);
        RuntimeVfx.Burst(position, Color.yellow, 12, 0.4f);
        RuntimeVfx.FloatingText(position + Vector3.up * 0.45f, "+1", new Color(1f, 0.82f, 0.08f));
    }

    public void AddHealth(Vector3 position)
    {
        if (Health >= 3)
        {
            return;
        }

        Health = Mathf.Min(3, Health + 1);
        runtimeAudio.PlaySfx(RuntimeSfx.Coin);
        RuntimeVfx.Burst(position, new Color(1f, 0.32f, 0.4f), 16, 0.5f);
        RuntimeVfx.FloatingText(position + Vector3.up * 0.45f, "+1", new Color(1f, 0.32f, 0.42f));
        playerHealth.RefreshVisuals();
    }

    private void TryDropHeart(Vector3 position)
    {
        if (Health >= 3 || State != DungeonState.Playing || levelManager == null)
        {
            return;
        }

        if (Random.value > 0.18f)
        {
            return;
        }

        HealthPickup.Create(levelManager.transform, position);
    }

    public void DoorEntered()
    {
        if (State != DungeonState.Playing || Time.time < transitionLockUntil)
        {
            return;
        }

        transitionLockUntil = Time.time + 0.4f;
        transitionStartedAt = Time.time;
        if (CurrentLevelIndex >= levelNames.Length - 1)
        {
            RoomsCleared = levelNames.Length;
            State = DungeonState.Victory;
            runTimerActive = false;
            UpdateBestRun();
            if (levelManager != null)
            {
                levelManager.StopFallingRows();
            }
            runtimeAudio.PlaySfx(RuntimeSfx.Victory);
            RuntimeVfx.Burst(player.transform.position + Vector3.up, Color.yellow, 70, 1.3f);
            return;
        }

        RoomsCleared = Mathf.Max(RoomsCleared, CurrentLevelIndex + 1);
        StartCoroutine(LoadNextLevelRoutine());
    }

    public void RespawnPlayerAfterFall()
    {
        if (State != DungeonState.Playing || levelManager == null)
        {
            return;
        }

        DamagePlayer(1, player.transform.position + Vector3.down);
        player.ResetAt(levelManager.GetPlayerSpawnPosition());
        cameraFollow.Snap();
    }

    public void ShowMessage(string text)
    {
        ShowMessage(text, 1.6f);
    }

    public void ShowMessage(string text, float duration)
    {
        message = text;
        messageUntil = string.IsNullOrEmpty(text) ? 0f : Time.time + Mathf.Max(0.2f, duration);
    }

    public void PlayAttackSfx(bool hit)
    {
        runtimeAudio.PlaySfx(hit ? RuntimeSfx.Hit : RuntimeSfx.Attack);
        cameraFollow.Shake(hit ? 0.12f : 0.06f, hit ? 0.14f : 0.07f);
    }

    public void PlayDash(Vector3 position, Vector3 direction)
    {
        runtimeAudio.PlaySfx(RuntimeSfx.Dash);
        cameraFollow.Shake(0.07f, 0.08f);
        RuntimeVfx.DashTrail(position + Vector3.up * 0.35f, direction);
    }

    public void PlayEnemyDeath(Vector3 position)
    {
        runtimeAudio.PlaySfx(RuntimeSfx.Hit);
        RuntimeVfx.Burst(position + Vector3.up * 0.7f, new Color(1f, 0.35f, 0.15f), 24, 0.65f);
        RuntimeVfx.FloatingText(position + Vector3.up * 1.5f, "KO", new Color(1f, 0.38f, 0.16f));
    }

    public void PlayEnemyHit(Vector3 position)
    {
        runtimeAudio.PlaySfx(RuntimeSfx.Hit);
        RuntimeVfx.Burst(position + Vector3.up * 0.75f, new Color(1f, 0.92f, 0.2f), 10, 0.28f);
        RuntimeVfx.FloatingText(position + Vector3.up * 1.35f, "-1", new Color(1f, 0.92f, 0.22f));
    }

    public void PlayTrapActivated(Vector3 position, string trapType)
    {
        runtimeAudio.PlaySfx(RuntimeSfx.Trap);
        Color color = trapType == "Flame"
            ? new Color(1f, 0.32f, 0.05f)
            : new Color(0.84f, 0.84f, 0.72f);
        RuntimeVfx.Burst(position + Vector3.up * 0.4f, color, 12, 0.35f);
    }

    public void PlayEnemyCast(Vector3 position, Color color)
    {
        runtimeAudio.PlaySfx(RuntimeSfx.Cast);
        RuntimeVfx.Burst(position + Vector3.up * 0.7f, color, 8, 0.22f);
    }

    public void PlayFloorFall(Vector3 position)
    {
        runtimeAudio.PlaySfx(RuntimeSfx.FloorFall);
        cameraFollow.Shake(0.16f, 0.16f);
        RuntimeVfx.Burst(position + Vector3.up * 0.25f, new Color(1f, 0.25f, 0.06f), 18, 0.45f);
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        LoadLevelIndex(CurrentLevelIndex + 1, false);
    }

    private void LoadLevelIndex(int index, bool resetHealth)
    {
        CurrentLevelIndex = Mathf.Clamp(index, 0, levelNames.Length - 1);
        if (resetHealth)
        {
            Health = 3;
        }

        State = DungeonState.Playing;
        Time.timeScale = 1f;
        runTimerActive = true;
        aliveEnemies.Clear();
        SetPlayerActive(true);
        if (CurrentLevelIndex == levelNames.Length - 1)
        {
            runtimeAudio.PlayBossMusic();
        }
        else
        {
            runtimeAudio.PlayGameMusic();
        }

        levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("[DungeonGameRuntime] No hay LevelManager en la escena.");
            return;
        }

        levelManager.LoadLevel(levelNames[CurrentLevelIndex]);
    }

    private void CreatePlayer()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject == null)
        {
            playerObject = new GameObject("Player");
        }

        if (!playerObject.CompareTag("Player"))
        {
            playerObject.tag = "Player";
        }

        CharacterController controller = playerObject.GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = playerObject.AddComponent<CharacterController>();
        }
        controller.height = 1.35f;
        controller.radius = 0.34f;
        controller.center = new Vector3(0f, 0.68f, 0f);
        controller.stepOffset = 0.25f;

        player = playerObject.GetComponent<PlayerMovement>();
        if (player == null)
        {
            player = playerObject.AddComponent<PlayerMovement>();
        }

        PlayerCombat combat = playerObject.GetComponent<PlayerCombat>();
        if (combat == null)
        {
            combat = playerObject.AddComponent<PlayerCombat>();
        }

        playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = playerObject.AddComponent<PlayerHealth>();
        }

        PlayerVisualBuilder.Build(playerObject.transform);
        SetPlayerActive(false);
    }

    private void SetPlayerActive(bool active)
    {
        if (player != null)
        {
            player.gameObject.SetActive(active);
        }
    }

    private void ConfigureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 6.2f;
        camera.transform.rotation = Quaternion.Euler(58f, -38f, 0f);
        camera.backgroundColor = new Color(0.18f, 0.08f, 0.08f);
        camera.clearFlags = CameraClearFlags.SolidColor;

        cameraFollow = camera.GetComponent<CameraFollowDungeon>();
        if (cameraFollow == null)
        {
            cameraFollow = camera.gameObject.AddComponent<CameraFollowDungeon>();
        }

        RuntimeSceneLighting.Ensure();
    }

    private void CreateDoor(Vector3 position)
    {
        if (door != null)
        {
            Destroy(door.gameObject);
        }

        GameObject doorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorObject.name = "Exit Door";
        doorObject.transform.position = position + Vector3.up * 0.75f;
        doorObject.transform.localScale = new Vector3(0.9f, 1.5f, 0.18f);
        if (levelManager != null)
        {
            doorObject.transform.SetParent(levelManager.transform, true);
        }

        door = doorObject.AddComponent<DungeonDoor>();
    }

    private static bool WasLevelKeyPressed(int levelIndex)
    {
        KeyCode alpha = KeyCode.Alpha0 + levelIndex;
        KeyCode keypad = KeyCode.Keypad0 + levelIndex;
        return Input.GetKeyDown(alpha) || Input.GetKeyDown(keypad);
    }

    private void OnGUI()
    {
        EnsureGuiStyles();

        if (State == DungeonState.Menu)
        {
            Rect panel = DrawCenteredPanel(540f, 400f);
            GUI.Label(PanelRect(panel, 20f, 20f, 500f, 70f), "Looty Dungeon 3D", titleStyle);
            GUI.Label(PanelRect(panel, 28f, 92f, 484f, 108f), "WASD/flechas para moverte.  Shift: dash.  Espacio o click: atacar.\nP o Esc: pausa.  0-9: saltar a salas.", bodyStyle);
            string bestLine = (BestCoins > 0 || BestVictorySeconds > 0f)
                ? "Mejor partida: " + BestCoins + " monedas" + (BestVictorySeconds > 0f ? "  -  Victoria " + FormatSeconds(BestVictorySeconds) : "")
                : "Sin partidas previas guardadas.";
            GUI.Label(PanelRect(panel, 28f, 208f, 484f, 32f), bestLine, bodyStyle);
            if (GUI.Button(PanelCenteredRect(panel, 256f, 220f, 46f), "Jugar", buttonStyle)) StartNewGame();
            if (GUI.Button(PanelCenteredRect(panel, 316f, 220f, 46f), "Creditos", buttonStyle)) ShowCredits();
            return;
        }

        if (State == DungeonState.Credits)
        {
            Rect panel = DrawCenteredPanel(560f, 360f);
            GUI.Label(PanelRect(panel, 90f, 22f, 380f, 60f), "Creditos", titleStyle);
            GUI.Label(PanelRect(panel, 30f, 92f, 500f, 200f), "Proyecto VJ - FIB UPC\nCarolina Rouj y Mateus\nReferencia: Looty Dungeon\nArte low-poly procedural sobre assets del repositorio.\nMusica/SFX generados en runtime con tonos sintetizados.\nIluminacion y ambiente diferenciados por seccion.", bodyStyle);
            if (GUI.Button(PanelCenteredRect(panel, 296f, 220f, 46f), "Volver", buttonStyle)) ShowMenu();
            return;
        }

        DrawHud();
        DrawDamageOverlay();
        DrawTransitionFade();

        if (State == DungeonState.GameOver)
        {
            DrawEndPanel("Has perdido", BuildEndSubtitle());
        }
        else if (State == DungeonState.Victory)
        {
            DrawEndPanel("Victoria", BuildEndSubtitle());
        }
        else if (State == DungeonState.Paused)
        {
            DrawPausePanel();
        }
    }

    private string BuildEndSubtitle()
    {
        string time = FormatSeconds(RunSeconds);
        string stats = "Sala " + (CurrentLevelIndex + 1) + "/10  -  Monedas " + Coins + "  -  KO " + EnemiesDefeated + "  -  Tiempo " + time;
        string best = (BestCoins > 0 || BestVictorySeconds > 0f)
            ? "\nMejor: " + BestCoins + " monedas" + (BestVictorySeconds > 0f ? "  -  Victoria " + FormatSeconds(BestVictorySeconds) : "")
            : "";
        return stats + best;
    }

    private void DrawPausePanel()
    {
        Rect dim = new Rect(0f, 0f, Screen.width, Screen.height);
        Color previous = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(dim, Texture2D.whiteTexture);
        GUI.color = previous;

        Rect panel = DrawCenteredPanel(500f, 320f);
        GUI.Label(PanelRect(panel, 20f, 22f, 460f, 62f), "Pausa", titleStyle);
        GUI.Label(PanelRect(panel, 30f, 90f, 440f, 70f), "Sala " + (CurrentLevelIndex + 1) + "/10  -  Vida " + Health + "/3  -  Monedas " + Coins + "\nEnter o P para continuar. M para menu.", bodyStyle);
        if (GUI.Button(PanelCenteredRect(panel, 174f, 220f, 46f), "Continuar", buttonStyle)) Resume();
        if (GUI.Button(PanelCenteredRect(panel, 234f, 220f, 46f), "Menu", buttonStyle)) ShowMenu();
    }

    private void DrawHud()
    {
        float scale = GetGuiScale();
        DrawHealthPips();
        float labelX = 118f * scale;
        float menuWidth = 88f * scale;
        float margin = 18f * scale;
        float labelWidth = Mathf.Max(190f * scale, Screen.width - labelX - menuWidth - margin * 2f);
        string timeText = FormatSeconds(RunSeconds);
        string enemiesText = aliveEnemies.Count > 0 ? "   Quedan: " + aliveEnemies.Count : "   Puerta abierta";
        GUI.Label(new Rect(labelX, 16f * scale, labelWidth, 42f * scale), "Vida: " + Health + "/3   Monedas: " + Coins + "   Salas: " + RoomsCleared + "/10   Sala: " + (CurrentLevelIndex + 1) + "/10" + enemiesText + "   " + timeText, hudStyle);
        if (GUI.Button(new Rect(Screen.width - menuWidth - margin, 18f * scale, menuWidth, 36f * scale), "Menu", buttonStyle)) ShowMenu();
        DrawRoomProgressBar(scale);
        if (Time.time < messageUntil)
        {
            float messageWidth = Mathf.Min(520f * scale, Screen.width - 32f * scale);
            GUI.Label(new Rect(Screen.width * 0.5f - messageWidth * 0.5f, 78f * scale, messageWidth, 40f * scale), message, hudStyle);
        }
        DrawSectionBanner(scale);
    }

    private void DrawSectionBanner(float scale)
    {
        if (Time.time >= sectionBannerUntil || string.IsNullOrEmpty(sectionBanner))
        {
            return;
        }

        float remaining = sectionBannerUntil - Time.time;
        float fade = Mathf.Clamp01(Mathf.Min(remaining, 0.6f) / 0.6f) * Mathf.Clamp01((2.8f - remaining) / 0.45f);
        if (fade <= 0.01f)
        {
            return;
        }

        float width = Mathf.Min(640f * scale, Screen.width - 40f * scale);
        float height = 64f * scale;
        Rect rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.32f, width, height);
        Color previous = GUI.color;
        GUI.color = new Color(0.04f, 0.02f, 0.04f, 0.78f * fade);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        Color accent = lastBannerSection == 3
            ? new Color(1f, 0.32f, 0.16f, fade)
            : new Color(1f, 0.84f, 0.32f, fade);
        GUI.color = accent;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3f * scale), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 3f * scale, rect.width, 3f * scale), Texture2D.whiteTexture);

        GUIStyle bannerStyle = new GUIStyle(titleStyle);
        bannerStyle.fontSize = Mathf.RoundToInt(28 * scale);
        Color textColor = bannerStyle.normal.textColor;
        bannerStyle.normal.textColor = new Color(textColor.r, textColor.g, textColor.b, fade);
        GUI.color = new Color(1f, 1f, 1f, fade);
        GUI.Label(rect, sectionBanner, bannerStyle);
        GUI.color = previous;
    }

    private void DrawHealthPips()
    {
        Color previous = GUI.color;
        float scale = GetGuiScale();
        for (int i = 0; i < 3; i++)
        {
            GUI.color = i < Health ? new Color(0.95f, 0.05f, 0.08f, 1f) : new Color(0.22f, 0.05f, 0.06f, 0.85f);
            GUI.DrawTexture(new Rect((22f + i * 30f) * scale, 24f * scale, 22f * scale, 22f * scale), Texture2D.whiteTexture);
        }
        GUI.color = previous;
    }

    private void DrawRoomProgressBar(float scale)
    {
        int totalRooms = levelNames.Length;
        float dotSize = 12f * scale;
        float spacing = 6f * scale;
        float bossExtra = 6f * scale;
        float totalWidth = totalRooms * dotSize + (totalRooms - 1) * spacing + bossExtra;
        float maxWidth = Screen.width - 36f * scale;
        if (totalWidth > maxWidth)
        {
            float ratio = maxWidth / totalWidth;
            dotSize *= ratio;
            spacing *= ratio;
            bossExtra *= ratio;
            totalWidth = totalRooms * dotSize + (totalRooms - 1) * spacing + bossExtra;
        }

        float originX = 18f * scale;
        float originY = 62f * scale;
        Color previous = GUI.color;

        Color cleared = new Color(0.38f, 0.85f, 0.32f, 0.95f);
        Color current = new Color(1f, 0.78f, 0.16f, 0.95f);
        Color future = new Color(0.18f, 0.13f, 0.12f, 0.82f);
        Color bossTint = new Color(0.95f, 0.16f, 0.12f, 0.92f);

        for (int i = 0; i < totalRooms; i++)
        {
            bool isBoss = i == totalRooms - 1;
            float x = originX + i * (dotSize + spacing);
            if (isBoss)
            {
                x += bossExtra;
            }

            float size = dotSize;
            float y = originY;
            bool isCurrent = State == DungeonState.Playing || State == DungeonState.Paused || State == DungeonState.GameOver
                ? i == CurrentLevelIndex
                : false;

            Color color;
            if (State == DungeonState.Victory || i < CurrentLevelIndex || (State == DungeonState.Playing && i < RoomsCleared))
            {
                color = isBoss && State == DungeonState.Victory ? bossTint : cleared;
            }
            else if (isCurrent)
            {
                float pulse = 0.85f + Mathf.PingPong(Time.unscaledTime * 1.6f, 0.15f);
                color = isBoss ? new Color(bossTint.r * pulse, bossTint.g * pulse, bossTint.b * pulse, bossTint.a) : new Color(current.r * pulse, current.g * pulse, current.b * pulse, current.a);
                size *= 1.18f;
                y -= size * 0.08f;
            }
            else
            {
                color = future;
            }

            GUI.color = color;
            GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
        }

        GUI.color = previous;
    }

    private void DrawTransitionFade()
    {
        if (transitionStartedAt < 0f)
        {
            return;
        }

        float fadeOutDuration = 0.18f;
        float fadeInDuration = 0.34f;
        float alpha;
        if (transitionFinishedAt < transitionStartedAt)
        {
            float elapsed = Time.time - transitionStartedAt;
            alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
        }
        else
        {
            float elapsed = Time.time - transitionFinishedAt;
            alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);
            if (alpha <= 0.01f)
            {
                transitionStartedAt = -1f;
                return;
            }
        }

        if (alpha <= 0.01f)
        {
            return;
        }

        Color previous = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void DrawDamageOverlay()
    {
        if (State != DungeonState.Playing && State != DungeonState.GameOver)
        {
            return;
        }

        float flash = Mathf.Clamp01((damageOverlayUntil - Time.time) / 0.42f);
        float lowHealthPulse = State == DungeonState.Playing && Health == 1
            ? 0.10f + Mathf.PingPong(Time.time * 1.9f, 0.07f)
            : 0f;
        float alpha = Mathf.Max(flash * 0.34f, lowHealthPulse);
        if (alpha <= 0f)
        {
            return;
        }

        float border = Mathf.Max(18f, Mathf.Min(Screen.width, Screen.height) * 0.035f);
        Color previous = GUI.color;
        GUI.color = new Color(1f, 0.04f, 0.02f, alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, border), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(0f, Screen.height - border, Screen.width, border), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(0f, 0f, border, Screen.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(Screen.width - border, 0f, border, Screen.height), Texture2D.whiteTexture);
        if (flash > 0f)
        {
            GUI.color = new Color(1f, 0.04f, 0.02f, flash * 0.08f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        }
        GUI.color = previous;
    }

    private void DrawEndPanel(string title, string subtitle)
    {
        Rect panel = DrawCenteredPanel(540f, 380f);
        GUI.Label(PanelRect(panel, 50f, 18f, 440f, 60f), title, titleStyle);
        GUI.Label(PanelRect(panel, 30f, 86f, 480f, 110f), subtitle, bodyStyle);
        string hint = State == DungeonState.Victory ? "Enter o M: menu" : "R o Enter: reintentar  -  M: menu";
        GUI.Label(PanelRect(panel, 30f, 202f, 480f, 36f), hint, bodyStyle);
        if (GUI.Button(PanelCenteredRect(panel, 246f, 220f, 46f), "Reintentar", buttonStyle)) LoadLevelIndex(CurrentLevelIndex, true);
        if (GUI.Button(PanelCenteredRect(panel, 304f, 220f, 46f), "Menu", buttonStyle)) ShowMenu();
    }

    private Rect DrawCenteredPanel(float width, float height)
    {
        float scale = GetGuiScale();
        float scaledWidth = Mathf.Min(width * scale, Screen.width - 24f * scale);
        float scaledHeight = Mathf.Min(height * scale, Screen.height - 24f * scale);
        Rect rect = new Rect(
            Screen.width * 0.5f - scaledWidth * 0.5f,
            Screen.height * 0.5f - scaledHeight * 0.5f,
            scaledWidth,
            scaledHeight);

        Color previous = GUI.color;
        GUI.color = new Color(0.12f, 0.06f, 0.1f, 0.9f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
        return rect;
    }

    private Rect PanelRect(Rect panel, float x, float y, float width, float height)
    {
        float scale = GetGuiScale();
        return new Rect(panel.x + x * scale, panel.y + y * scale, width * scale, height * scale);
    }

    private Rect PanelCenteredRect(Rect panel, float y, float width, float height)
    {
        float scale = GetGuiScale();
        float scaledWidth = width * scale;
        return new Rect(panel.x + panel.width * 0.5f - scaledWidth * 0.5f, panel.y + y * scale, scaledWidth, height * scale);
    }

    private float GetGuiScale()
    {
        return Mathf.Clamp(Mathf.Min(Screen.width / 960f, Screen.height / 540f), 0.72f, 1.18f);
    }

    private bool ShouldRunStandaloneSmoke()
    {
        if (standaloneSmokeStarted)
        {
            return false;
        }

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-vjSmokeTest")
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator RunStandaloneSmoke()
    {
        yield return null;
        yield return null;

        Debug.Log("[DungeonStandaloneSmoke] Starting");
        StartNewGame();

        float deadline = Time.realtimeSinceStartup + 120f;
        for (int expectedLevel = 0; expectedLevel < levelNames.Length; expectedLevel++)
        {
            while (!IsPlaying || CurrentLevelIndex != expectedLevel || player == null || !player.gameObject.activeInHierarchy || door == null)
            {
                if (StandaloneTimedOut(deadline, "waiting for level " + (expectedLevel + 1)))
                {
                    yield break;
                }
                yield return null;
            }

            if (door.IsOpen)
            {
                FinishStandaloneSmoke(1, "Door should start closed in level " + (expectedLevel + 1));
                yield break;
            }

            int enemies = DefeatAllEnemiesForStandaloneSmoke(12);
            if (enemies < 1)
            {
                FinishStandaloneSmoke(1, "Level " + (expectedLevel + 1) + " has no enemies.");
                yield break;
            }

            while (door != null && !door.IsOpen)
            {
                if (StandaloneTimedOut(deadline, "waiting for door in level " + (expectedLevel + 1)))
                {
                    yield break;
                }
                yield return null;
            }

            if (!DefeatedEnemiesAreHarmlessForStandaloneSmoke())
            {
                yield break;
            }

            Debug.Log("[DungeonStandaloneSmoke] Cleared level " + (expectedLevel + 1) + " enemies=" + enemies);
            DoorEntered();

            if (expectedLevel < levelNames.Length - 1)
            {
                while (!IsPlaying || CurrentLevelIndex != expectedLevel + 1)
                {
                    if (StandaloneTimedOut(deadline, "waiting for next level after " + (expectedLevel + 1)))
                    {
                        yield break;
                    }
                    yield return null;
                }
            }
        }

        while (State != DungeonState.Victory)
        {
            if (StandaloneTimedOut(deadline, "waiting for victory"))
            {
                yield break;
            }
            yield return null;
        }

        if (RoomsCleared != levelNames.Length)
        {
            FinishStandaloneSmoke(1, "Victory should mark " + levelNames.Length + " rooms, got " + RoomsCleared);
            yield break;
        }

        Debug.Log("[DungeonStandaloneSmoke] OK");
        FinishStandaloneSmoke(0, "");
    }

    private bool StandaloneTimedOut(float deadline, string label)
    {
        if (Time.realtimeSinceStartup <= deadline)
        {
            return false;
        }

        FinishStandaloneSmoke(1, "Timeout " + label);
        return true;
    }

    private int DefeatAllEnemiesForStandaloneSmoke(int hitsPerEnemy)
    {
        HashSet<GameObject> enemies = FindEnemyObjectsForStandaloneSmoke();
        PlayerCombat combat = player.GetComponent<PlayerCombat>();
        if (combat == null)
        {
            FinishStandaloneSmoke(1, "Player is missing PlayerCombat.");
            return 0;
        }

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            player.ResetAt(enemy.transform.position + Vector3.back * 1.05f);
            Physics.SyncTransforms();
            for (int hit = 0; hit < hitsPerEnemy; hit++)
            {
                combat.Attack();
            }
        }

        return enemies.Count;
    }

    private bool DefeatedEnemiesAreHarmlessForStandaloneSmoke()
    {
        foreach (GameObject enemy in FindEnemyObjectsForStandaloneSmoke())
        {
            if (enemy == null)
            {
                continue;
            }

            foreach (EnemyTouchDamage damage in enemy.GetComponents<EnemyTouchDamage>())
            {
                if (damage.enabled)
                {
                    FinishStandaloneSmoke(1, enemy.name + " kept EnemyTouchDamage enabled after death.");
                    return false;
                }
            }

            foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
            {
                if (collider.enabled)
                {
                    FinishStandaloneSmoke(1, enemy.name + " kept collider enabled after death: " + collider.name);
                    return false;
                }
            }
        }

        return true;
    }

    private static HashSet<GameObject> FindEnemyObjectsForStandaloneSmoke()
    {
        HashSet<GameObject> enemies = new HashSet<GameObject>();
        foreach (Slime enemy in FindObjectsByType<Slime>()) enemies.Add(enemy.gameObject);
        foreach (Bat enemy in FindObjectsByType<Bat>()) enemies.Add(enemy.gameObject);
        foreach (Wizard enemy in FindObjectsByType<Wizard>()) enemies.Add(enemy.gameObject);
        foreach (Gnome enemy in FindObjectsByType<Gnome>()) enemies.Add(enemy.gameObject);
        foreach (Boss enemy in FindObjectsByType<Boss>()) enemies.Add(enemy.gameObject);
        return enemies;
    }

    private void FinishStandaloneSmoke(int exitCode, string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("[DungeonStandaloneSmoke] FAILED " + error);
        }

        Application.Quit(exitCode);
    }

    private void EnsureGuiStyles()
    {
        float scale = GetGuiScale();
        if (titleStyle != null && Mathf.Abs(scale - guiScale) < 0.01f)
        {
            return;
        }

        guiScale = scale;
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(42 * scale),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(18 * scale),
            wordWrap = true,
            normal = { textColor = new Color(1f, 0.86f, 0.48f) }
        };

        hudStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(22 * scale),
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.RoundToInt(20 * scale),
            fontStyle = FontStyle.Bold
        };
    }
}

public class PlayerMovement : MonoBehaviour
{
    public Vector3 Facing { get; private set; } = Vector3.forward;
    public bool IsSlowed => Time.time <= slowUntil;
    public float CurrentSpeedMultiplier => IsSlowed ? slowFactor : 1f;
    public bool IsDashing => Time.time <= dashUntil;
    public float DashCooldownRemaining => Mathf.Max(0f, nextDashTime - Time.time);

    private CharacterController controller;
    private float speed = 4.4f;
    private float slowUntil;
    private float slowFactor = 1f;
    private float fallGraceUntil;
    private Vector3 dashDirection;
    private float dashUntil;
    private float nextDashTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void ResetAt(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position + Vector3.up * 0.05f;
        transform.rotation = Quaternion.LookRotation(Vector3.forward);
        Facing = Vector3.forward;
        slowUntil = 0f;
        slowFactor = 1f;
        dashUntil = 0f;
        nextDashTime = 0f;
        dashDirection = Vector3.zero;
        controller.enabled = true;
        fallGraceUntil = Time.time + 0.6f;
    }

    public void ApplySlow(float factor, float duration)
    {
        slowFactor = Mathf.Min(slowFactor, factor);
        slowUntil = Mathf.Max(slowUntil, Time.time + duration);
    }

    private void Update()
    {
        if (!DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        Vector3 input = ReadMovement();
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        if (input.sqrMagnitude > 0.01f)
        {
            Facing = input.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Facing), 16f * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            TryDash(input.sqrMagnitude > 0.01f ? input : Facing);
        }

        float multiplier = CurrentSpeedMultiplier;
        if (Time.time > slowUntil)
        {
            slowFactor = 1f;
        }

        if (IsDashing)
        {
            controller.Move(dashDirection * (11.5f * Time.deltaTime));
        }
        else
        {
            controller.Move(input * (speed * multiplier * Time.deltaTime));
        }
        controller.Move(Vector3.down * 3f * Time.deltaTime);

        LevelManager manager = LevelManager.Instance;
        if (manager != null && Time.time > fallGraceUntil && !manager.HasFloorAt(transform.position))
        {
            fallGraceUntil = Time.time + 0.7f;
            DungeonGameRuntime.Instance.RespawnPlayerAfterFall();
        }
    }

    public bool TryDash(Vector3 direction)
    {
        if (DungeonGameRuntime.Instance == null || !DungeonGameRuntime.Instance.IsPlaying || Time.time < nextDashTime)
        {
            return false;
        }

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
        {
            return false;
        }

        dashDirection = direction.normalized;
        Facing = dashDirection;
        transform.rotation = Quaternion.LookRotation(Facing);
        dashUntil = Time.time + 0.14f;
        nextDashTime = Time.time + 0.82f;
        fallGraceUntil = Time.time + 0.24f;

        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.PlayDash(transform.position, dashDirection);
        }

        return true;
    }

    private static Vector3 ReadMovement()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;

        return new Vector3(x, 0f, z);
    }
}

public class PlayerCombat : MonoBehaviour
{
    private readonly Collider[] hits = new Collider[24];
    private readonly HashSet<GameObject> damagedThisSwing = new HashSet<GameObject>();
    private PlayerMovement movement;
    private float nextAttackTime;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    public void Attack()
    {
        nextAttackTime = Time.time + 0.34f;
        damagedThisSwing.Clear();

        Vector3 center = transform.position + Vector3.up * 0.75f + movement.Facing * 0.75f;
        int count = Physics.OverlapSphereNonAlloc(center, 0.85f, hits);
        bool hitEnemy = false;

        for (int i = 0; i < count; i++)
        {
            Collider hit = hits[i];
            if (hit == null || hit.transform == transform)
            {
                continue;
            }

            GameObject root = FindEnemyRoot(hit.transform);
            if (root == null || damagedThisSwing.Contains(root))
            {
                continue;
            }

            Vector3 toTarget = root.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.01f && Vector3.Dot(movement.Facing, toTarget.normalized) < -0.15f)
            {
                continue;
            }

            damagedThisSwing.Add(root);
            root.SendMessage("Hurt", SendMessageOptions.DontRequireReceiver);
            hitEnemy = true;
        }

        DungeonGameRuntime.Instance.PlayAttackSfx(hitEnemy);
        RuntimeVfx.Slash(center, movement.Facing, hitEnemy ? Color.yellow : Color.white);
    }

    private static GameObject FindEnemyRoot(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.GetComponent<Slime>() != null ||
                current.GetComponent<Bat>() != null ||
                current.GetComponent<Wizard>() != null ||
                current.GetComponent<Gnome>() != null ||
                current.GetComponent<Boss>() != null)
            {
                return current.gameObject;
            }
            current = current.parent;
        }

        return null;
    }
}

public class PlayerHealth : MonoBehaviour
{
    private float invulnerableUntil;
    private Renderer[] renderers;

    private void Awake()
    {
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        if (Time.time < invulnerableUntil)
        {
            return;
        }

        invulnerableUntil = Time.time + 0.8f;
        DungeonGameRuntime.Instance.CommitPlayerDamage(amount);
        RuntimeVfx.Burst(transform.position + Vector3.up, Color.red, 18, 0.45f);
        StartCoroutine(Flash());

        Vector3 away = transform.position - sourcePosition;
        away.y = 0f;
        if (away.sqrMagnitude > 0.01f)
        {
            CharacterController controller = GetComponent<CharacterController>();
            controller.Move(away.normalized * 0.35f);
        }
    }

    private IEnumerator Flash()
    {
        float end = Time.time + 0.55f;
        while (Time.time < end)
        {
            bool visible = Mathf.FloorToInt(Time.time * 18f) % 2 == 0;
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }
            yield return null;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }
}

public class EnemyHitFeedback : MonoBehaviour
{
    private Renderer[] renderers;
    private MaterialPropertyBlock flashBlock;
    private Coroutine flashRoutine;
    private Transform barRoot;
    private Transform barFill;
    private int maxHealth = 1;

    public static EnemyHitFeedback Ensure(GameObject enemy)
    {
        EnemyHitFeedback feedback = enemy.GetComponent<EnemyHitFeedback>();
        if (feedback == null)
        {
            feedback = enemy.AddComponent<EnemyHitFeedback>();
        }

        feedback.CacheRenderers();
        return feedback;
    }

    private void Awake()
    {
        CacheRenderers();
    }

    private void Update()
    {
        if (barRoot != null && Camera.main != null)
        {
            barRoot.rotation = Camera.main.transform.rotation;
        }
    }

    public void SetHealth(int currentHealth, int configuredMaxHealth)
    {
        maxHealth = Mathf.Max(1, configuredMaxHealth);
        if (maxHealth <= 1)
        {
            return;
        }

        EnsureHealthBar();
        UpdateHealthBar(currentHealth);
    }

    public void Hit(int currentHealth, int configuredMaxHealth)
    {
        SetHealth(currentHealth, configuredMaxHealth);
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.PlayEnemyHit(transform.position);
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(Flash());
    }

    private void CacheRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();
        if (flashBlock == null)
        {
            flashBlock = new MaterialPropertyBlock();
        }
    }

    private IEnumerator Flash()
    {
        CacheRenderers();
        float end = Time.time + 0.18f;
        Color flashColor = new Color(1f, 0.95f, 0.35f);

        while (Time.time < end)
        {
            bool visible = Mathf.FloorToInt(Time.time * 40f) % 2 == 0;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                if (visible)
                {
                    flashBlock.Clear();
                    flashBlock.SetColor("_BaseColor", flashColor);
                    flashBlock.SetColor("_Color", flashColor);
                    renderer.SetPropertyBlock(flashBlock);
                }
                else
                {
                    renderer.SetPropertyBlock(null);
                }
            }

            yield return null;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.SetPropertyBlock(null);
            }
        }
    }

    private void EnsureHealthBar()
    {
        if (barRoot != null)
        {
            return;
        }

        barRoot = new GameObject("Enemy Health Bar").transform;
        barRoot.SetParent(transform, false);
        barRoot.localPosition = Vector3.up * GetBarHeight();

        CreateBarPart("Bar Back", barRoot, new Vector3(0f, 0f, 0.01f), new Vector3(0.92f, 0.09f, 0.04f), new Color(0.08f, 0.05f, 0.05f));
        barFill = CreateBarPart("Bar Fill", barRoot, Vector3.zero, new Vector3(0.86f, 0.07f, 0.045f), new Color(0.25f, 0.95f, 0.28f)).transform;
    }

    private void UpdateHealthBar(int currentHealth)
    {
        if (barFill == null)
        {
            return;
        }

        float fraction = Mathf.Clamp01(currentHealth / (float)maxHealth);
        barFill.localScale = new Vector3(0.86f * fraction, 0.07f, 0.045f);
        barFill.localPosition = new Vector3(-0.43f * (1f - fraction), 0f, 0f);
    }

    private float GetBarHeight()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            return Mathf.Max(1.2f, collider.bounds.size.y + 0.35f);
        }

        return 1.45f;
    }

    private static GameObject CreateBarPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = RuntimeMaterials.Get("enemy_health_" + name, color);
        Object.Destroy(part.GetComponent<Collider>());
        return part;
    }
}

public class EnemyTouchDamage : MonoBehaviour
{
    private float nextHitTime;

    public static void Ensure(GameObject enemy)
    {
        EnemyTouchDamage damage = enemy.GetComponent<EnemyTouchDamage>();
        if (damage == null)
        {
            damage = enemy.AddComponent<EnemyTouchDamage>();
        }

        SphereCollider trigger = enemy.GetComponent<SphereCollider>();
        if (trigger == null)
        {
            trigger = enemy.AddComponent<SphereCollider>();
        }
        trigger.isTrigger = true;
        trigger.radius = 0.62f;
        trigger.center = Vector3.up * 0.55f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < nextHitTime || !other.CompareTag("Player"))
        {
            return;
        }

        nextHitTime = Time.time + 1.0f;
        DungeonGameRuntime.Instance.DamagePlayer(1, transform.position);
    }

    private void Update()
    {
        if (DungeonGameRuntime.Instance == null || !DungeonGameRuntime.Instance.IsPlaying || Time.time < nextHitTime)
        {
            return;
        }

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            return;
        }

        Vector3 delta = player.transform.position - transform.position;
        delta.y = 0f;
        if (delta.sqrMagnitude <= 0.75f * 0.75f)
        {
            nextHitTime = Time.time + 1.0f;
            DungeonGameRuntime.Instance.DamagePlayer(1, transform.position);
        }
    }
}

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private Color impactColor;
    private float hitRadius;
    private float expiresAt;

    public static void Launch(Vector3 position, Vector3 direction, float speed, Color color, float size)
    {
        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Enemy Projectile";
        projectile.transform.position = position;
        projectile.transform.localScale = Vector3.one * size;
        projectile.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("enemy_projectile_" + color, color, 1.6f);

        Collider collider = projectile.GetComponent<Collider>();
        collider.isTrigger = true;

        Rigidbody body = projectile.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        EnemyProjectile enemyProjectile = projectile.AddComponent<EnemyProjectile>();
        enemyProjectile.Initialize(direction.normalized * speed, color, Mathf.Max(0.24f, size * 0.8f));
    }

    private void Initialize(Vector3 launchVelocity, Color color, float radius)
    {
        velocity = launchVelocity;
        impactColor = color;
        hitRadius = radius;
        expiresAt = Time.time + 3.2f;
    }

    private void Update()
    {
        if (!EnemyMovementUtility.IsGameplayActive())
        {
            Destroy(gameObject);
            return;
        }

        transform.position += velocity * Time.deltaTime;
        transform.Rotate(120f * Time.deltaTime, 180f * Time.deltaTime, 80f * Time.deltaTime, Space.World);

        LevelManager manager = LevelManager.Instance;
        if (manager != null && (!manager.IsInBounds(transform.position) || !manager.HasFloorAt(transform.position)))
        {
            Expire();
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.activeInHierarchy)
        {
            Vector3 delta = player.transform.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude <= hitRadius * hitRadius)
            {
                HitPlayer(player.transform.position);
                return;
            }
        }

        if (Time.time >= expiresAt)
        {
            Expire();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HitPlayer(other.transform.position);
        }
    }

    private void HitPlayer(Vector3 playerPosition)
    {
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.DamagePlayer(1, transform.position);
        }
        RuntimeVfx.Burst(playerPosition + Vector3.up * 0.55f, impactColor, 14, 0.3f);
        Destroy(gameObject);
    }

    private void Expire()
    {
        RuntimeVfx.Burst(transform.position, impactColor, 6, 0.2f);
        Destroy(gameObject);
    }
}

public class HealthPickup : MonoBehaviour
{
    private bool collected;
    private float baseY;

    public static void Create(Transform parent, Vector3 position)
    {
        GameObject root = new GameObject("Heart");
        root.transform.SetParent(parent, false);
        root.transform.position = position + Vector3.up * 0.55f;
        root.transform.localScale = Vector3.one * 0.28f;

        SphereCollider collider = root.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.9f;

        Color heartColor = new Color(1f, 0.32f, 0.4f);
        GameObject lobeLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lobeLeft.name = "LobeLeft";
        lobeLeft.transform.SetParent(root.transform, false);
        lobeLeft.transform.localPosition = new Vector3(-0.32f, 0.2f, 0f);
        lobeLeft.transform.localScale = Vector3.one * 0.62f;
        Destroy(lobeLeft.GetComponent<Collider>());
        lobeLeft.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("heart", heartColor, 0.45f);

        GameObject lobeRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lobeRight.name = "LobeRight";
        lobeRight.transform.SetParent(root.transform, false);
        lobeRight.transform.localPosition = new Vector3(0.32f, 0.2f, 0f);
        lobeRight.transform.localScale = Vector3.one * 0.62f;
        Destroy(lobeRight.GetComponent<Collider>());
        lobeRight.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("heart", heartColor, 0.45f);

        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
        point.name = "Point";
        point.transform.SetParent(root.transform, false);
        point.transform.localPosition = new Vector3(0f, -0.24f, 0f);
        point.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        point.transform.localScale = new Vector3(0.62f, 0.62f, 0.62f);
        Destroy(point.GetComponent<Collider>());
        point.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("heart", heartColor, 0.45f);

        HealthPickup pickup = root.AddComponent<HealthPickup>();
        pickup.baseY = root.transform.position.y;
    }

    private void Update()
    {
        if (collected || !EnemyMovementUtility.IsGameplayActive())
        {
            return;
        }

        transform.Rotate(0f, 110f * Time.deltaTime, 0f, Space.World);
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * 3.4f) * 0.07f;
        transform.position = p;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.AddHealth(transform.position);
        }
        Destroy(gameObject);
    }
}

public class CoinPickup : MonoBehaviour
{
    private bool collected;
    private float baseY;

    public static void Create(Transform parent, Vector3 position)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.transform.SetParent(parent, false);
        coin.transform.position = position + Vector3.up * 0.45f;
        coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        coin.transform.localScale = new Vector3(0.24f, 0.05f, 0.24f);
        coin.GetComponent<Renderer>().material = RuntimeMaterials.Get("coin", new Color(1f, 0.78f, 0.08f));
        Collider collider = coin.GetComponent<Collider>();
        collider.isTrigger = true;
        CoinPickup pickup = coin.AddComponent<CoinPickup>();
        pickup.baseY = coin.transform.position.y;
    }

    private void Update()
    {
        if (collected || !EnemyMovementUtility.IsGameplayActive())
        {
            return;
        }

        transform.Rotate(0f, 0f, 180f * Time.deltaTime, Space.Self);
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * 4f) * 0.08f;
        transform.position = p;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        DungeonGameRuntime.Instance.AddCoin(transform.position);
        Destroy(gameObject);
    }
}

public class DungeonDoor : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    private Renderer doorRenderer;
    private Collider doorCollider;

    private void Awake()
    {
        doorRenderer = GetComponent<Renderer>();
        doorCollider = GetComponent<Collider>();
        doorRenderer.material = RuntimeMaterials.Get("door_closed", new Color(0.45f, 0.16f, 0.08f));
        doorCollider.isTrigger = false;
    }

    public void Open()
    {
        IsOpen = true;
        doorRenderer.material = RuntimeMaterials.Get("door_open", new Color(1f, 0.73f, 0.08f));
        doorCollider.isTrigger = true;
        StartCoroutine(OpenAnimation());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOpen && other.CompareTag("Player"))
        {
            DungeonGameRuntime.Instance.DoorEntered();
        }
    }

    private IEnumerator OpenAnimation()
    {
        Vector3 start = transform.localScale;
        Vector3 end = new Vector3(start.x, start.y, 0.04f);
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, end, elapsed / 0.25f);
            yield return null;
        }
    }
}

public class RuntimeTrap : MonoBehaviour
{
    private string trapType;
    private bool active = true;
    private float nextToggle;
    private Transform bladePivot;
    private Renderer[] renderers;

    public static GameObject Create(string type, Vector3 position, Transform parent)
    {
        if (type == "SpiderWebs")
        {
            return CreateSpiderWebs(position, parent);
        }

        if (type == "RetractileFork")
        {
            return CreateRetractileFork(position, parent);
        }

        if (type != "Spikes" && type != "Flame" && type != "Blade")
        {
            return null;
        }

        GameObject trap = new GameObject(type + " Trap");
        trap.transform.SetParent(parent, false);
        trap.transform.position = position + Vector3.up * 0.08f;
        RuntimeTrap runtimeTrap = trap.AddComponent<RuntimeTrap>();
        runtimeTrap.Initialize(type);
        return trap;
    }

    private static GameObject CreateSpiderWebs(Vector3 position, Transform parent)
    {
        GameObject trap = new GameObject("SpiderWebs Trap");
        trap.transform.SetParent(parent, false);
        trap.transform.position = position + Vector3.up * 0.05f;

        BoxCollider trigger = trap.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(0.95f, 0.28f, 0.95f);
        trigger.center = new Vector3(0f, 0.16f, 0f);

        CreateStaticPart(trap.transform, "Web Plate", PrimitiveType.Cube, Vector3.zero, new Vector3(0.94f, 0.04f, 0.94f), Quaternion.identity, new Color(0.10f, 0.12f, 0.14f));
        CreateStaticPart(trap.transform, "Web Strand A", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(0.9f, 0.035f, 0.055f), Quaternion.Euler(0f, 28f, 0f), new Color(0.78f, 0.92f, 1f));
        CreateStaticPart(trap.transform, "Web Strand B", PrimitiveType.Cube, new Vector3(0f, 0.09f, 0f), new Vector3(0.9f, 0.035f, 0.055f), Quaternion.Euler(0f, -28f, 0f), new Color(0.78f, 0.92f, 1f));
        CreateStaticPart(trap.transform, "Web Strand C", PrimitiveType.Cube, new Vector3(0f, 0.1f, 0f), new Vector3(0.72f, 0.035f, 0.045f), Quaternion.Euler(0f, 90f, 0f), new Color(0.78f, 0.92f, 1f));
        trap.AddComponent<SpiderWebs>();
        return trap;
    }

    private static GameObject CreateRetractileFork(Vector3 position, Transform parent)
    {
        GameObject trap = new GameObject("RetractileFork Trap");
        trap.transform.SetParent(parent, false);
        trap.transform.position = position + Vector3.up * 0.18f;

        BoxCollider trigger = trap.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.1f, 0.42f, 0.42f);
        trigger.center = new Vector3(0.28f, 0.02f, 0f);

        CreateStaticPart(trap.transform, "Fork Handle", PrimitiveType.Cube, new Vector3(-0.22f, 0f, 0f), new Vector3(0.55f, 0.12f, 0.12f), Quaternion.identity, new Color(0.5f, 0.46f, 0.36f));
        CreateStaticPart(trap.transform, "Fork Prong A", PrimitiveType.Cube, new Vector3(0.18f, 0.08f, 0.13f), new Vector3(0.42f, 0.06f, 0.045f), Quaternion.identity, new Color(0.82f, 0.8f, 0.68f));
        CreateStaticPart(trap.transform, "Fork Prong B", PrimitiveType.Cube, new Vector3(0.18f, 0.08f, 0f), new Vector3(0.42f, 0.06f, 0.045f), Quaternion.identity, new Color(0.82f, 0.8f, 0.68f));
        CreateStaticPart(trap.transform, "Fork Prong C", PrimitiveType.Cube, new Vector3(0.18f, 0.08f, -0.13f), new Vector3(0.42f, 0.06f, 0.045f), Quaternion.identity, new Color(0.82f, 0.8f, 0.68f));

        RetractileFork fork = trap.AddComponent<RetractileFork>();
        fork.extendDistance = 0.65f;
        fork.idleDuration = 1.2f;
        return trap;
    }

    private static void CreateStaticPart(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = RuntimeMaterials.Get("trap_" + name + color, color);
        Object.Destroy(part.GetComponent<Collider>());
    }

    private void Initialize(string type)
    {
        trapType = type;

        if (trapType == "Spikes")
        {
            BuildSpikes();
            nextToggle = Time.time + Random.Range(0.3f, 0.9f);
        }
        else if (trapType == "Flame")
        {
            BuildFlame();
            active = false;
            nextToggle = Time.time + Random.Range(0.5f, 1.1f);
        }
        else
        {
            BuildBlade();
            active = true;
        }

        renderers = GetComponentsInChildren<Renderer>();
        UpdateVisualState();
    }

    private void Update()
    {
        if (DungeonGameRuntime.Instance == null || !DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        if (trapType == "Blade")
        {
            active = true;
            if (bladePivot != null)
            {
                bladePivot.Rotate(0f, 180f * Time.deltaTime, 0f, Space.World);
            }
            return;
        }

        if (Time.time >= nextToggle)
        {
            active = !active;
            nextToggle = Time.time + (active ? 1.0f : 1.35f);
            UpdateVisualState();
            if (active)
            {
                DungeonGameRuntime.Instance.PlayTrapActivated(transform.position, trapType);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!active || !other.CompareTag("Player"))
        {
            return;
        }

        DungeonGameRuntime.Instance.DamagePlayer(1, transform.position);
    }

    private void BuildSpikes()
    {
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(0.95f, 0.45f, 0.95f);
        trigger.center = new Vector3(0f, 0.25f, 0f);

        CreatePart("Spike Plate", PrimitiveType.Cube, Vector3.zero, new Vector3(0.95f, 0.08f, 0.95f), new Color(0.16f, 0.14f, 0.13f));
        for (int i = 0; i < 4; i++)
        {
            Vector3 local = new Vector3(i % 2 == 0 ? -0.24f : 0.24f, 0.18f, i < 2 ? -0.24f : 0.24f);
            CreatePart("Spike", PrimitiveType.Cylinder, local, new Vector3(0.12f, 0.24f, 0.12f), new Color(0.82f, 0.78f, 0.67f));
        }
    }

    private void BuildFlame()
    {
        CapsuleCollider trigger = gameObject.AddComponent<CapsuleCollider>();
        trigger.isTrigger = true;
        trigger.height = 1.2f;
        trigger.radius = 0.42f;
        trigger.center = new Vector3(0f, 0.45f, 0f);

        CreatePart("Brazier", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.46f, 0.14f, 0.46f), new Color(0.12f, 0.1f, 0.09f));
        CreatePart("Flame", PrimitiveType.Capsule, new Vector3(0f, 0.48f, 0f), new Vector3(0.28f, 0.46f, 0.28f), new Color(1f, 0.32f, 0.05f));
    }

    private void BuildBlade()
    {
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.95f;
        trigger.center = new Vector3(0f, 0.28f, 0f);

        bladePivot = new GameObject("Blade Pivot").transform;
        bladePivot.SetParent(transform, false);
        CreatePart("Blade A", PrimitiveType.Cube, new Vector3(0.38f, 0.3f, 0f), new Vector3(0.85f, 0.1f, 0.16f), new Color(0.82f, 0.8f, 0.68f), bladePivot);
        CreatePart("Blade B", PrimitiveType.Cube, new Vector3(-0.38f, 0.3f, 0f), new Vector3(0.85f, 0.1f, 0.16f), new Color(0.82f, 0.8f, 0.68f), bladePivot);
    }

    private void CreatePart(string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
    {
        CreatePart(name, type, localPosition, localScale, color, transform);
    }

    private void CreatePart(string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color, Transform parent)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = RuntimeMaterials.Get("trap_" + name + color, color);
        Object.Destroy(part.GetComponent<Collider>());
    }

    private void UpdateVisualState()
    {
        if (renderers == null) return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            bool persistent = renderer.name.Contains("Plate") || renderer.name.Contains("Brazier");
            renderer.enabled = active || persistent;
        }
    }
}

public class CameraFollowDungeon : MonoBehaviour
{
    private Transform target;
    private Vector3 staticFocus;
    private bool useStaticFocus;
    private Vector3 offset = new Vector3(3.92f, 10.18f, -5.01f);
    private float shakeUntil;
    private float shakePower;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        useStaticFocus = false;
        Snap();
    }

    public void SetStaticFocus(Vector3 focus)
    {
        target = null;
        staticFocus = focus;
        useStaticFocus = true;
        Snap();
    }

    public void Snap()
    {
        if (TryGetFocus(out Vector3 focus))
        {
            transform.position = focus + offset;
        }
    }

    public void Shake(float duration, float power)
    {
        shakeUntil = Mathf.Max(shakeUntil, Time.time + duration);
        shakePower = Mathf.Max(shakePower, power);
    }

    private void LateUpdate()
    {
        if (!TryGetFocus(out Vector3 focus))
        {
            return;
        }

        Vector3 desired = focus + offset;
        if (Time.time < shakeUntil)
        {
            desired += Random.insideUnitSphere * shakePower;
        }
        else
        {
            shakePower = 0f;
        }

        transform.position = Vector3.Lerp(transform.position, desired, 8f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(58f, -38f, 0f);
    }

    private bool TryGetFocus(out Vector3 focus)
    {
        if (useStaticFocus)
        {
            focus = staticFocus;
            return true;
        }

        if (target != null)
        {
            focus = target.position;
            return true;
        }

        focus = Vector3.zero;
        return false;
    }
}

public static class EnemyMovementUtility
{
    public static bool IsGameplayActive()
    {
        return DungeonGameRuntime.Instance == null || DungeonGameRuntime.Instance.IsPlaying;
    }

    public static void MoveForwardKeepingFloor(Transform transform, float distance)
    {
        Vector3 previousPosition = transform.position;
        transform.Translate(Vector3.forward * distance);

        LevelManager manager = LevelManager.Instance;
        if (manager == null)
        {
            return;
        }

        if (!manager.IsInBounds(transform.position) || !manager.HasFloorAt(transform.position))
        {
            transform.position = previousPosition;
        }
    }

    public static void DisableEnemyAfterDeath(GameObject enemy)
    {
        foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        foreach (EnemyTouchDamage damage in enemy.GetComponents<EnemyTouchDamage>())
        {
            damage.enabled = false;
        }
    }
}

public static class PlayerVisualBuilder
{
    public static void Build(Transform root)
    {
        if (root.Find("RuntimePlayerVisual") != null)
        {
            return;
        }

        Transform visual = new GameObject("RuntimePlayerVisual").transform;
        visual.SetParent(root, false);

        CreatePart(visual, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.66f, 0f), new Vector3(0.55f, 0.7f, 0.55f), new Color(0.22f, 0.55f, 0.92f));
        CreatePart(visual, PrimitiveType.Cube, "Helmet", new Vector3(0f, 1.32f, 0f), new Vector3(0.48f, 0.25f, 0.48f), new Color(0.86f, 0.84f, 0.72f));
        CreatePart(visual, PrimitiveType.Cube, "Cape", new Vector3(0f, 0.68f, -0.32f), new Vector3(0.48f, 0.7f, 0.08f), new Color(0.86f, 0.08f, 0.12f));
        CreatePart(visual, PrimitiveType.Cube, "Sword", new Vector3(0.48f, 0.78f, 0.22f), new Vector3(0.08f, 0.75f, 0.08f), new Color(0.88f, 0.86f, 0.74f));
    }

    private static void CreatePart(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = RuntimeMaterials.Get("player_" + name, color);
        Object.Destroy(part.GetComponent<Collider>());
    }
}

public static class RuntimeEnemyFactory
{
    public static GameObject Create(string type, Vector3 position)
    {
        GameObject enemy = new GameObject(type);
        enemy.transform.position = position;
        CapsuleCollider collider = enemy.AddComponent<CapsuleCollider>();
        collider.height = type == "Boss" ? 1.8f : 1.2f;
        collider.radius = type == "Boss" ? 0.65f : 0.38f;
        collider.center = Vector3.up * collider.height * 0.5f;

        switch (type)
        {
            case "Bat":
                enemy.AddComponent<Bat>();
                BuildBatVisual(enemy.transform);
                break;
            case "Wizard":
                enemy.AddComponent<Wizard>();
                BuildWizardVisual(enemy.transform);
                break;
            case "Gnome":
                enemy.AddComponent<Gnome>();
                BuildGnomeVisual(enemy.transform);
                break;
            case "Boss":
                enemy.AddComponent<Boss>();
                BuildBossVisual(enemy.transform);
                break;
            default:
                enemy.AddComponent<Gnome>();
                BuildGnomeVisual(enemy.transform);
                break;
        }

        return enemy;
    }

    private static Transform CreateVisualRoot(Transform root, float bobAmplitude, float bobSpeed)
    {
        Transform visualRoot = new GameObject("RuntimeEnemyVisual").transform;
        visualRoot.SetParent(root, false);
        RuntimeEnemyVisualAnimator animator = root.gameObject.AddComponent<RuntimeEnemyVisualAnimator>();
        animator.Initialize(visualRoot, bobAmplitude, bobSpeed);
        return visualRoot;
    }

    private static void BuildBatVisual(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.12f, 6f);
        CreatePart(visual, PrimitiveType.Capsule, "Bat Body", new Vector3(0f, 0.62f, 0f), new Vector3(0.46f, 0.38f, 0.46f), new Color(0.34f, 0.16f, 0.48f));
        CreatePart(visual, PrimitiveType.Cube, "Bat Wing L", new Vector3(-0.48f, 0.62f, 0f), new Vector3(0.7f, 0.08f, 0.28f), new Color(0.18f, 0.08f, 0.28f));
        CreatePart(visual, PrimitiveType.Cube, "Bat Wing R", new Vector3(0.48f, 0.62f, 0f), new Vector3(0.7f, 0.08f, 0.28f), new Color(0.18f, 0.08f, 0.28f));
        CreatePart(visual, PrimitiveType.Sphere, "Bat Eye L", new Vector3(-0.11f, 0.72f, 0.27f), Vector3.one * 0.09f, new Color(1f, 0.18f, 0.08f));
        CreatePart(visual, PrimitiveType.Sphere, "Bat Eye R", new Vector3(0.11f, 0.72f, 0.27f), Vector3.one * 0.09f, new Color(1f, 0.18f, 0.08f));
    }

    private static void BuildWizardVisual(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.05f, 3.5f);
        CreatePart(visual, PrimitiveType.Capsule, "Wizard Robe", new Vector3(0f, 0.72f, 0f), new Vector3(0.62f, 0.82f, 0.62f), new Color(0.38f, 0.12f, 0.72f));
        CreatePart(visual, PrimitiveType.Cylinder, "Wizard Brim", new Vector3(0f, 1.24f, 0f), new Vector3(0.62f, 0.08f, 0.62f), new Color(0.13f, 0.07f, 0.22f));
        CreatePart(visual, PrimitiveType.Cylinder, "Wizard Hat", new Vector3(0f, 1.48f, 0f), new Vector3(0.28f, 0.36f, 0.28f), new Color(0.22f, 0.08f, 0.42f));
        CreatePart(visual, PrimitiveType.Cube, "Wizard Staff", new Vector3(0.42f, 0.72f, 0.08f), new Vector3(0.07f, 0.95f, 0.07f), new Color(0.48f, 0.24f, 0.08f));
        CreatePart(visual, PrimitiveType.Sphere, "Wizard Orb", new Vector3(0.42f, 1.26f, 0.08f), Vector3.one * 0.18f, new Color(0.28f, 0.88f, 1f));
    }

    private static void BuildGnomeVisual(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.04f, 4.2f);
        CreatePart(visual, PrimitiveType.Capsule, "Gnome Body", new Vector3(0f, 0.58f, 0f), new Vector3(0.55f, 0.58f, 0.55f), new Color(0.82f, 0.28f, 0.12f));
        CreatePart(visual, PrimitiveType.Sphere, "Gnome Nose", new Vector3(0f, 0.72f, 0.32f), Vector3.one * 0.14f, new Color(0.95f, 0.58f, 0.4f));
        CreatePart(visual, PrimitiveType.Cylinder, "Gnome Hat", new Vector3(0f, 1.02f, 0f), new Vector3(0.28f, 0.38f, 0.28f), new Color(0.05f, 0.34f, 0.2f));
        CreatePart(visual, PrimitiveType.Cube, "Gnome Axe", new Vector3(-0.42f, 0.62f, 0.08f), new Vector3(0.08f, 0.72f, 0.08f), new Color(0.5f, 0.27f, 0.1f));
        CreatePart(visual, PrimitiveType.Cube, "Gnome Axe Head", new Vector3(-0.42f, 0.98f, 0.08f), new Vector3(0.34f, 0.16f, 0.08f), new Color(0.82f, 0.78f, 0.68f));
    }

    private static void BuildBossVisual(Transform root)
    {
        Transform visual = CreateVisualRoot(root, 0.03f, 2.4f);
        CreatePart(visual, PrimitiveType.Capsule, "Boss Body", new Vector3(0f, 1.02f, 0f), new Vector3(1.25f, 1.32f, 1.25f), new Color(0.12f, 0.06f, 0.06f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Crown", new Vector3(0f, 1.92f, 0f), new Vector3(0.78f, 0.18f, 0.78f), new Color(0.96f, 0.66f, 0.08f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Horn L", new Vector3(-0.48f, 1.92f, 0.05f), new Vector3(0.14f, 0.46f, 0.14f), new Color(0.9f, 0.86f, 0.72f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Horn R", new Vector3(0.48f, 1.92f, 0.05f), new Vector3(0.14f, 0.46f, 0.14f), new Color(0.9f, 0.86f, 0.72f));
        CreatePart(visual, PrimitiveType.Sphere, "Boss Eye L", new Vector3(-0.22f, 1.18f, 0.62f), Vector3.one * 0.16f, new Color(1f, 0.08f, 0.04f));
        CreatePart(visual, PrimitiveType.Sphere, "Boss Eye R", new Vector3(0.22f, 1.18f, 0.62f), Vector3.one * 0.16f, new Color(1f, 0.08f, 0.04f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Shoulder L", new Vector3(-0.8f, 1.1f, 0f), new Vector3(0.38f, 0.32f, 0.62f), new Color(0.34f, 0.08f, 0.07f));
        CreatePart(visual, PrimitiveType.Cube, "Boss Shoulder R", new Vector3(0.8f, 1.1f, 0f), new Vector3(0.38f, 0.32f, 0.62f), new Color(0.34f, 0.08f, 0.07f));
    }

    private static void CreatePart(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = RuntimeMaterials.Get("enemy_visual_" + name, color);
        Object.Destroy(part.GetComponent<Collider>());
    }
}

public class RuntimeEnemyVisualAnimator : MonoBehaviour
{
    private Transform visualRoot;
    private float amplitude;
    private float speed;
    private float seed;

    public void Initialize(Transform root, float bobAmplitude, float bobSpeed)
    {
        visualRoot = root;
        amplitude = bobAmplitude;
        speed = bobSpeed;
        seed = Random.value * 10f;
    }

    private void Update()
    {
        if (visualRoot == null || !EnemyMovementUtility.IsGameplayActive())
        {
            return;
        }

        float wave = Mathf.Sin(Time.time * speed + seed);
        visualRoot.localPosition = Vector3.up * (wave * amplitude);
        visualRoot.localRotation = Quaternion.Euler(0f, 0f, wave * 2.5f);
    }
}

public class RuntimeDungeonAudio : MonoBehaviour
{
    private AudioSource sfx;
    private AudioSource music;
    private Dictionary<RuntimeSfx, AudioClip> clips;
    private AudioClip menuLoop;
    private AudioClip gameLoop;
    private AudioClip bossLoop;

    private void Awake()
    {
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.volume = 0.7f;

        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.volume = 0.18f;

        clips = new Dictionary<RuntimeSfx, AudioClip>
        {
            { RuntimeSfx.Attack, Tone("Attack", 420f, 0.08f, 0.4f) },
            { RuntimeSfx.Hit, Tone("Hit", 150f, 0.12f, 0.55f) },
            { RuntimeSfx.Dash, Tone("Dash", 760f, 0.1f, 0.28f) },
            { RuntimeSfx.Coin, Tone("Coin", 980f, 0.16f, 0.42f) },
            { RuntimeSfx.Door, Tone("Door", 260f, 0.3f, 0.42f) },
            { RuntimeSfx.PlayerDamage, Tone("Damage", 90f, 0.22f, 0.55f) },
            { RuntimeSfx.Cast, Tone("Cast", 520f, 0.16f, 0.34f) },
            { RuntimeSfx.Trap, Tone("Trap", 620f, 0.08f, 0.33f) },
            { RuntimeSfx.FloorFall, Tone("FloorFall", 70f, 0.35f, 0.45f) },
            { RuntimeSfx.GameOver, Tone("GameOver", 120f, 0.55f, 0.5f) },
            { RuntimeSfx.Victory, Tone("Victory", 740f, 0.65f, 0.45f) }
        };
        menuLoop = Loop("MenuLoop", 160f, 6f, 0.12f);
        gameLoop = Loop("GameLoop", 110f, 7f, 0.14f);
        bossLoop = BossLoop("BossLoop", 78f, 7.6f, 0.16f);
    }

    public void PlaySfx(RuntimeSfx sfxType)
    {
        if (clips.TryGetValue(sfxType, out AudioClip clip))
        {
            sfx.pitch = Random.Range(0.96f, 1.04f);
            sfx.PlayOneShot(clip);
        }
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuLoop);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameLoop);
    }

    public void PlayBossMusic()
    {
        PlayMusic(bossLoop);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (music.clip == clip && music.isPlaying)
        {
            return;
        }

        music.clip = clip;
        music.Play();
    }

    private static AudioClip Tone(string name, float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - t / duration;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip Loop(string name, float baseFrequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];
        float[] notes = { baseFrequency, baseFrequency * 1.33f, baseFrequency * 1.5f, baseFrequency * 1.33f };
        float beat = duration / notes.Length;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            int note = Mathf.FloorToInt(t / beat) % notes.Length;
            data[i] = Mathf.Sin(2f * Mathf.PI * notes[note] * t) * volume;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip BossLoop(string name, float baseFrequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];
        float[] notes = { baseFrequency, baseFrequency * 0.94f, baseFrequency * 1.19f, baseFrequency * 0.84f, baseFrequency * 1.5f, baseFrequency * 0.94f };
        float beat = duration / notes.Length;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            int noteIndex = Mathf.FloorToInt(t / beat) % notes.Length;
            float fundamental = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t);
            float fifth = 0.45f * Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * 1.5f * t);
            float drone = 0.35f * Mathf.Sin(2f * Mathf.PI * baseFrequency * 0.5f * t);
            float pulse = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 3.4f * t);
            data[i] = (fundamental + fifth + drone) * volume * pulse * 0.55f;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

public enum RuntimeSfx
{
    Attack,
    Hit,
    Dash,
    Coin,
    Door,
    PlayerDamage,
    Cast,
    Trap,
    FloorFall,
    GameOver,
    Victory
}

public static class RuntimeVfx
{
    public static void Burst(Vector3 position, Color color, int count, float lifetime)
    {
        GameObject fx = new GameObject("Burst FX");
        fx.transform.position = position;
        ParticleSystem particles = fx.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startColor = color;
        main.startLifetime = lifetime;
        main.startSpeed = 2.7f;
        main.startSize = 0.12f;
        main.maxParticles = count;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        particles.Play();
        Object.Destroy(fx, lifetime + 0.5f);
    }

    public static void Slash(Vector3 position, Vector3 direction, Color color)
    {
        GameObject slash = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slash.name = "Slash FX";
        slash.transform.position = position;
        slash.transform.rotation = Quaternion.LookRotation(direction == Vector3.zero ? Vector3.forward : direction);
        slash.transform.localScale = new Vector3(1f, 0.06f, 0.36f);
        slash.GetComponent<Renderer>().material = RuntimeMaterials.Get("slash_" + color, color);
        Object.Destroy(slash.GetComponent<Collider>());
        Object.Destroy(slash, 0.12f);
    }

    public static void DashTrail(Vector3 position, Vector3 direction)
    {
        Color color = new Color(0.28f, 0.78f, 1f);
        Burst(position, color, 14, 0.22f);

        Vector3 flatDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
        for (int i = 0; i < 3; i++)
        {
            GameObject streak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            streak.name = "Dash Trail FX";
            streak.transform.position = position - flatDirection * (0.18f + i * 0.18f);
            streak.transform.rotation = Quaternion.LookRotation(flatDirection);
            streak.transform.localScale = new Vector3(0.12f, 0.05f, 0.45f - i * 0.09f);
            streak.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("dash_trail", color, 1.4f);
            Object.Destroy(streak.GetComponent<Collider>());
            Object.Destroy(streak, 0.12f + i * 0.035f);
        }
    }

    public static void FloatingText(Vector3 position, string value, Color color)
    {
        GameObject textObject = new GameObject("Floating Text FX");
        textObject.transform.position = position;
        FloatingTextFx floatingText = textObject.AddComponent<FloatingTextFx>();
        floatingText.Initialize(value, color);
    }
}

public class FloatingTextFx : MonoBehaviour
{
    private TextMesh textMesh;
    private Color baseColor;
    private float createdAt;
    private float lifetime = 0.72f;
    private Vector3 drift;

    public void Initialize(string value, Color color)
    {
        baseColor = color;
        createdAt = Time.time;
        drift = new Vector3(0f, 0.85f, 0f);

        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.text = value;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.fontSize = 64;
        textMesh.characterSize = 0.055f;
        textMesh.color = color;

        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");
            renderer.material = new Material(shader);
            renderer.material.color = color;
        }
    }

    private void Update()
    {
        float age = Time.time - createdAt;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += drift * Time.deltaTime;
        transform.localScale = Vector3.one * Mathf.Lerp(1.15f, 0.85f, age / lifetime);

        Camera camera = Camera.main;
        if (camera != null)
        {
            transform.rotation = camera.transform.rotation;
        }

        Color color = baseColor;
        color.a = Mathf.Lerp(1f, 0f, age / lifetime);
        if (textMesh != null)
        {
            textMesh.color = color;
        }
    }
}

public static class RuntimeMaterials
{
    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public static Material Get(string key, Color color)
    {
        if (Materials.TryGetValue(key, out Material material))
        {
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        Materials[key] = material;
        return material;
    }

    public static Material GetEmissive(string key, Color color, float intensity)
    {
        Material material = Get(key, color);
        Color emission = color * Mathf.Max(0f, intensity);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }
        return material;
    }
}

public static class RuntimeSceneLighting
{
    public static void Ensure()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.34f, 0.24f, 0.19f);
        RenderSettings.ambientEquatorColor = new Color(0.16f, 0.09f, 0.08f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.04f, 0.04f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.16f, 0.07f, 0.07f);
        RenderSettings.fogDensity = 0.022f;

        EnsureDirectional("Runtime Key Light", Quaternion.Euler(48f, -34f, 0f), new Color(1f, 0.82f, 0.62f), 1.25f);
        EnsureDirectional("Runtime Rim Light", Quaternion.Euler(34f, 136f, 0f), new Color(0.42f, 0.58f, 1f), 0.45f);
    }

    public static void TintMainLights(Color keyColor, float keyIntensity, Color rimColor, float rimIntensity)
    {
        TintDirectional("Runtime Key Light", keyColor, keyIntensity);
        TintDirectional("Runtime Rim Light", rimColor, rimIntensity);
    }

    private static void TintDirectional(string name, Color color, float intensity)
    {
        GameObject lightObject = GameObject.Find(name);
        if (lightObject == null)
        {
            return;
        }

        Light light = lightObject.GetComponent<Light>();
        if (light == null)
        {
            return;
        }

        light.color = color;
        light.intensity = intensity;
    }

    private static void EnsureDirectional(string name, Quaternion rotation, Color color, float intensity)
    {
        GameObject lightObject = GameObject.Find(name);
        if (lightObject == null)
        {
            lightObject = new GameObject(name);
        }

        Light light = lightObject.GetComponent<Light>();
        if (light == null)
        {
            light = lightObject.AddComponent<Light>();
        }

        light.type = LightType.Directional;
        light.color = color;
        light.intensity = intensity;
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = rotation;
    }
}
