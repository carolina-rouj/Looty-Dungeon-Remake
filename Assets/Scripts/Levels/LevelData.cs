using System;

[Serializable]
public class LevelData
{
    public string name;
    public float tamañoCasilla = 1.0f;
    public string[] grid;          
    public EnemySpawn[] enemies;
    public TrapSpawn[] traps;
    public bool fallingFloor;      // Si es true, el suelo se cae fila a fila cada 3s
}

[Serializable]
public class EnemySpawn
{
    public string type;   // "Slime", "Bat", "Wizard", "Gnome", "Boss"
    public int col;       // columna 
    public int row;       // fila 
}

[Serializable]
public class TrapSpawn
{
    public string type;  
    public int col;
    public int row;
}
