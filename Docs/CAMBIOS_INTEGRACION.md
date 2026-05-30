# Looty Dungeon 3D — Estado e integración (notas para la memoria)

> Documento de seguimiento para poder actualizar la **memoria** (que ya está hecha
> pero le faltan cambios). Complementa a [`estructura.md`](estructura.md).

## Datos del proyecto
- **Juego:** Looty Dungeon 3D (versión propia del *Looty Dungeon*).
- **Asignatura:** VJ — Videojocs (FIB · UPC), curso 2025/26 Q2.
- **Autores:** Carolina Rodríguez Ujano y Mateus Grandolfi Albuquerque.
- **Profesor tutor:** Oscar Argudo Medrano.
- **Motor:** Unity **6000.0.74f1** (URP).

## Arquitectura (dos capas complementarias)
El proyecto se divide en dos capas que encajan sin duplicarse:

- **Capa de entorno (Carolina)** — `Scripts/Levels/`, `Scripts/Enemies/` (IA), `Scripts/Traps/`:
  niveles por JSON (`Resources/Levels/`), `LevelManager` que monta suelo, paredes,
  decoración, enemigos y trampas desde el grid, y el suelo que cae por filas.
- **Capa de juego (Mateus)** — `Scripts/Player/`, `Scripts/Game/`, `Scripts/Items/`,
  y *glue* de enemigos en `Scripts/Enemies/`: jugador (movimiento, dash, combate,
  3 corazones), cámara ortográfica, HUD, menús/créditos, audio/SFX, monedas, puerta,
  partículas y estados de partida (menú/jugar/pausa/game over/victoria).

**Cómo encajan:** el combate del jugador llama a `enemy.Hurt()` (contrato de los
enemigos de Carolina) y el jugador se llama `"Player"` (lo que sus enemigos esperan).
El runtime descubre los enemigos que spawnea el `LevelManager` y les engancha el glue
(daño por contacto, flash al golpear, detección de muerte) **sin modificar** sus scripts.

## Niveles (10 salas, dificultad creciente)
- **Pares (Carolina):** `level2, level4, level6, level8` + **`final_level`** (boss).
- **Impares (Mateus):** `level1, level3, level5, level7, level9`.
- **Suelo que cae:** activo desde la **sala 3** (incl.) hasta la sala 9, excluyendo la
  del boss (flag `fallingFloor` en el JSON de cada nivel: level3/4/5/6/7/8/9 y final).
- Teclas **0–9** saltan directamente a cada sala (0→sala1 … 9→boss).

## Controles
- **Mover:** WASD / flechas. **Dash:** Shift. **Atacar:** Espacio / clic izquierdo.
- **Pausa:** P / Esc. **Menú:** botón "Menu" o M. **Jugar/Créditos:** desde el menú.

## Requisitos del enunciado — estado
| Requisito | Estado |
|---|---|
| 10 salas de dificultad creciente | ✔ (pares Carolina + impares Mateus) |
| Puerta que abre al limpiar la sala | ✔ |
| 3 enemigos distintos | ⚠ Slime ✔ (con rastro que ralentiza); Bat/Gnome en JSON, **pendiente asignar sus prefabs** en el `LevelManager` de la escena |
| Enemigo que deja rastro ralentizador | ✔ (Slime → RastroSlime) |
| Boss en la sala final | ✔ (lógica) — pendiente afinar |
| Jugador ataca y elimina enemigos | ✔ |
| Enemigos dañan al jugador | ✔ (daño por contacto) |
| 3 corazones / perder al quedarse sin vida | ✔ |
| Suelo que cae desde la sala 3 | ✔ |
| Cámara ortográfica estilo Looty | ✔ |
| Trampas (de entre 3) | ✔ RetractileFork / RastroSlime |
| Monedas | ⚠ sistema de recogida hecho; falta colocarlas/soltarlas |
| ≥5 decoraciones distintas | ⚠ en JSON (FloorTorch, Caliz, Barrel…); **pendiente asignar prefabs** |
| HUD (vida, monedas, salas) | ✔ |
| Volver al menú desde el juego | ✔ |
| 3 pantallas (menú, jugar, créditos) | ✔ |
| Sonido y música | ✔ (sintetizados en runtime) |
| Teclas 0–9 a cada nivel | ✔ |

## Pendiente (wiring de escena / contenido)
- Asignar en el `LevelManager` de `LevelScene` los prefabs de **Bat, Wizard, Gnome,
  Boss** y de las **decoraciones** (Barrel, Bookshelf, Cauldron, Carpet, Tapestry) que
  faltan, para que aparezcan los que ya están referenciados en los JSON.
- Colocar/soltar **monedas**.
- Pulir el comportamiento del **boss** y la IA de persecución de los enemigos
  (en sus scripts está preparada y comentada con `// TODO (player)`).

## Registro de cambios de la integración (rama `mateus-integration` → PR a `develop`)
1. Reintroducido el código de gameplay de Mateus **reestructurado** (1 clase = 1
   fichero) sobre la estructura actual de Carolina, de forma aditiva.
2. Eliminado lo que duplicaba el trabajo de Carolina (fábrica procedural de enemigos
   y trampas, proyectil) — mandan sus prefabs/scripts.
3. Adaptado el runtime a la API real de su `LevelManager` (spawn/salida/cámara desde
   los límites del nivel y tiles de suelo reales; el suelo que cae lo dirige su JSON).
4. Arreglado `FindObjectsByType<T>` (requiere `FindObjectsSortMode` en Unity 6000.0.74).
5. `activeInputHandler` → **Both** (el código usa el Input clásico; el proyecto estaba
   en Input System only y lanzaba excepción cada frame).
6. **Build Settings** apuntaba a `SampleScene` (inexistente) → corregido a `LevelScene`.
7. **Rellenados los niveles impares** (estaban vacíos) en el formato JSON de Carolina.
8. HUD y menús/créditos **más grandes y legibles**; créditos con autores y tutor.
