using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public string levelFileName = "level2";
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
    private Dictionary<string, GameObject> enemyPrefabs;
    private Dictionary<string, GameObject> trapPrefabs;
    private Dictionary<int, List<GameObject>> tilesByRow = new Dictionary<int, List<GameObject>>();
    private Coroutine fallingCoroutine;

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
            { "RetractileFork", retractileForkPrefab },
        };

        LoadAndBuild();
    }

    // Borra el nivel actual antes de cargar otro
    void ClearLevel()
    {
        StopAllCoroutines();
        fallingCoroutine = null;
        tilesByRow.Clear();

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (GameObject go in spawnedObjects)
            if (go != null) Destroy(go);

        spawnedObjects.Clear();
        levelData = null;
        gridRows = 0;
        gridCols = 0;
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
        SpawnEnemies();
        SpawnTraps();

        if (levelData.fallingFloor)
            fallingCoroutine = StartCoroutine(FallingFloors());

        Debug.Log($"[LevelManager] Nivel '{levelData.name}' cargado.");
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
                tile.layer = LayerMask.NameToLayer("Floor");
                tile.name  = $"Tile_{c}_{r}";

                if (!tilesByRow.ContainsKey(r))
                    tilesByRow[r] = new List<GameObject>();
                tilesByRow[r].Add(tile);
            }
        }
    }

    void SpawnEnemies()
    {
        if (levelData.enemies == null) return;
        foreach (var spawn in levelData.enemies)
        {
            if (!enemyPrefabs.TryGetValue(spawn.type, out GameObject prefab) || prefab == null)
            {
                Debug.LogWarning($"[LevelManager] Prefab de enemigo '{spawn.type}' no asignado.");
                continue;
            }
            Vector3 pos = GridToWorld(spawn.col, spawn.row);
            pos.y = enemySpawnHeight;
            spawnedObjects.Add(Instantiate(prefab, pos, Quaternion.identity));
        }
    }

    void SpawnTraps()
    {
        if (levelData.traps == null) return;
        foreach (var spawn in levelData.traps)
        {
            if (!trapPrefabs.TryGetValue(spawn.type, out GameObject prefab) || prefab == null)
            {
                Debug.LogWarning($"[LevelManager] Prefab de trampa '{spawn.type}' no asignado.");
                continue;
            }
            spawnedObjects.Add(Instantiate(prefab, GridToWorld(spawn.col, spawn.row), Quaternion.identity));
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

    // codigo para que el suelo caiga por filas
    IEnumerator FallingFloors()
    {
        for (int r = 0; r < gridRows; r++)
        {
            yield return new WaitForSeconds(2f);   // pausa silenciosa antes del aviso

            if (tilesByRow.ContainsKey(r))
            {
                List<GameObject> rowTiles = tilesByRow[r];
                tilesByRow.Remove(r);
                StartCoroutine(ShakeAndFallRow(rowTiles));
            }

            yield return new WaitForSeconds(1f);   // espera el shake antes de pasar a la siguiente fila
        }
        fallingCoroutine = null;
    }

    IEnumerator ShakeAndFallRow(List<GameObject> tiles)
    {
        // temblor (1s)
        float elapsed = 0f;
        float shakeDuration = 1f;
        float shakeMag = 0.07f;

        Vector3[] origins = new Vector3[tiles.Count];
        for (int i = 0; i < tiles.Count; i++)
            if (tiles[i] != null) origins[i] = tiles[i].transform.position;

        while (elapsed < shakeDuration)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] == null) continue;
                float ox = Mathf.Sin((elapsed + i * 0.3f) * 22f) * shakeMag;
                float oz = Mathf.Cos((elapsed + i * 0.5f) * 17f) * shakeMag * 0.5f;
                tiles[i].transform.position = origins[i] + new Vector3(ox, 0f, oz);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // caída física del bloque
        for (int i = 0; i < tiles.Count; i++)
        {
            GameObject tile = tiles[i];
            if (tile == null) continue;
            tile.transform.position = origins[i];
            Collider col = tile.GetComponent<Collider>();
            if (col) col.enabled = false;
            Rigidbody rb = tile.AddComponent<Rigidbody>(); 
            rb.useGravity = true;
            Destroy(tile, 2f);
        }
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown) {
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
