# Estructura del proyecto — VJ-3D

Dungeon crawler en Unity 6 (6000.0.74f1) con URP.  
Equipo: Carolina + Mateus.

---

## Estructura actual

```
Assets/
  Materials/              ← materiales del proyecto
  Models/                 ← modelos 3D propios (caliz, floorTorch, throne…)
  Prefabs/                ← prefabs del juego (Barrel, Slime, wall, floor, floorTorch, etc.)
  Resources/
    Levels/               ← datos de niveles cargados en runtime
  Scenes/
    LevelScene.unity      ← escena principal de juego
  Scripts/
    Enemies/              ← Bat.cs, Boss.cs, Gnome.cs, Slime.cs, Wizard.cs
    Grid/                 ← sistema de cuadrícula del dungeon
    Levels/               ← LevelData.cs, LevelManager.cs
    Traps/                ← Arrow.cs, ArrowTrap.cs, Diana.cs,
                             RastroSlime.cs, RetractileFork.cs, SpiderWebs.cs
  Settings/               ← assets de configuración URP
  Shaders/                ← shaders personalizados
  Textures/               ← texturas
  VoxBox/                 ← pack de assets 3D de terceros (no modificar)
```

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
| **Sonido y música** | `Assets/Scripts/Audio/AudioManager.cs` | `Assets/Audio/` (clips de audio) |
| **Cámara** | `Assets/Scripts/Camera/CameraController.cs` | — |

---

## Notas

- Todos los scripts de UI van en `Assets/Scripts/UI/`.
- Los clips de audio (`.wav`, `.mp3`, `.ogg`) van en `Assets/Audio/`, separados en subcarpetas `Music/` y `SFX/` cuando haya suficientes archivos.
- No tocar la carpeta `Assets/VoxBox/` — es un asset de terceros.
