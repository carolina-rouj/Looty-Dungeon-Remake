# Looty Dungeon 3D

A 3D dungeon-crawler built in Unity for the **VJ (Videojocs)** course.

The project is an original take on **Looty Dungeon** (Tinytouchtales, 2016), a grid-based isometric rogue-lite for mobile. It keeps the original's core feel — direct cell-by-cell movement and constant pressure from a collapsing floor — while building it as a full 3D, voxel-styled experience.

## Authors

- Carolina Rodriguez Ujano
- Mateus Grandolfi Albuquerque

## Gameplay Overview

The player advances through a dungeon of **10 rooms** of increasing difficulty. Each room has an objective — defeat all enemies, or collect a target number of coins (rooms 3, 6 and 9) — that must be completed to open the door to the next room. The run is **won** by defeating the boss in the final room, and **lost** if the player runs out of hearts; in both cases the game can be restarted from room 1.

### Controls

| Input | Action |
|---|---|
| `WASD` / Arrow keys | Move one cell in a direction (hold to chain moves) |
| `Space` / Left click | Attack in the direction the player is facing |
| `P` / `Esc` | Pause (from pause: `Enter`/`P` resume, `M` returns to menu) |
| `0`–`9` | Jump directly to the corresponding room (resets it with enemies and objective) |
| `R` / `Enter` (on Victory/Game Over screens) | Restart from room 1 |

## Features

- **10 rooms** of increasing difficulty, defined in JSON (`Assets/Resources/Levels/`).
- **Conditional doors** — locked until the room's objective is completed.
- **Four enemy types**: Slime, Gnome, Wizard (ranged), and a multi-hit Boss in the final room.
- **Slime trail** that slows the player when stepped on.
- **Three hearts** — each hit costs one heart; enemies occasionally drop heart pickups.
- **Falling floor** mechanic from room 3 through room 9 (the boss room is exempt), with a short grace period before it starts collapsing.
- **Four trap types**: arrow trap (launcher + target), retractable spike fork, spider webs, and the slime trail.
- **Isometric orthographic camera** with smooth, room-bounded follow, matching the original game's framing.
- **Full HUD**: hearts, coin counter, rooms cleared, current room, timer, 10-room progress bar, and active objective.
- **Procedural audio**: distinct background music for menu, gameplay, and boss fight, plus per-action sound effects.
- **Game feel polish**: attack slashes, camera shake, floating damage numbers, hit particles, delayed enemy death, damage flash/red overlay, animated coins/hearts, floor-collapse warnings, room fade transitions, and per-section ambient lighting.

## Architecture

The codebase is split into two complementary layers, which allowed the team to work in parallel without stepping on each other's work:

- **Environment layer** — `LevelManager`, enemy/trap scripts, and JSON-defined levels. Builds room geometry (floor, walls, decorations), spawns enemies and traps, and manages the row-by-row floor collapse.
- **Gameplay layer** — `DungeonGameRuntime` plus `Player/`, `Game/`, and `Items/` scripts. Handles the player, combat, health, HUD, menus, audio, camera, door, coins, objectives, and global game state.

The two layers connect through a simple contract: the player's combat calls `Hurt()` on enemy scripts, and the runtime discovers enemies instantiated by `LevelManager` to hook up death detection and feedback.

Game states (menu, credits, playing, paused, game over, victory) are managed by the `DungeonState` enum in `DungeonGameRuntime`.

## Tech Stack

- **Engine:** Unity `6000.0.74f1` (URP render pipeline)
- **Language:** C# 
- **Version control:** Git / GitHub
- **Modeling:** MagicaVoxel (for original voxel assets)

## Assets & Credits

- **Original models** (MagicaVoxel): most of the level geometry — floor, walls, door, throne, chalice, target, spike fork, and more.
- **Asset Store packages**:
  - [VoxBox](https://assetstore.unity.com/packages/3d/characters/voxbox-voxel-game-assets-182002) — arrows
  - [Voxel Castle Pack Lite](https://assetstore.unity.com/packages/3d/environments/historic/voxel-castle-pack-lite-164189) — bookshelves, cauldron, carpet, barrels
  - [Full Opaque Fire](https://assetstore.unity.com/packages/vfx/full-opaque-fire-312221) — torch fire, chalice light, cauldron smoke
- **Procedural visuals**: the player and enemies without a dedicated model (slime, gnome, wizard, boss) are built at runtime from primitives and URP materials (`PlayerVisualBuilder`, `EnemyVisualBuilder`); music and sound effects are also procedurally synthesized.
- A [YouTube tutorial](https://youtu.be/_5pxcUykXcA) was used only as orientation for enemy/player behavior — no external code was reused.

## Getting Started

1. Clone the repository.
2. Open the project in **Unity 6000.0.74f1** (or a compatible Unity 6 version) with the URP package installed.
3. Open the `Menu` scene and press Play.

## License

Course assignment for VJ (Videojocs), FIB – UPC. See individual asset licenses in the Asset Store links above for third-party content.
