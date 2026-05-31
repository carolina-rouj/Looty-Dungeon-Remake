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
    public GameObject wizardPrefab;
    public GameObject gnomePrefab;
    public GameObject bossPrefab;

    // trampas del nivel
    public GameObject rastroSlimePrefab;
    public GameObject spiderWebsPrefab;
    public GameObject forkContainerPrefab;
    public GameObject forkMeshPrefab;
    public GameObject arrowShootPrefab;
    public GameObject dianaPrefab;
    public GameObject arrowProjectilePrefab;

    // paredes del nivel
    public GameObject wallPlainPrefab;
    public GameObject wallDoorPrefab;
    public GameObject doorPrefab;
    public Vector3 wallScale = Vector3.one;

    // decoraciones del nivel
    public GameObject thronePrefab;
    public GameObject floorTorchPrefab;
    public GameObject floorTorchFirePrefab;
    public GameObject calizPrefab;
    public GameObject cauldronPrefab;
    public GameObject bookshelfPrefab;
    public GameObject barrelPrefab;
    public GameObject carpetPrefab;
    public GameObject tapestryPrefab;

    // altura Y de spawn para que los enemigos no traspasen el suelo
    public float enemySpawnHeight = 1f;
    // altura Y de spawn para que las decoraciones queden encima del cubo del suelo
    public float decorationSpawnHeight = 1.0f;

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
    private Dictionary<string, GameObject> decorationPrefabs;
    private Dictionary<int, List<GameObject>> tilesByRow = new Dictionary<int, List<GameObject>>();
    private Dictionary<int, List<GameObject>> objectsByRow = new Dictionary<int, List<GameObject>>();
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
            { "Wizard", wizardPrefab },
            { "Gnome",  gnomePrefab  },
            { "Boss",   bossPrefab   },
        };

        trapPrefabs = new Dictionary<string, GameObject>
        {
            { "RastroSlime", rastroSlimePrefab },
        };

        decorationPrefabs = new Dictionary<string, GameObject>
        {
            { "Throne",     thronePrefab     },
            { "FloorTorch", floorTorchPrefab },
            { "Caliz",      calizPrefab      },
            { "Cauldron",   cauldronPrefab   },
            { "Bookshelf",  bookshelfPrefab  },
            { "Barrel",     barrelPrefab     },
            { "Carpet",     carpetPrefab     },
            { "Tapestry",   tapestryPrefab   },
        };

        LoadAndBuild();
    }

    // Borra el nivel actual antes de cargar otro
    void ClearLevel()
    {
        StopAllCoroutines();
        fallingCoroutine = null;
        tilesByRow.Clear();
        objectsByRow.Clear();

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (GameObject go in spawnedObjects)
            if (go != null) Destroy(go);

        spawnedObjects.Clear();

        foreach (var obj in FindObjectsByType<RastroSlime>(FindObjectsSortMode.None))
            Destroy(obj.gameObject);

        foreach (var obj in FindObjectsByType<Arrow>(FindObjectsSortMode.None))
            Destroy(obj.gameObject);

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
        BuildWalls();
        SpawnEnemies();
        SpawnTraps();
        SpawnDecorations();

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

                // SpiderWebs trap provides its own floor block, so skip the normal tile
                if (levelData.traps != null)
                {
                    bool replaced = false;
                    foreach (var t in levelData.traps)
                        if (t.type == "SpiderWebs" && t.col == c && t.row == r) { replaced = true; break; }
                    if (replaced) continue;
                }

                Vector3 tilePos = GridToWorld(c, r);
                tilePos.y = levelData.floorYOffset;
                GameObject tile = Instantiate(prefabToUse, tilePos, Quaternion.identity, transform);
                tile.layer = LayerMask.NameToLayer("Floor");
                tile.name  = $"Tile_{c}_{r}";

                if (!tilesByRow.ContainsKey(r))
                    tilesByRow[r] = new List<GameObject>();
                tilesByRow[r].Add(tile);
            }
        }
    }

    void BuildWalls()
    {
        if (levelData.walls == null) return;
        foreach (var spawn in levelData.walls)
        {
            Vector3 pos = GridToWorld(spawn.col, spawn.row);
            pos.y = 0f;
            Quaternion rot = Quaternion.Euler(0f, spawn.rotation, 0f);

            bool isDoor = spawn.type == "Door";
            GameObject wallPrefab = isDoor ? wallDoorPrefab : wallPlainPrefab;

            if (wallPrefab == null)
            {
                Debug.LogWarning($"[LevelManager] Prefab de pared '{spawn.type}' no asignado.");
                continue;
            }

            Vector3 scale = wallScale;
            scale.z = spawn.width > 0f ? spawn.width * tamañoCasilla : wallScale.z;
            GameObject w = Instantiate(wallPrefab, pos, rot);
            w.transform.localScale = scale;
            spawnedObjects.Add(w);
            RegisterRowObject(w, spawn.row);   // que la pared caiga con su fila

            if (isDoor && doorPrefab != null)
            {
                GameObject door = Instantiate(doorPrefab, w.transform);
                door.transform.localPosition = Vector3.zero;
                door.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
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
            if (!objectsByRow.ContainsKey(spawn.row))
                objectsByRow[spawn.row] = new List<GameObject>();
            objectsByRow[spawn.row].Add(spawnedObjects[spawnedObjects.Count - 1]);
        }
    }

    void SpawnTraps()
    {
        if (levelData.traps == null) return;
        foreach (var spawn in levelData.traps)
        {
            if (spawn.type == "ArrowShoot")
            {
                SpawnArrowTrap(spawn);
                continue;
            }
            if (spawn.type == "RetractileFork")
            {
                SpawnRetractileFork(spawn);
                continue;
            }
            if (spawn.type == "SpiderWebs")
            {
                SpawnSpiderWebs(spawn);
                continue;
            }
            if (!trapPrefabs.TryGetValue(spawn.type, out GameObject prefab) || prefab == null)
            {
                Debug.LogWarning($"[LevelManager] Prefab de trampa '{spawn.type}' no asignado.");
                continue;
            }
            Vector3 trapPos = GridToWorld(spawn.col, spawn.row);
            trapPos.y = levelData.floorYOffset;
            GameObject trapGo = Instantiate(prefab, trapPos, Quaternion.identity);
            spawnedObjects.Add(trapGo);
            RegisterRowObject(trapGo, spawn.row);
        }
    }

    void SpawnArrowTrap(TrapSpawn spawn)
    {
        if (arrowShootPrefab == null || dianaPrefab == null)
        {
            Debug.LogWarning("[LevelManager] arrowShootPrefab o dianaPrefab no asignados.");
            return;
        }

        Vector3 shooterPos = GridToWorld(spawn.col, spawn.row);
        shooterPos.y = levelData.floorYOffset + 1f;

        Vector3 dianaPos = GridToWorld(spawn.dianaCol, spawn.dianaRow);
        dianaPos.y = levelData.floorYOffset;

        Vector3 dir = dianaPos - shooterPos;
        dir.y = 0f;
        Quaternion rot = dir != Vector3.zero ? Quaternion.FromToRotation(Vector3.right, dir) : Quaternion.identity;

        GameObject shooter = Instantiate(arrowShootPrefab, shooterPos, rot);
        GameObject diana = Instantiate(dianaPrefab, dianaPos, Quaternion.identity);

        ArrowTrap trap = shooter.GetComponent<ArrowTrap>();
        Diana dianaComp = diana.GetComponent<Diana>();

        if (trap != null)
        {
            trap.diana = diana.transform;
            trap.arrowPrefab = arrowProjectilePrefab;
            if (dianaComp != null)
                dianaComp.arrowTrap = trap;
        }

        // Diana es su propio tile de suelo; registrarla en tilesByRow para fallingFloor
        if (!tilesByRow.ContainsKey(spawn.dianaRow))
            tilesByRow[spawn.dianaRow] = new List<GameObject>();
        tilesByRow[spawn.dianaRow].Add(diana);

        // El shooter cae con su fila
        if (!objectsByRow.ContainsKey(spawn.row))
            objectsByRow[spawn.row] = new List<GameObject>();
        objectsByRow[spawn.row].Add(shooter);

        spawnedObjects.Add(shooter);
        spawnedObjects.Add(diana);
        RegisterRowObject(shooter, spawn.row);
        RegisterRowObject(diana, spawn.dianaRow);
    }

    void SpawnRetractileFork(TrapSpawn spawn)
    {
        Vector3 pos = GridToWorld(spawn.col, spawn.row);
        pos.y = 1.0f; // altura de pared (centro del tile de pared)

        GameObject root = new GameObject("RetractileFork");
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(0f, spawn.rotation, 0f);

        if (forkContainerPrefab != null)
        {
            GameObject container = Instantiate(forkContainerPrefab, root.transform);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }

        GameObject forkObj = null;
        if (forkMeshPrefab != null)
        {
            forkObj = Instantiate(forkMeshPrefab, root.transform);
            forkObj.transform.localPosition = new Vector3(0.3f, -0.8f, 0f); // dentro del contenedor
            forkObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        RetractileFork script = root.AddComponent<RetractileFork>();
        if (forkObj != null) script.forkMesh = forkObj.transform;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size   = new Vector3(1.1f, 0.5f, 0.5f); // orientado en X (dirección del pinchazo)
        col.center = new Vector3(0.55f, 0f, 0f);     // delante del contenedor

        if (!objectsByRow.ContainsKey(spawn.row))
            objectsByRow[spawn.row] = new List<GameObject>();
        objectsByRow[spawn.row].Add(root);

        spawnedObjects.Add(root);
        RegisterRowObject(root, spawn.row);
    }

    void SpawnSpiderWebs(TrapSpawn spawn)
    {
        Vector3 pos = GridToWorld(spawn.col, spawn.row);
        pos.y = levelData.floorYOffset;

        GameObject root = new GameObject("SpiderWebs");
        root.transform.position = pos;

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size   = new Vector3(0.85f, 1.0f, 0.85f);
        col.center = new Vector3(0f, 1.5f, 0f);

        SpiderWebs script = root.AddComponent<SpiderWebs>();
        script.tela1Prefab = spiderWebsPrefab;

        // Colisionador de suelo invisible (sin mesh) en capa "Floor" para HasFloorAt.
        // BuildFloor salta esta casilla, así que lo creamos aquí manualmente.
        GameObject floorDetector = new GameObject("FloorDetector");
        floorDetector.transform.SetParent(root.transform, false);
        floorDetector.layer = LayerMask.NameToLayer("Floor");
        BoxCollider floorCol = floorDetector.AddComponent<BoxCollider>();
        floorCol.size   = new Vector3(1f, 1f, 1f);
        floorCol.center = Vector3.zero;

        if (!objectsByRow.ContainsKey(spawn.row))
            objectsByRow[spawn.row] = new List<GameObject>();
        objectsByRow[spawn.row].Add(root);

        spawnedObjects.Add(root);
        RegisterRowObject(root, spawn.row);
    }

    void SpawnDecorations()
    {
        if (levelData.decorations == null) return;
        foreach (var spawn in levelData.decorations)
        {
            if (!decorationPrefabs.TryGetValue(spawn.type, out GameObject prefab) || prefab == null)
            {
                Debug.LogWarning($"[LevelManager] Prefab de decoración '{spawn.type}' no asignado.");
                continue;
            }
            Quaternion rot = Quaternion.Euler(0f, spawn.rotation, 0f);
            Vector3 pos = GridToWorld(spawn.col, spawn.row);
            pos.y = decorationSpawnHeight + spawn.yOffset;
            GameObject decoration = Instantiate(prefab, pos, rot);
            if (spawn.scale != 1f)
                decoration.transform.localScale *= spawn.scale;
            spawnedObjects.Add(decoration);

            // Dar colisión a las decoraciones que no traen collider del prefab.
            // El box se ancla en world Y=0 (suelo) hasta Y=1.2, independientemente de
            // la altura de spawn, para que el slime pueda detectarlo en ambas alturas.
            if (spawn.type != "Carpet" && decoration.GetComponent<Collider>() == null)
            {
                BoxCollider box = decoration.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 0.6f - pos.y, 0f);
                box.size   = new Vector3(0.7f, 1.2f, 0.7f);
            }

            if (spawn.type == "FloorTorch" && floorTorchFirePrefab != null)
            {
                GameObject fire = Instantiate(floorTorchFirePrefab, decoration.transform);
                // El prefab tiene escala (0.4,0.4,0.4). localPosition.y=0.9 coloca el fuego
                // en la punta del modelo (0.9*0.4 = 0.36u en espacio mundo)
                fire.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                fire.transform.localRotation = Quaternion.identity;
            }

            if (spawn.type == "Cauldron" && floorTorchFirePrefab != null)
            {
                GameObject steam = Instantiate(floorTorchFirePrefab, decoration.transform);
                steam.transform.localPosition = new Vector3(0f, 0.65f, 0f);
                steam.transform.localRotation = Quaternion.identity;
                var fireCtrl = steam.GetComponent<YourNamespace.VFX_FireController>();
                if (fireCtrl != null)
                {
                    fireCtrl.SetFireColor(new Color(0.85f, 0.95f, 1f));
                    fireCtrl.SetFireIntensity(1.0f);
                }
            }

            if (spawn.type == "Caliz" && floorTorchFirePrefab != null)
            {
                GameObject mana = Instantiate(floorTorchFirePrefab, decoration.transform);
                mana.transform.localPosition = new Vector3(0f, 1.1f, 0f);
                mana.transform.localRotation = Quaternion.identity;
                var fireCtrl = mana.GetComponent<YourNamespace.VFX_FireController>();
                if (fireCtrl != null)
                {
                    fireCtrl.SetFireColor(new Color(0.2f, 0.5f, 1.0f));
                    fireCtrl.SetFireIntensity(1.2f);
                }
            }

            if (!objectsByRow.ContainsKey(spawn.row))
                objectsByRow[spawn.row] = new List<GameObject>();
            objectsByRow[spawn.row].Add(decoration);
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

    // Mateus: ¿hay una decoracion, trampa o pared en esta casilla? Sirve para no soltar
    // monedas encima de un caliz / barril / trampa, etc. (que quedaban montados unos sobre
    // otros). No cuenta enemigos porque se mueven y dejan la casilla libre.
    public bool IsCellOccupied(int col, int row)
    {
        if (levelData == null) return false;
        if (levelData.decorations != null)
            foreach (var d in levelData.decorations)
                if (d.col == col && d.row == row) return true;
        if (levelData.walls != null)
            foreach (var w in levelData.walls)
                if (w.col == col && w.row == row) return true;
        if (levelData.traps != null)
            foreach (var t in levelData.traps)
            {
                if (t.col == col && t.row == row) return true;
                if (t.type == "ArrowShoot" && t.dianaCol == col && t.dianaRow == row) return true;
            }
        return false;
    }

    // Mateus: registra un objeto para que CAIGA junto con su fila cuando el suelo se
    // desploma (mismo mecanismo que enemigos/decoraciones). Asi trampas y obstaculos no
    // quedan flotando en el vacio: todo tiene "gravedad" cuando se cae el suelo.
    void RegisterRowObject(GameObject go, int row)
    {
        if (go == null) return;
        if (!objectsByRow.ContainsKey(row))
            objectsByRow[row] = new List<GameObject>();
        objectsByRow[row].Add(go);
    }

    // Mateus: gracia inicial antes de que el suelo EMPIECE a caer. Antes la primera fila
    // (donde aparece el jugador) se desplomaba a los ~2s de entrar, y se perdia una vida
    // nada mas empezar el nivel. Con esto el jugador tiene tiempo de orientarse y arrancar.
    private const float StartFallGrace = 5f;

    // codigo para que el suelo caiga por filas
    IEnumerator FallingFloors()
    {
        yield return new WaitForSeconds(StartFallGrace);

        for (int r = 0; r < gridRows; r++)
        {
            yield return new WaitForSeconds(2f);   // pausa silenciosa antes del aviso

            List<GameObject> rowAll = new List<GameObject>();
            if (tilesByRow.ContainsKey(r))
            {
                rowAll.AddRange(tilesByRow[r]);
                tilesByRow.Remove(r);
            }
            if (objectsByRow.ContainsKey(r))
            {
                rowAll.AddRange(objectsByRow[r]);
                objectsByRow.Remove(r);
            }
            float rowZ = GridToWorld(0, r).z;
            foreach (RastroSlime rastro in FindObjectsByType<RastroSlime>(FindObjectsSortMode.None))
            {
                if (Mathf.Abs(rastro.transform.position.z - rowZ) < tamañoCasilla * 0.5f)
                    rowAll.Add(rastro.gameObject);
            }
            if (rowAll.Count > 0)
                StartCoroutine(ShakeAndFallRow(rowAll));

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

        for (int i = 0; i < tiles.Count; i++)
            if (tiles[i] != null) SpawnDebris(origins[i], 1);

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

        // caída física descoordinada: cada bloque cae con retardo aleatorio
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == null) continue;
            StartCoroutine(FallTileDelayed(tiles[i], origins[i], Random.Range(0f, 0.6f)));
        }
    }

    IEnumerator FallTileDelayed(GameObject tile, Vector3 origin, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tile == null) yield break;
        tile.transform.position = origin;
        Collider col = tile.GetComponent<Collider>();
        if (col) col.enabled = false;
        SpawnDebris(origin);
        Rigidbody rb = tile.AddComponent<Rigidbody>();
        rb.useGravity = true;
        Destroy(tile, 2f);
    }

    void SpawnDebris(Vector3 position, int count = -1)
    {
        if (count < 0) count = Random.Range(2, 4);
        for (int i = 0; i < count; i++)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float size = Random.Range(0.04f, 0.08f);
            piece.transform.localScale = Vector3.one * size;
            piece.transform.position = position + new Vector3(
                Random.Range(-0.25f, 0.25f),
                Random.Range(0.1f, 0.3f),
                Random.Range(-0.25f, 0.25f));
            piece.transform.rotation = Random.rotation;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(
                Random.Range(0.72f, 0.82f),
                Random.Range(0.62f, 0.72f),
                Random.Range(0.85f, 0.95f)));
            piece.GetComponent<MeshRenderer>().sharedMaterial = mat;

            Destroy(piece.GetComponent<BoxCollider>());

            Rigidbody rb = piece.AddComponent<Rigidbody>();
            rb.AddForce(new Vector3(
                Random.Range(-1.5f, 1.5f),
                Random.Range(0.5f, 1.5f),
                Random.Range(-1.5f, 1.5f)), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);

            Destroy(piece, 1.5f);
        }
    }

    // NOTA: el salto de nivel con teclas 0-9 lo gestiona DungeonGameRuntime (que ademas
    // actualiza el estado del juego, el HUD y reposiciona al player). Antes habia aqui un
    // OnGUI que cargaba niveles directamente y puenteaba al runtime: eso desincronizaba la
    // barra de progreso del HUD y provocaba dobles cargas. Se elimino a proposito.
}
