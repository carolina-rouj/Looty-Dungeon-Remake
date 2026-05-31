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
| 3 enemigos distintos | ✔ Slime, Bat, Gnome (+ Wizard) — prefabs ya cableados en la escena |
| Enemigo que deja rastro ralentizador | ✔ (Slime → RastroSlime) |
| Boss en la sala final | ✔ esqueleto rey procedural (corona, escudo, espada) |
| Jugador ataca y elimina enemigos | ✔ |
| Enemigos dañan al jugador | ✔ (daño por contacto) |
| 3 corazones / perder al quedarse sin vida | ✔ |
| Suelo que cae desde la sala 3 | ✔ |
| Cámara ortográfica estilo Looty | ✔ |
| Trampas (de entre 3) | ✔ RastroSlime (Slime). RetractileFork sin cablear; niveles impares sin trampas, como los de Carolina |
| Monedas | ✔ apoyadas en el suelo y caen con él |
| ≥5 decoraciones distintas | ✔ FloorTorch, Caliz, Barrel, Bookshelf, Carpet, Cauldron (prefabs cableados) |
| HUD (vida, monedas, salas) | ✔ |
| Volver al menú desde el juego | ✔ |
| 3 pantallas (menú, jugar, créditos) | ✔ |
| Sonido y música | ✔ (sintetizados en runtime) |
| Teclas 0–9 a cada nivel | ✔ |

## Pendiente
- Verificar en juego real (teclado) el control y el HUD (las capturas en batchmode no
  ejercitan el input ni dibujan el HUD IMGUI).
- IA de persecución de los enemigos / boss: en sus scripts está preparada y comentada con
  `// TODO (player)` (la mueve Carolina).

## Rama de trabajo
Tras aceptarse el **PR #10** (`mateus-integration`) y el **PR #11** (`carol-updating-objects`),
**todo está integrado en `develop`**, que es **a partir de ahora la rama de trabajo** del
proyecto. Se trabaja sobre `develop`; no se pisan los niveles pares de Carolina.

## Registro de cambios de la integración (PR #10 `mateus-integration` → `develop`)
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

## Ajustes post-merge en `develop` (2026-05-31)
Retoques sobre los **niveles impares de Mateus** y la capa de juego (sin tocar los pares
de Carolina):
1. **Input a prueba de backend** — nuevo `DungeonInput` (Scripts/Game) que lee por el
   Input System nuevo (`Keyboard.current`/`Mouse.current`) **y** por el clásico como
   respaldo. Antes, según la config de input, solo respondían las teclas 0–9 (que el
   `LevelManager` leía por `OnGUI`/`Event.current`) y el player no se movía/atacaba.
   Movimiento, dash, ataque, Esc/P (pausa) y 0–9 ya funcionan pase lo que pase.
2. **Saltos de nivel 0–9** centralizados en `DungeonGameRuntime` (se quitó el `OnGUI` de
   teclas del `LevelManager` que puenteaba al runtime y desincronizaba el HUD).
3. **HUD** — contador de enemigos ahora **"Enemigos: 0/X"** (abatidos/total de la sala);
   la barra/cuadrícula de salas vuelve a avanzar al pasar de nivel.
4. **Cámara** — más zoom (orthographicSize 6.2 → 4.7) y **seguimiento leve** del player
   acotado a la sala (`CameraFollowDungeon.SetSoftFollow`), estilo Looty oficial.
5. **Monedas** — apoyadas en la superficie real del suelo y **colgadas del tile**, de modo
   que caen junto al suelo cuando este se desploma (antes flotaban).
6. **Puerta** — en los niveles impares se monta la **puerta de Carolina** (marco `wallDoor`
   + hoja `door`, sin collider) sobre una compuerta funcional invisible que bloquea hasta
   limpiar la sala. En los pares la puerta ya la pone el JSON de Carolina.
7. **Decoración** — niveles impares enriquecidos imitando a Carolina (Barrel, Bookshelf,
   Carpet, Caliz, Cauldron, FloorTorch) con `yOffset 0` (antes flotaban a 0.5).
8. **Dificultad** — rampa suave fácil→difícil en armonía con Carolina (que usa ~1 enemigo
   por sala): 1 → 2 → 2 → 3 → 4 enemigos, introduciendo un tipo nuevo por nivel; sin
   trampas (su prefab no está cableado y ella tampoco las usa).
9. **Boss** — rediseñado como **esqueleto rey** procedural (cráneo, caja torácica, corona
   dorada, **escudo** y **espada**), al estilo del jefe final del Looty Dungeon original.
10. **Sonido** — síntesis chiptune/arcade (ondas cuadrada/triangular/ruido, envolventes,
    arpegios) en lugar de pitidos sinusoidales; SFX y música acercados al Looty original.
11. **Capturas** en batchmode (`DungeonScreenshotCapture`) de las 10 salas →
    `Memoria/captures/`.

## Ajustes 2ª ronda en `develop`
- **Objetivos por sala** (enunciado: "regles que explorar i objectius que aconseguir"):
  cada sala tiene un objetivo que abre la puerta al cumplirse — *derrota a todos los
  enemigos* o *recoge N monedas* — más un **reto de tiempo** opcional que da monedas extra.
  Se muestra en el HUD y al entrar en la sala. La partida se reinicia al ganar/perder (ya
  estaba). Definido en `DungeonGameRuntime.levelGoals` (también para las salas de Carolina).
- **Cámara** ahora se mueve **solo en el eje del nivel** (sigue el avance en Z, X fija al
  centro): "sigue el nivel", no al jugador.
- **HUD y pantallas** (menú/créditos/jugar/pausa) rediseñados: paleta de mazmorra, paneles
  con borde dorado (9-slice), fuente del sistema más limpia, botones con estados y texto con
  sombra. Mismo contenido y textos.
- **Boss**: acotado a la sala (su embestida ya no lo saca del mapa) y ya no atraviesa el
  suelo ni se cae cerca del trono (no se le aplica `EnemyFloorFall`; mantiene la Y sobre el
  suelo).
- **Vacío ante la puerta** (solo niveles impares de Mateus): plataforma de suelo invisible
  en la salida para no perder vida al cruzar hacia la puerta.
