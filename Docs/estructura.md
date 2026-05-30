# Estructura y funcionamiento del proyecto — VJ-3D

Dungeon crawler en Unity 6 (6000.0.74f1) con URP.  
Equipo: Carolina + Mateus. 9 niveles + nivel final (boss).

---

## Estructura de carpetas

```
Assets/
  Materials/              ← materiales del proyecto (paredes, alfombras…)
  Models/                 ← modelos 3D propios (caliz, floorTorch, throne,
                             barrel, bookshelf, wall, wallBroken…)
  Prefabs/                ← prefabs del juego (un prefab por objeto/enemigo/trampa)
  Resources/
    Levels/               ← niveles en JSON (level1.json … level9.json, final_level.json)
  Scenes/
    LevelScene.unity      ← única escena de juego actualmente
  Scripts/
    Enemies/              ← Boss, Gnome, Slime, Wizard
    Grid/                 ← sistema de cuadrícula
    Levels/               ← LevelData, LevelManager
    Traps/                ← Arrow, ArrowTrap, Diana, RastroSlime,
                             RetractileFork, SpiderWebs
  Settings/               ← assets de configuración URP
  Shaders/
  Textures/
  VoxBox/                 ← pack de assets 3D de terceros (no modificar)
```

---

## Cómo funciona el proyecto

### Sistema de niveles

Cada nivel es un archivo JSON en `Assets/Resources/Levels/`. Se carga con `LevelManager.LoadLevel("nombre")`, que lee el JSON con `Resources.Load<TextAsset>()` y lo parsea a `LevelData` con `JsonUtility.FromJson`.

**Estructura de un nivel (`LevelData`):**

| Campo | Tipo | Descripción |
|---|---|---|
| `name` | string | Nombre del nivel |
| `tamañoCasilla` | float | Tamaño de cada celda de la cuadrícula (por defecto 1.0) |
| `grid` | string[] | Filas de caracteres que definen el suelo |
| `enemies` | EnemySpawn[] | Enemigos con tipo y posición en la cuadrícula |
| `traps` | TrapSpawn[] | Trampas con tipo y posición |
| `walls` | WallSpawn[] | Paredes con posición, rotación y estilo |
| `decorations` | DecorationSpawn[] | Decoración con tipo, posición, rotación y yOffset |
| `fallingFloor` | bool | Si es `true`, el suelo se cae fila a fila al iniciar |

**Caracteres del grid:**

| Carácter | Suelo |
|---|---|
| `#` | Estándar |
| `D` | Oscuro |
| `B` | Azul |
| `c` | Inicio de alfombra |
| `C` | Continuación de alfombra |

**Pipeline de carga (`LevelManager.LoadAndBuild`):**
1. Carga y parsea el JSON
2. `BuildFloor()` — instancia losetas según los caracteres del grid
3. `BuildWalls()` — instancia paredes con la rotación correcta
4. `SpawnEnemies()` — instancia enemigos desde el diccionario de prefabs
5. `SpawnTraps()` — instancia trampas
6. `SpawnDecorations()` — instancia decoraciones con rotación y yOffset
7. Si `fallingFloor = true`, arranca la corutina `FallingFloors()`

Para cambiar de nivel desde el editor: teclas 0–9 en `OnGUI` cargan `level1`–`final_level`.

### Sistema de cuadrícula

Todo se posiciona en el mundo mediante coordenadas de cuadrícula (col, row) que `LevelManager` convierte a posiciones 3D:

- `GridToWorld(col, row)` → `Vector3` en el mundo
- `SnapToGrid(worldPos)` → redondea una posición al centro de celda más cercano
- `IsInBounds(pos)` → comprueba si una posición está dentro del nivel

Los enemigos y objetos usan estas funciones para moverse y posicionarse con precisión.

### Suelo que se cae (`fallingFloor`)

Cuando está activado, la corutina `FallingFloors` va eliminando filas del suelo de arriba a abajo. Cada fila hace una animación de vibración (`ShakeAndFallRow`) antes de desaparecer. Está activado en el nivel del boss para añadir presión de tiempo.

---

## Enemigos

| Enemigo | Estado | Vidas | Comportamiento |
|---|---|---|---|
| **Slime** | Implementado | 1 | Salta entre celdas, deja rastro de slime, animación squash & stretch |
| **Gnome** | Stub (espera Player) | 1 | Perseguirá al player en rango de 4 unidades |
| **Wizard** | Stub (espera Player) | 3 | Perseguirá al player en rango de 5 unidades, ataques a distancia |
| **Boss** | Stub (espera Player) | 3 | Igual que Wizard, mecánica especial por implementar |

**Comportamiento del Slime (único completamente funcional):**
1. Espera `timeWaitMove` segundos (1s por defecto)
2. Elige una celda adyacente válida (sin obstáculos, con suelo)
3. Salta: squash → estiramiento vertical → aterrizaje
4. Al moverse deja un `RastroSlime` en la celda anterior
5. Muere con un golpe

Todos los enemigos tienen los métodos `Hurt()` y `Die()`. Los stubs tienen toda la lógica del player comentada con `// TODO (player):`.

---

## Trampas

| Trampa | Estado | Comportamiento |
|---|---|---|
| **RetractileFork** | Funcional | Ciclo: reposo → extensión (girando) → retracción. Daña al contacto durante extensión/retracción |
| **SpiderWebs** | Funcional | Ciclo: sin tela → tela parcial (paraliza) → tela completa (daña). 2s por estado |
| **RastroSlime** | Funcional (sin efecto visible) | Spawneado por el Slime. Ralentiza al player 2s. El `ApplySlow` está comentado |
| **ArrowTrap** | Stub (espera Player) | Dispara flechas hacia la diana. El intervalo de disparo escala con la distancia a la diana |

**RetractileFork — estados:**
- Reposo: 1s parado
- Extensión: 0.25s, avanza 1 celda girando a 720°/s
- Retracción: 0.25s, vuelve a posición original

**SpiderWebs — estados:**
- `Idle` (0): sin tela, 2s
- `Partial` (1): tela parcial → paraliza al player `timeParalised` segundos (1.5s)
- `Full` (2): tela completa → 1 punto de daño

---

## Decoraciones disponibles

Prefabs listos para usar en los JSONs de nivel:

`Throne`, `FloorTorch`, `Caliz`, `Cauldron`, `Bookshelf`, `Barrel`, `Carpet`, `Tapestry`, `wall`, `wallBroken`

Todos admiten `rotation` (0, 90, 180, 270) y `yOffset` en la definición de `DecorationSpawn`.

---

## Estado actual del desarrollo

**Funciona:**
- Carga y construcción de niveles desde JSON
- Suelo que se cae
- Enemigo Slime con IA completa
- Trampa RetractileFork (animación + daño)
- Trampa SpiderWebs (estados + efectos sobre Player cuando exista)
- Spawning de RastroSlime por el Slime

**Espera implementación del Player:**
- Gnome, Wizard, Boss — detección y persecución
- ArrowTrap — disparo condicionado a que el player esté en la diana
- RastroSlime — `ApplySlow()` en el Player
- SpiderWebs — `ApplyParalysis()` y `TakeDamage()` en el Player
- RetractileFork — `TakeDamage()` en el Player

---

## Dónde crear los sistemas futuros

| Sistema | Carpeta de scripts | Escenas / otros assets |
|---|---|---|
| **Player** | `Assets/Scripts/Player/` | — |
| **Objetos interactuables** | `Assets/Scripts/Interactables/` | Prefabs → `Assets/Prefabs/` |
| **Menú principal** | `Assets/Scripts/UI/MainMenu.cs` | `Assets/Scenes/MainMenu.unity` |
| **Pausa** | `Assets/Scripts/UI/PauseMenu.cs` | — |
| **Victoria / Game Over** | `Assets/Scripts/UI/Victory.cs`, `GameOver.cs` | — |
| **Vidas e ítems (HUD)** | `Assets/Scripts/UI/HUD.cs` | — |
| **Créditos** | `Assets/Scripts/UI/Credits.cs` | `Assets/Scenes/Credits.unity` (opcional) |
| **Sonido y música** | `Assets/Scripts/Audio/AudioManager.cs` | `Assets/Audio/Music/`, `Assets/Audio/SFX/` |
| **Cámara** | `Assets/Scripts/Camera/CameraController.cs` | — |

---

## Notas

- No tocar `Assets/VoxBox/` — es un asset de terceros.
- Los clips de audio van en `Assets/Audio/` separados en `Music/` y `SFX/`.
- Para añadir un nivel nuevo: crear `Assets/Resources/Levels/levelN.json` siguiendo la estructura de los existentes y añadir la tecla correspondiente en `LevelManager.OnGUI`.
