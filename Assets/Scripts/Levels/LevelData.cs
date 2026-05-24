using System;

[Serializable]
public class LevelData
{
    public string name;
    public float tamañoCasilla = 1.0f;
    public string[] grid;          // una string por fila; '#' = suelo
    public EnemySpawn[] enemies;
    public TrapSpawn[] traps;
}

[Serializable]
public class EnemySpawn
{
    public string type;   // "Slime", "Bat", "Wizard", "Gnome", "Boss"
    public int col;       // columna (eje X)
    public int row;       // fila    (eje Z)
}

[Serializable]
public class TrapSpawn
{
    public string type;   // "RastroSlime", ...
    public int col;
    public int row;
}
