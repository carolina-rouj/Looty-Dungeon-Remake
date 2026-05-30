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
    private GUIStyle heartStyle;
    private GUIStyle buttonStyle;
    private float guiScale = -1f;
    private float messageUntil;
    private string message = "";
    private float transitionLockUntil;
    private float damageOverlayUntil;
    private bool standaloneSmokeStarted;
    private bool clearingLevel;
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
                StartNewGame();
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
            EnemyDeathWatcher.Ensure(enemy, this);
            EnemyFloorFall.Ensure(enemy);
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
        Vector3 spawn = ComputePlayerSpawn(manager);
        player.ResetAt(spawn);
        playerHealth.RefreshVisuals();
        cameraFollow.SetStaticFocus(ComputeCameraFocus(manager));
        CreateDoor(ComputeExitPosition(manager));
        // El suelo que cae lo gestiona el LevelManager de Carolina segun el flag
        // 'fallingFloor' de cada JSON, asi que aqui ya no lo disparamos.
        DiscoverEnemies();
        ScatterCoins(manager);
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

    // --- Integracion con el LevelManager de Carolina ---
    // Su LevelManager centra la cuadricula alrededor de su propio transform y
    // expone los limites del nivel (MinBoundZ/MaxBoundZ/MaxBoundX), pero no marca
    // puntos de spawn/salida. Los derivamos de esos limites.

    // OJO: el GridToWorld de Carolina centra el nivel en el ORIGEN DEL MUNDO e ignora
    // la posicion del GameObject LevelManager, asi que estos puntos se calculan en
    // coordenadas de mundo (no relativas al transform del LevelManager).
    private Vector3 ComputePlayerSpawn(LevelManager manager)
    {
        // Colocamos al jugador sobre un tile de suelo REAL del frente del nivel para que
        // no aparezca en el vacio y caiga (lo que le drenaria la vida al instante).
        Transform tile = FindFloorTileNear(manager, new Vector3(0f, 0f, manager.MinBoundZ));
        if (tile != null) return tile.position + Vector3.up * 0.9f;
        return new Vector3(0f, 0.9f, manager.MinBoundZ);
    }

    private Vector3 ComputeExitPosition(LevelManager manager)
    {
        Transform tile = FindFloorTileNear(manager, new Vector3(0f, 0f, manager.MaxBoundZ));
        if (tile != null) return tile.position + Vector3.up * 0.1f;
        return new Vector3(0f, 0.1f, manager.MaxBoundZ);
    }

    private Vector3 ComputeCameraFocus(LevelManager manager)
    {
        return new Vector3(0f, 0f, (manager.MinBoundZ + manager.MaxBoundZ) * 0.5f);
    }

    // Coloca monedas sobre tiles de suelo aleatorios (de forma aditiva, sin tocar el JSON
    // de Carolina). El jugador las recoge y suben el contador del HUD.
    private void ScatterCoins(LevelManager manager)
    {
        int floorLayer = LayerMask.NameToLayer("Floor");
        List<Transform> floors = new List<Transform>();
        foreach (Transform child in manager.transform)
        {
            if (child.gameObject.layer == floorLayer) floors.Add(child);
        }
        if (floors.Count == 0) return;

        int count = Mathf.Clamp(floors.Count / 5, 3, 9);
        for (int i = 0; i < count; i++)
        {
            Transform tile = floors[Random.Range(0, floors.Count)];
            CoinPickup.Create(manager.transform, tile.position + Vector3.up * 0.55f);
        }
    }

    // Los tiles de suelo son hijos del LevelManager y estan en la capa "Floor"; devolvemos
    // el mas cercano a una posicion de referencia (frente del nivel = spawn, fondo = salida).
    private static Transform FindFloorTileNear(LevelManager manager, Vector3 reference)
    {
        int floorLayer = LayerMask.NameToLayer("Floor");
        Transform best = null;
        float bestSqr = float.MaxValue;
        foreach (Transform child in manager.transform)
        {
            if (child.gameObject.layer != floorLayer) continue;
            float sqr = (child.position - reference).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = child; }
        }
        return best;
    }

    // El LevelManager de Carolina instancia sus propios prefabs de enemigos pero no
    // avisa al runtime; aqui los recogemos y les enganchamos el glue de gameplay
    // (dano por contacto, flash al golpear, deteccion de muerte) sin tocar sus scripts.
    private void DiscoverEnemies()
    {
        aliveEnemies.Clear();
        foreach (MonoBehaviour enemy in FindEnemyBehaviours())
        {
            if (enemy != null)
            {
                RegisterEnemy(enemy.gameObject);
            }
        }
    }

    private static List<MonoBehaviour> FindEnemyBehaviours()
    {
        List<MonoBehaviour> result = new List<MonoBehaviour>();
        result.AddRange(FindObjectsByType<Slime>(FindObjectsSortMode.None));
        result.AddRange(FindObjectsByType<Bat>(FindObjectsSortMode.None));
        result.AddRange(FindObjectsByType<Wizard>(FindObjectsSortMode.None));
        result.AddRange(FindObjectsByType<Gnome>(FindObjectsSortMode.None));
        result.AddRange(FindObjectsByType<Boss>(FindObjectsSortMode.None));
        return result;
    }

    // Lo llama EnemyDeathWatcher.OnDestroy. Ignora las destrucciones provocadas por
    // el cambio de nivel para no contarlas como bajas.
    public void HandleEnemyDestroyed(GameObject enemy)
    {
        if (clearingLevel || State != DungeonState.Playing)
        {
            aliveEnemies.Remove(enemy);
            return;
        }
        NotifyEnemyDefeated(enemy);
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
        player.ResetAt(ComputePlayerSpawn(levelManager));
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

        // El LevelManager de Carolina construye el nivel de forma sincrona dentro de
        // LoadLevel y no hace callback, asi que disparamos nosotros el wiring posterior.
        // La guarda 'clearingLevel' evita que las destrucciones de enemigos del nivel
        // anterior (al limpiar) se cuenten como bajas.
        // try/finally para que el wiring posterior SIEMPRE se ejecute: si LoadLevel
        // fallara, sin esto el fade de transicion se quedaria en negro para siempre.
        try
        {
            clearingLevel = true;
            levelManager.LoadLevel(levelNames[CurrentLevelIndex]);
        }
        finally
        {
            clearingLevel = false;
            NotifyLevelBuilt(levelManager);
        }
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
            Rect panel = DrawCenteredPanel(600f, 440f);
            GUI.Label(PanelRect(panel, 90f, 24f, 420f, 60f), "Creditos", titleStyle);
            GUI.Label(PanelRect(panel, 40f, 96f, 520f, 250f), "Looty Dungeon 3D - Proyecto VJ (FIB - UPC)\n\nDesarrollado por:\nCarolina Rodriguez Ujano\nMateus Grandolfi Albuquerque\n\nProfesor tutor: Oscar Argudo Medrano\n\nReferencia: Looty Dungeon", bodyStyle);
            if (GUI.Button(PanelCenteredRect(panel, 372f, 240f, 48f), "Volver", buttonStyle)) ShowMenu();
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
        string enemiesText = aliveEnemies.Count > 0 ? "     Enemigos: " + aliveEnemies.Count : "     Puerta abierta";
        GUI.Label(new Rect(labelX, 16f * scale, labelWidth, 42f * scale), "\u25C6 " + Coins + "     Salas " + RoomsCleared + "/10     Sala " + (CurrentLevelIndex + 1) + "/10" + enemiesText + "     " + timeText, hudStyle);
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
        float scale = GetGuiScale();
        int target = Mathf.RoundToInt(34 * scale);
        if (heartStyle == null || heartStyle.fontSize != target)
        {
            heartStyle = new GUIStyle
            {
                fontSize = target,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
        Color previous = GUI.color;
        for (int i = 0; i < 3; i++)
        {
            GUI.color = i < Health ? new Color(0.97f, 0.18f, 0.22f) : new Color(0.34f, 0.10f, 0.12f);
            GUI.Label(new Rect((14f + i * 34f) * scale, 6f * scale, 36f * scale, 42f * scale), "\u2665", heartStyle);
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
            // Salvavidas: si por lo que sea la carga no termina, no nos quedamos en negro.
            if (elapsed > 1.5f)
            {
                transitionStartedAt = -1f;
                return;
            }
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
        Rect panel = DrawCenteredPanel(540f, 360f);
        GUI.Label(PanelRect(panel, 50f, 22f, 440f, 60f), title, titleStyle);
        GUI.Label(PanelRect(panel, 30f, 96f, 480f, 110f), subtitle, bodyStyle);
        if (GUI.Button(PanelCenteredRect(panel, 226f, 240f, 48f), State == DungeonState.Victory ? "Jugar de nuevo" : "Reintentar", buttonStyle)) StartNewGame();
        if (GUI.Button(PanelCenteredRect(panel, 288f, 240f, 48f), "Menu", buttonStyle)) ShowMenu();
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
        return Mathf.Clamp(Mathf.Min(Screen.width / 620f, Screen.height / 350f), 1.25f, 2.0f);
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
        foreach (Slime enemy in FindObjectsByType<Slime>(FindObjectsSortMode.None)) enemies.Add(enemy.gameObject);
        foreach (Bat enemy in FindObjectsByType<Bat>(FindObjectsSortMode.None)) enemies.Add(enemy.gameObject);
        foreach (Wizard enemy in FindObjectsByType<Wizard>(FindObjectsSortMode.None)) enemies.Add(enemy.gameObject);
        foreach (Gnome enemy in FindObjectsByType<Gnome>(FindObjectsSortMode.None)) enemies.Add(enemy.gameObject);
        foreach (Boss enemy in FindObjectsByType<Boss>(FindObjectsSortMode.None)) enemies.Add(enemy.gameObject);
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

