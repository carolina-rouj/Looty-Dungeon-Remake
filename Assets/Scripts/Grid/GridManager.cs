using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    public int gridSize = 10;
    public float tamañoCasilla = 1.0f;   // misma convención que Slime.cs
    public GameObject floorPrefab;        // asignar Assets/Prefabs/floor.prefab en el Inspector

    public static GridManager Instance { get; private set; }
    public float MinBound { get; private set; }
    public float MaxBound { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // offset centra el grid en el origen:
        // ej. gridSize=10, tamañoCasilla=1 → offset=4.5 → tiles van de -4.5 a +4.5
        float offset = (gridSize * tamañoCasilla) / 2f - tamañoCasilla / 2f;
        MinBound = -offset;
        MaxBound =  offset;

        GenerateGrid(offset);
    }

    void GenerateGrid(float offset)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 pos = new Vector3(
                    x * tamañoCasilla - offset,
                    0f,
                    z * tamañoCasilla - offset
                );

                GameObject tile = Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                tile.layer = LayerMask.NameToLayer("Floor");
                tile.name = $"Tile_{x}_{z}";
            }
        }
    }

    // Comprueba si una posición del mundo está dentro del grid
    public bool IsInBounds(Vector3 p) =>
        p.x >= MinBound && p.x <= MaxBound &&
        p.z >= MinBound && p.z <= MaxBound;

    // Redondea una posición al centro de casilla más cercano.
    // Los centros están en posiciones .5 (ej: -4.5, -3.5, … 4.5),
    // así que la fórmula correcta es Floor + mitad de casilla.
    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        float x = Mathf.Floor(worldPos.x / tamañoCasilla) * tamañoCasilla + tamañoCasilla * 0.5f;
        float z = Mathf.Floor(worldPos.z / tamañoCasilla) * tamañoCasilla + tamañoCasilla * 0.5f;
        return new Vector3(x, worldPos.y, z);
    }
}
