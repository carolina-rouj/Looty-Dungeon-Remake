using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public string levelFileName = "level1";
    public GameObject floorPrefab; // #
    public GameObject floorDarkPrefab; // D
    public GameObject floorBluePrefab; // B
    public GameObject floorCarpetEnd; // c
    public GameObject floorCarpetContinue; // C

    // Enemigos del nivel
    public GameObject slimePrefab;
    public GameObject batPrefab;
    public GameObject wizardPrefab;
    public GameObject gnomePrefab;
    public GameObject bossPrefab;

    // trampas del nivel
    public GameObject rastroSlimePrefab;
    public GameObject spiderWebsPrefab;
    public GameObject retractileForkPrefab;

    // decoraciones (prefabs reales de Carolina)
    public GameObject floorTorchPrefab;
    public GameObject thronePrefab;
    public GameObject calizPrefab;

    // altura Y de spawn para que los enemigos no traspasen el suelo
    public float enemySpawnHeight = 1f;

    // Singleton
    public static LevelManager Instance { get; private set; }


    public float tamañoCasilla { get; private set; } = 1.0f;
    public float MaxBoundX { get; private set; }
    public float MinBoundZ { get; private set; }
    public float MaxBoundZ { get; private set; }

    private LevelData levelData;
    private int gridRows;
    private int gridCols;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Dictionary<int, List<GameObject>> floorTilesByRow = new Dictionary<int, List<GameObject>>();
    private HashSet<GameObject> activeFloorTiles = new HashSet<GameObject>();
    private Dictionary<string, GameObject> enemyPrefabs;
    private Dictionary<string, GameObject> trapPrefabs;
    private Coroutine fallingRowsRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        enemyPrefabs = new Dictionary<string, GameObject>
        {
            { "Slime",  slimePrefab  },
            { "Bat",    batPrefab    },
            { "Wizard", wizardPrefab },
            { "Gnome",  gnomePrefab  },
            { "Boss",   bossPrefab   },
        };

        trapPrefabs = new Dictionary<string, GameObject>
        {
            { "RastroSlime",    rastroSlimePrefab    },
            { "SpiderWebs",     spiderWebsPrefab     },
            { "RetractileFork", retractileForkPrefab },
        };

        LoadAndBuild();
    }

    // Borra el nivel actual antes de cargar otro
    void ClearLevel()
    {
        if (fallingRowsRoutine != null)
        {
            StopCoroutine(fallingRowsRoutine);
            fallingRowsRoutine = null;
        }

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (GameObject go in spawnedObjects)
            if (go != null) Destroy(go);

        spawnedObjects.Clear();
        levelData = null;
        gridRows = 0;
        gridCols = 0;
        floorTilesByRow.Clear();
        activeFloorTiles.Clear();
    }

    // Cambio de nivel
    public void LoadLevel(string fileName)
    {
        ClearLevel();
        levelFileName = fileName;
        LoadAndBuild();
    }

    void LoadAndBuild()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>($"Levels/{levelFileName}");
        if (jsonAsset == null)
        {
            Debug.LogError($"[LevelManager] No se encontró Resources/Levels/{levelFileName}.json");
            return;
        }

        levelData = JsonUtility.FromJson<LevelData>(jsonAsset.text);
        if (levelData == null)
        {
            Debug.LogError("[LevelManager] Error al parsear el JSON del nivel.");
            return;
        }

        tamañoCasilla = levelData.tamañoCasilla > 0f ? levelData.tamañoCasilla : 1.0f;

        BuildFloor();
        BuildWallsAndDecorations();
        SpawnCoins();
        SpawnEnemies();
        SpawnTraps();

        Debug.Log($"[LevelManager] Nivel '{levelData.name}' cargado.");
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.NotifyLevelBuilt(this);
        }
    }

    // funcion para montar el suelo del nivel segun el json
    void BuildFloor()
    {
        if (levelData.grid == null || levelData.grid.Length == 0) return;

        gridRows = levelData.grid.Length;
        gridCols = 0;
        foreach (var row in levelData.grid)
            if (row.Length > gridCols) gridCols = row.Length;

        float halfW = gridCols * tamañoCasilla / 2f - tamañoCasilla / 2f;
        float halfH = gridRows * tamañoCasilla / 2f - tamañoCasilla / 2f;

        MaxBoundX =  halfW;
        MinBoundZ = -halfH;
        MaxBoundZ =  halfH;

        for (int r = 0; r < gridRows; r++)
        {
            string rowStr = levelData.grid[r];
            for (int c = 0; c < rowStr.Length; c++)
            {
                GameObject prefabToUse = rowStr[c] switch
                {
                    '#' => floorPrefab,
                    'D' => floorDarkPrefab,
                    'B' => floorBluePrefab,
                    'c' => floorCarpetEnd,
                    'C' => floorCarpetContinue,
                    _   => null
                };
                if (prefabToUse == null) continue;

                GameObject tile = Instantiate(prefabToUse, GridToWorld(c, r), Quaternion.identity, transform);
                int floorLayer = LayerMask.NameToLayer("Floor");
                if (floorLayer >= 0)
                {
                    tile.layer = floorLayer;
                }
                tile.name  = $"Tile_{c}_{r}";
                EnsureUrpMaterials(tile, GetFloorColor(rowStr[c]));
                if (!floorTilesByRow.TryGetValue(r, out List<GameObject> rowTiles))
                {
                    rowTiles = new List<GameObject>();
                    floorTilesByRow[r] = rowTiles;
                }
                rowTiles.Add(tile);
                activeFloorTiles.Add(tile);
            }
        }
    }

    void SpawnEnemies()
    {
        if (levelData.enemies == null) return;
        foreach (var spawn in levelData.enemies)
        {
            GameObject enemy = null;
            if (!enemyPrefabs.TryGetValue(spawn.type, out GameObject prefab) || prefab == null)
            {
                Debug.Log($"[LevelManager] Prefab de enemigo '{spawn.type}' no asignado. Usando fallback runtime.");
                enemy = RuntimeEnemyFactory.Create(spawn.type, GridToWorld(spawn.col, spawn.row));
            }
            else
            {
                Vector3 pos = GridToWorld(spawn.col, spawn.row);
                pos.y = enemySpawnHeight;
                enemy = Instantiate(prefab, pos, Quaternion.identity);
                EnsureUrpMaterials(enemy, GetEnemyColor(spawn.type));
            }

            if (enemy != null)
            {
                enemy.transform.SetParent(transform, true);
                spawnedObjects.Add(enemy);
                if (DungeonGameRuntime.Instance != null)
                {
                    DungeonGameRuntime.Instance.RegisterEnemy(enemy);
                }
            }
        }
    }

    void SpawnTraps()
    {
        if (levelData.traps == null) return;
        foreach (var spawn in levelData.traps)
        {
            if (!trapPrefabs.TryGetValue(spawn.type, out GameObject prefab) || prefab == null)
            {
                GameObject runtimeTrap = RuntimeTrap.Create(spawn.type, GridToWorld(spawn.col, spawn.row), transform);
                if (runtimeTrap == null)
                {
                    Debug.Log($"[LevelManager] Prefab de trampa '{spawn.type}' no asignado.");
                    continue;
                }

                spawnedObjects.Add(runtimeTrap);
                continue;
            }

            GameObject trap = Instantiate(prefab, GridToWorld(spawn.col, spawn.row), Quaternion.identity, transform);
            spawnedObjects.Add(trap);
        }
    }

    // Convierte coordenadas de casilla (col, row) a posición en el mundo
    public Vector3 GridToWorld(int col, int row)
    {
        float halfW = gridCols * tamañoCasilla / 2f - tamañoCasilla / 2f;
        float halfH = gridRows * tamañoCasilla / 2f - tamañoCasilla / 2f;
        return new Vector3(
            col * tamañoCasilla - halfW,
            0f,
            row * tamañoCasilla - halfH
        );
    }

    public Vector3 GetPlayerSpawnPosition()
    {
        if (levelData?.grid == null) return Vector3.up;

        int preferredColumn = Mathf.Clamp(gridCols / 2, 0, gridCols - 1);
        for (int r = 0; r < gridRows; r++)
        {
            if (IsSpawnTileAvailable(preferredColumn, r))
            {
                return GridToWorld(preferredColumn, r) + Vector3.up * 0.7f;
            }
        }

        for (int r = 0; r < gridRows; r++)
        {
            for (int c = 0; c < gridCols; c++)
            {
                if (IsSpawnTileAvailable(c, r))
                {
                    return GridToWorld(c, r) + Vector3.up * 0.7f;
                }
            }
        }

        return Vector3.up;
    }

    private bool IsSpawnTileAvailable(int col, int row)
    {
        if (!IsWalkable(col, row))
        {
            return false;
        }

        return activeFloorTiles.Count == 0 || HasFloorAt(GridToWorld(col, row));
    }

    public Vector3 GetExitPosition()
    {
        if (levelData?.grid == null) return Vector3.forward * 4f;

        for (int r = gridRows - 1; r >= 0; r--)
        {
            int c = Mathf.Clamp(gridCols / 2, 0, gridCols - 1);
            if (IsWalkable(c, r))
            {
                return GridToWorld(c, r) + Vector3.up * 0.1f;
            }
        }

        return Vector3.forward * 4f;
    }

    public Vector3 GetCameraFocusPosition()
    {
        return transform.position;
    }

    public int GetActiveFloorCount()
    {
        int count = 0;
        foreach (GameObject tile in activeFloorTiles)
        {
            if (tile != null)
            {
                count++;
            }
        }

        return count;
    }

    public bool HasFloorAt(Vector3 worldPosition)
    {
        foreach (GameObject tile in activeFloorTiles)
        {
            if (tile == null) continue;

            Vector3 tilePosition = tile.transform.position;
            float half = tamañoCasilla * 0.48f;
            if (Mathf.Abs(worldPosition.x - tilePosition.x) <= half &&
                Mathf.Abs(worldPosition.z - tilePosition.z) <= half)
            {
                return true;
            }
        }

        return false;
    }

    public void StartFallingRowsIfNeeded(int levelIndex)
    {
        StopFallingRows();

        if (levelData != null && levelData.fallingFloor && levelIndex >= 2 && levelIndex < 9)
        {
            fallingRowsRoutine = StartCoroutine(FallingRows(levelIndex));
        }
    }

    public void StopFallingRows()
    {
        if (fallingRowsRoutine != null)
        {
            StopCoroutine(fallingRowsRoutine);
            fallingRowsRoutine = null;
        }
    }

    private IEnumerator FallingRows(int levelIndex)
    {
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.ShowMessage("El suelo empezara a caer");
        }

        yield return new WaitForSeconds(Mathf.Max(3.2f, 6.0f - levelIndex * 0.25f));

        for (int r = 0; r < gridRows - 1; r++)
        {
            if (floorTilesByRow.TryGetValue(r, out List<GameObject> rowTiles))
            {
                yield return StartCoroutine(WarnRow(rowTiles));
                if (DungeonGameRuntime.Instance != null)
                {
                    DungeonGameRuntime.Instance.PlayFloorFall(AveragePosition(rowTiles));
                }

                foreach (GameObject tile in rowTiles)
                {
                    if (tile == null) continue;
                    activeFloorTiles.Remove(tile);
                    StartCoroutine(FallAndDestroy(tile));
                }
            }

            yield return new WaitForSeconds(Mathf.Max(0.75f, 1.65f - levelIndex * 0.08f));
        }
    }

    private IEnumerator WarnRow(List<GameObject> rowTiles)
    {
        float elapsed = 0f;
        float duration = 0.45f;
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color warningColor = Color.Lerp(new Color(1f, 0.18f, 0.04f), new Color(1f, 0.82f, 0.08f), Mathf.PingPong(elapsed * 8f, 1f));
            block.Clear();
            block.SetColor("_BaseColor", warningColor);
            block.SetColor("_Color", warningColor);

            foreach (GameObject tile in rowTiles)
            {
                if (tile == null) continue;
                foreach (Renderer renderer in tile.GetComponentsInChildren<Renderer>())
                {
                    renderer.SetPropertyBlock(block);
                }
            }

            yield return null;
        }

        foreach (GameObject tile in rowTiles)
        {
            if (tile == null) continue;
            foreach (Renderer renderer in tile.GetComponentsInChildren<Renderer>())
            {
                renderer.SetPropertyBlock(null);
            }
        }
    }

    private IEnumerator FallAndDestroy(GameObject tile)
    {
        Collider tileCollider = tile.GetComponent<Collider>();
        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }

        Vector3 start = tile.transform.position;
        Vector3 end = start + Vector3.down * 4f;
        float elapsed = 0f;
        while (elapsed < 0.75f)
        {
            elapsed += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(start, end, elapsed / 0.75f);
            tile.transform.Rotate(0f, 150f * Time.deltaTime, 0f, Space.World);
            yield return null;
        }

        Destroy(tile);
    }

    private Vector3 AveragePosition(List<GameObject> objects)
    {
        Vector3 total = Vector3.zero;
        int count = 0;
        foreach (GameObject item in objects)
        {
            if (item == null) continue;
            total += item.transform.position;
            count++;
        }

        return count > 0 ? total / count : transform.position;
    }

    private bool IsWalkable(int col, int row)
    {
        if (levelData?.grid == null || row < 0 || row >= levelData.grid.Length) return false;
        string rowText = levelData.grid[row];
        if (col < 0 || col >= rowText.Length) return false;

        char cell = rowText[col];
        return cell == '#' || cell == 'D' || cell == 'B' || cell == 'c' || cell == 'C';
    }

    private void SpawnCoins()
    {
        int levelIndex = GetLevelIndex();
        int coinsToSpawn = Mathf.Clamp(3 + levelIndex, 3, 12);
        int spawned = 0;

        for (int r = 1; r < gridRows - 1 && spawned < coinsToSpawn; r++)
        {
            for (int c = 1; c < gridCols - 1 && spawned < coinsToSpawn; c++)
            {
                if (!IsWalkable(c, r) || (r + c + levelIndex) % 3 != 0)
                {
                    continue;
                }

                CoinPickup.Create(transform, GridToWorld(c, r));
                spawned++;
            }
        }
    }

    private void BuildWallsAndDecorations()
    {
        if (gridCols == 0 || gridRows == 0) return;

        float wallHeight = 1.25f;
        float halfW = gridCols * tamañoCasilla / 2f;
        float halfH = gridRows * tamañoCasilla / 2f;
        CreateBlock("Wall North", new Vector3(0f, wallHeight * 0.5f, halfH), new Vector3(gridCols * tamañoCasilla + 1f, wallHeight, 0.35f), new Color(0.42f, 0.13f, 0.1f));
        CreateBlock("Wall South", new Vector3(0f, wallHeight * 0.5f, -halfH), new Vector3(gridCols * tamañoCasilla + 1f, wallHeight, 0.35f), new Color(0.42f, 0.13f, 0.1f));
        CreateBlock("Wall East", new Vector3(halfW, wallHeight * 0.5f, 0f), new Vector3(0.35f, wallHeight, gridRows * tamañoCasilla + 1f), new Color(0.42f, 0.13f, 0.1f));
        CreateBlock("Wall West", new Vector3(-halfW, wallHeight * 0.5f, 0f), new Vector3(0.35f, wallHeight, gridRows * tamañoCasilla + 1f), new Color(0.42f, 0.13f, 0.1f));

        CreateDecoration("Torch", GridToWorld(0, 1) + Vector3.up * 0.7f, PrimitiveType.Capsule, new Vector3(0.18f, 0.45f, 0.18f), new Color(1f, 0.35f, 0.05f));

        if (floorTorchPrefab != null)
        {
            SpawnDecorationPrefab(floorTorchPrefab, "Decoration FloorTorch Right", GridToWorld(Mathf.Max(1, gridCols - 1), 1), Quaternion.identity, 1f);
        }

        CreateDecoration("Crate", GridToWorld(Mathf.Max(1, gridCols - 2), 1) + Vector3.up * 0.35f, PrimitiveType.Cube, Vector3.one * 0.65f, new Color(0.45f, 0.2f, 0.08f));
        CreateDecoration("Pillar", GridToWorld(1, Mathf.Max(1, gridRows - 2)) + Vector3.up * 0.55f, PrimitiveType.Cylinder, new Vector3(0.35f, 0.65f, 0.35f), new Color(0.54f, 0.24f, 0.18f));
        CreateDecoration("Statue", GridToWorld(Mathf.Max(1, gridCols - 2), Mathf.Max(1, gridRows - 2)) + Vector3.up * 0.55f, PrimitiveType.Capsule, new Vector3(0.35f, 0.55f, 0.35f), new Color(0.44f, 0.43f, 0.38f));
        CreateDecoration("Banner", GridToWorld(0, Mathf.Clamp(gridRows / 2, 0, gridRows - 1)) + new Vector3(-0.35f, 0.75f, 0f), PrimitiveType.Cube, new Vector3(0.06f, 0.75f, 0.42f), new Color(0.78f, 0.06f, 0.1f));

        // boss room: throne en el centro detras del boss; caliz como trofeo
        bool isBossRoom = levelFileName == "final_level";
        if (isBossRoom)
        {
            if (thronePrefab != null)
            {
                Vector3 thronePos = GridToWorld(gridCols / 2, gridRows - 3);
                SpawnDecorationPrefab(thronePrefab, "Decoration Throne", thronePos, Quaternion.Euler(0f, 180f, 0f), 1.2f);
            }
            if (calizPrefab != null)
            {
                Vector3 calizPos = GridToWorld(gridCols / 2, gridRows - 4);
                SpawnDecorationPrefab(calizPrefab, "Decoration Caliz", calizPos, Quaternion.identity, 0.8f);
            }
        }
    }

    private void SpawnDecorationPrefab(GameObject prefab, string name, Vector3 position, Quaternion rotation, float scale)
    {
        if (prefab == null) return;
        GameObject instance = Instantiate(prefab, position, rotation, transform);
        instance.name = name;
        if (scale != 1f)
        {
            instance.transform.localScale *= scale;
        }
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        Color decoColor = name.Contains("Torch")
            ? new Color(0.55f, 0.32f, 0.18f)
            : name.Contains("Throne")
                ? new Color(0.6f, 0.18f, 0.12f)
                : name.Contains("Caliz")
                    ? new Color(1f, 0.78f, 0.18f)
                    : new Color(0.6f, 0.45f, 0.3f);
        EnsureUrpMaterials(instance, decoColor);
        spawnedObjects.Add(instance);
    }

    private static Color GetEnemyColor(string type)
    {
        return type switch
        {
            "Slime" => new Color(0.38f, 0.82f, 0.36f),
            "Bat" => new Color(0.32f, 0.16f, 0.42f),
            "Wizard" => new Color(0.6f, 0.42f, 1f),
            "Gnome" => new Color(0.95f, 0.74f, 0.32f),
            "Boss" => new Color(0.88f, 0.18f, 0.18f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };
    }

    private static Color GetFloorColor(char cell)
    {
        return cell switch
        {
            '#' => new Color(0.66f, 0.30f, 0.18f),
            'D' => new Color(0.45f, 0.22f, 0.13f),
            'B' => new Color(0.30f, 0.38f, 0.55f),
            'c' => new Color(0.78f, 0.10f, 0.10f),
            'C' => new Color(0.86f, 0.16f, 0.14f),
            _ => new Color(0.55f, 0.27f, 0.16f)
        };
    }

    private static void EnsureUrpMaterials(GameObject root, Color color)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        string materialKey = "level_" + color.r.ToString("F2") + color.g.ToString("F2") + color.b.ToString("F2");
        Material urp = RuntimeMaterials.Get(materialKey, color);
        foreach (Renderer renderer in renderers)
        {
            int count = renderer.sharedMaterials.Length;
            if (count == 0)
            {
                renderer.sharedMaterial = urp;
                continue;
            }
            Material[] mats = new Material[count];
            for (int i = 0; i < count; i++)
            {
                mats[i] = urp;
            }
            renderer.sharedMaterials = mats;
        }
    }

    private void CreateBlock(string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(transform, false);
        block.transform.position = position;
        block.transform.localScale = scale;
        block.GetComponent<Renderer>().material = RuntimeMaterials.Get(name, color);
    }

    private void CreateDecoration(string name, Vector3 position, PrimitiveType type, Vector3 scale, Color color)
    {
        GameObject decoration = GameObject.CreatePrimitive(type);
        decoration.name = "Decoration " + name;
        decoration.transform.SetParent(transform, false);
        decoration.transform.position = position;
        decoration.transform.localScale = scale;
        decoration.GetComponent<Renderer>().material = RuntimeMaterials.Get(name, color);
        Collider collider = decoration.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (name == "Torch")
        {
            CreateTorchFlame(decoration.transform);
        }
        else if (name == "Statue")
        {
            CreateDecorationDetail(decoration.transform, "Statue Head", PrimitiveType.Sphere, new Vector3(0f, 0.72f, 0f), Vector3.one * 0.46f, new Color(0.5f, 0.49f, 0.44f));
        }
        else if (name == "Banner")
        {
            CreateDecorationDetail(decoration.transform, "Banner Trim", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(1.35f, 0.08f, 1.08f), new Color(0.96f, 0.7f, 0.08f));
        }
    }

    private void CreateTorchFlame(Transform torch)
    {
        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Torch Flame";
        flame.transform.SetParent(torch, false);
        flame.transform.localPosition = new Vector3(0f, 0.58f, 0f);
        flame.transform.localScale = new Vector3(1.25f, 0.9f, 1.25f);
        flame.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("torch_flame", new Color(1f, 0.38f, 0.05f), 1.8f);
        Destroy(flame.GetComponent<Collider>());

        ParticleSystem particles = flame.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 0.48f;
        main.startSpeed = 0.36f;
        main.startSize = 0.12f;
        main.startColor = new Color(1f, 0.44f, 0.08f, 0.72f);
        main.maxParticles = 36;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 14f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 18f;
        shape.radius = 0.06f;

        GameObject lightObject = new GameObject("Torch Light");
        lightObject.transform.SetParent(torch, false);
        lightObject.transform.localPosition = new Vector3(0f, 0.78f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.52f, 0.16f);
        light.range = 4.2f;
        light.intensity = 1.15f;
        light.shadows = LightShadows.None;
    }

    private void CreateDecorationDetail(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject detail = GameObject.CreatePrimitive(type);
        detail.name = name;
        detail.transform.SetParent(parent, false);
        detail.transform.localPosition = localPosition;
        detail.transform.localScale = localScale;
        detail.GetComponent<Renderer>().material = RuntimeMaterials.Get(name, color);
        Collider collider = detail.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private int GetLevelIndex()
    {
        if (levelFileName == "final_level") return 9;
        if (levelFileName.StartsWith("level") && int.TryParse(levelFileName.Substring(5), out int parsed))
        {
            return Mathf.Clamp(parsed - 1, 0, 8);
        }

        return 0;
    }

    // Redondea una posición del mundo al centro de casilla más cercano
    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        if (gridCols == 0 || gridRows == 0) return worldPos;

        float halfW = gridCols * tamañoCasilla / 2f - tamañoCasilla / 2f;
        float halfH = gridRows * tamañoCasilla / 2f - tamañoCasilla / 2f;

        int col = Mathf.RoundToInt((worldPos.x + halfW) / tamañoCasilla);
        int row = Mathf.RoundToInt((worldPos.z + halfH) / tamañoCasilla);

        col = Mathf.Clamp(col, 0, gridCols - 1);
        row = Mathf.Clamp(row, 0, gridRows - 1);

        return new Vector3(
            col * tamañoCasilla - halfW,
            worldPos.y,
            row * tamañoCasilla - halfH
        );
    }

    // Comprueba si una posición está dentro del área del nivel
    public bool IsInBounds(Vector3 p)
    {
        return p.x >= -MaxBoundX - tamañoCasilla * 0.5f &&
               p.x <=  MaxBoundX + tamañoCasilla * 0.5f &&
               p.z >=  MinBoundZ - tamañoCasilla * 0.5f &&
               p.z <=  MaxBoundZ + tamañoCasilla * 0.5f;
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown) {
            if (DungeonGameRuntime.Instance != null) return;
            if (e.keyCode == KeyCode.Alpha0) LoadLevel("level1");
            if (e.keyCode == KeyCode.Alpha1) LoadLevel("level2");
            if (e.keyCode == KeyCode.Alpha2) LoadLevel("level3");
            if (e.keyCode == KeyCode.Alpha3) LoadLevel("level4");
            if (e.keyCode == KeyCode.Alpha4) LoadLevel("level5");
            if (e.keyCode == KeyCode.Alpha5) LoadLevel("level6");
            if (e.keyCode == KeyCode.Alpha6) LoadLevel("level7");
            if (e.keyCode == KeyCode.Alpha7) LoadLevel("level8");
            if (e.keyCode == KeyCode.Alpha8) LoadLevel("level9");
            if (e.keyCode == KeyCode.Alpha9) LoadLevel("final_level");
        }
    }
}
