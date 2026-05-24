using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public string levelFileName = "level2";
    public GameObject floorPrefab;

    // Enemigos del nivel
    public GameObject slimePrefab;
    public GameObject batPrefab;
    public GameObject wizardPrefab;
    public GameObject gnomePrefab;
    public GameObject bossPrefab;

    // trampas del nivel
    public GameObject rastroSlimePrefab;

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
            { "RastroSlime", rastroSlimePrefab },
        };

        LoadAndBuild();
    }

    // Borra el nivel actual antes de cargar otro
    void ClearLevel()
    {
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
                if (rowStr[c] != '#') continue;

                if (floorPrefab == null)
                {
                    Debug.LogWarning("[LevelManager] floorPrefab no asignado.");
                    return;
                }
                GameObject tile = Instantiate(floorPrefab, GridToWorld(c, r), Quaternion.identity, transform);
                tile.layer = LayerMask.NameToLayer("Floor");
                tile.name  = $"Tile_{c}_{r}";
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

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown) {
            if (e.keyCode == KeyCode.Alpha2) LoadLevel("level2");
            if (e.keyCode == KeyCode.Alpha4) LoadLevel("level4");
            if (e.keyCode == KeyCode.Alpha6) LoadLevel("level6");
            if (e.keyCode == KeyCode.Alpha8) LoadLevel("level8");
            if (e.keyCode == KeyCode.F) LoadLevel("final_level");
        }
    }
}
