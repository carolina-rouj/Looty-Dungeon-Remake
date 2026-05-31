using System;

[Serializable]
public class LevelData
{
    public string name;
    public float tamañoCasilla = 1.0f;
    public float floorYOffset = 0f;
    public string[] grid;
    public EnemySpawn[] enemies;
    public TrapSpawn[] traps;
    public WallSpawn[] walls;
    public DecorationSpawn[] decorations;
    public bool fallingFloor;      // Si es true, el suelo se cae fila a fila cada 3s
}

[Serializable]
public class EnemySpawn
{
    public string type;   // "Slime", "Wizard", "Gnome", "Boss"
    public int col;       // columna
    public int row;       // fila
}

[Serializable]
public class TrapSpawn
{
    public string type;
    public int col;
    public int row;
    public int dianaCol;
    public int dianaRow;
    public float rotation; // rotación Y del contenedor en grados (usado por RetractileFork)
}

[Serializable]
public class WallSpawn
{
    public int col;
    public int row;
    public int rotation; // 0=front(-Z), 90=left(-X), 180=back(+Z), 270=right(+X)
    public string type;  // "Plain" (default) | "Door"
    public float width = 1f;
}

[Serializable]
public class DecorationSpawn
{
    public string type;    // "Throne", "FloorTorch", "Caliz"
    public int col;
    public int row;
    public float rotation; // rotación en el eje Y (0, 90, 180, 270)
    public float yOffset;  // altura extra sobre decorationSpawnHeight (por defecto 0)
    public float scale = 1f;
}
