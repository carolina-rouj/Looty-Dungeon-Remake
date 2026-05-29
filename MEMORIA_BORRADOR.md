# Memoria borrador - VJ 3D

> Borrador operativo. Falta convertir a formato final, anadir capturas y completar reparto definitivo Carolina/Mateus.

## 1. Datos del proyecto

- Asignatura: VJ - Videojuegos, FIB UPC.
- Proyecto: videojuego 3D tipo Looty Dungeon.
- Equipo: Carolina Rouj y Mateus.
- Motor: Unity 6000.4.4f1.
- Escena principal: `Assets/Scenes/LevelScene.unity`.
- Rama de trabajo actual: `mateus`.

## 2. Resumen del juego

El juego es un dungeon crawler 3D con camara ortografica isometrica. El jugador avanza por 10 salas, recoge monedas, elimina enemigos y atraviesa una puerta que solo se abre cuando todos los enemigos de la sala han sido derrotados. A partir de la tercera sala, el suelo empieza a caer por filas para obligar al jugador a avanzar. La ultima sala incluye un boss con multiples golpes de vida.

Controles:

- `Enter`: empezar desde menu.
- `WASD` o flechas: movimiento.
- `Espacio` o click: ataque.
- `Esc`: volver al menu.
- `0`-`9`: saltar a salas para debug/evaluacion.

## 3. Requisitos basicos cubiertos

- 10 salas cargadas desde JSON en `Assets/Resources/Levels`.
- Dificultad creciente por numero de enemigos, tipos de enemigos, trampas y suelo que cae.
- Puerta cerrada al entrar en cada sala y apertura al eliminar enemigos.
- 3 tipos de enemigos no boss: `Slime`, `Bat`, `Wizard`, `Gnome`.
- Slime deja `RastroSlime`, que ralentiza al jugador.
- Wizard y boss tienen proyectiles a media distancia para diferenciar ataques.
- Boss final con 8 vidas, barra de vida y abanico de proyectiles.
- Jugador con 3 puntos de vida.
- Enemigos y trampas hacen dano al jugador.
- Suelo cae por filas desde sala 3 hasta sala 9; no cae en boss.
- 5 trampas: `Spikes`, `Flame`, `Blade`, `SpiderWebs`, `RetractileFork`.
- Monedas coleccionables.
- Al menos 5 decoraciones por sala: antorcha, caja, pilar, estatua, banner y paredes.
- HUD con vida, monedas, salas superadas y sala actual.
- Pantallas/estados de menu, juego, creditos, derrota y victoria.
- Musica runtime de menu/juego y SFX para ataque, hit, moneda, puerta, dano, trampa, suelo, derrota y victoria.

## 4. Game feel implementado

- Ataque con slash visual, sonido, shake y feedback distinto si golpea.
- Enemigos con flash al recibir golpe, particulas y barras de vida cuando tienen varias vidas.
- Muerte de enemigos con particulas y delay antes de destruir.
- Puerta con cambio de material, animacion y sonido al abrir.
- Monedas con rotacion, flotacion, sonido y particulas al recoger.
- Jugador con invulnerabilidad temporal, parpadeo y knockback al recibir dano.
- Trampas con activacion visual/sonora.
- Filas del suelo avisan con parpadeo antes de caer, luego caen animadas con shake, sonido y particulas.
- Rastro de slime con pulso visual y feedback al activarse.
- Proyectiles enemigos con material emisivo, SFX de casteo e impacto con particulas.
- Enemigos runtime con siluetas low-poly diferenciadas y bob visual.

## 5. Arte y assets

Assets existentes del repositorio:

- Prefabs de suelo: `floor`, `floorDark`, `floorBlue`, `floorCarpetEnd`, `floorCarpetFill`.
- Prefab de `Slime`.
- Prefab de `RastroSlime`.
- Materiales de slime y rastro.

Assets/runtime generados por codigo:

- Jugador low-poly.
- Puerta.
- Monedas.
- Paredes y decoraciones.
- Trampas.
- Fallback visual para `Bat`, `Wizard`, `Gnome` y `Boss`.
- Particulas y clips de audio generados en runtime.

## 6. Validacion tecnica

Validador estatico/editor:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-final-pass.log
```

Resultado actual:

```text
[DungeonBatchValidator] OK
```

Smoke test de Play Mode:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-final.log
```

Resultado actual:

```text
[DungeonPlaySmoke] OK
```

El smoke test inicia partida, comprueba que la puerta empieza cerrada, derrota enemigos con `PlayerCombat.Attack()`, abre puerta, transiciona de sala, salta al boss, derrota enemigos finales con el ataque real del jugador y comprueba victoria. Ademas, `DungeonAllLevelsSmoke` recorre las 10 salas secuencialmente, limpia todos los enemigos con el ataque real y valida victoria final.

Smoke test de requisitos:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-smoke-start-level1.log
```

Resultado actual:

```text
[DungeonRequirementsSmoke] OK
```

Este smoke test comprueba:

- recogida de monedas y contador de HUD;
- ralentizacion por rastro de slime y limpieza del slow al respawnear/cambiar de sala;
- daño por contacto de enemigos;
- decoraciones por sala;
- iluminacion/fog runtime y antorchas con luz/particulas;
- daño de trampa;
- game over por daño repetido;
- suelo cayendo en sala avanzada;
- suelo estable en sala boss.

Smoke test de pantallas/menu:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-smoke.log
```

Resultado actual:

```text
[DungeonMenuSmoke] OK
```

Este smoke test comprueba menu inicial, creditos, vuelta al menu, inicio de partida, salida al menu y salto al boss por la ruta de debug.

Capturas automaticas:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-screenshot-capture-camera.log
```

Resultado actual:

```text
[DungeonScreenshotCapture] OK
```

Capturas generadas en `Docs/Captures`:

- `01_level1_start.png`: sala inicial.
- `02_level6_traps.png`: sala con enemigos y trampas.
- `03_level9_falling_floor.png`: suelo cayendo.
- `04_boss_room.png`: boss final.

Build smoke:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke.log
```

Resultado actual:

```text
[DungeonBuildSmoke] OK
```

El build Linux standalone se genera en `Builds/Linux/`.

Ultima pasada automatizada completa realizada tras pulido visual/UI, overlay de dano, barra de progreso, consistencia de saltos debug, feedback KO/Open y regeneracion de capturas:

- `git diff --check`: sin salida.
- `/tmp/vj-pre-merge-validator.log`: `[DungeonBatchValidator] OK`.
- `/tmp/vj-pre-merge-play.log`: `[DungeonPlaySmoke] OK`.
- `/tmp/vj-pre-merge-all-levels.log`: `[DungeonAllLevelsSmoke] OK`.
- `/tmp/vj-pre-merge-requirements.log`: `[DungeonRequirementsSmoke] OK`, incluyendo iluminacion, dash, textos flotantes, proyectiles, overlay de dano, trampas, game over, suelo cayendo y sala boss estable.
- `/tmp/vj-pre-merge-menu.log`: `[DungeonMenuSmoke] OK`, incluyendo salto debug al boss con 9 salas superadas.
- `/tmp/vj-pre-merge-screenshot.log`: `[DungeonScreenshotCapture] OK`.
- `/tmp/vj-pre-merge-build.log`: `[DungeonBuildSmoke] OK size=99780244 bytes`.
- `/tmp/vj-pre-merge-delivery.log`: `[DungeonDeliveryValidator] OK`.

## 7. Pendiente para memoria final

- Capturas manuales pendientes solo para menu, creditos, HUD final/victoria si se quieren incluir. Ya hay capturas automaticas de sala inicial, trampas, suelo cayendo y boss, regeneradas tras el pulido de luz. La captura automatica de UI desde `-batchmode` no se conserva porque Unity no escribe el Game View de forma fiable en esa ruta.
- Usar `Docs/REQUIREMENTS_AUDIT.md` como checklist para no olvidar ningun punto del enunciado.
- Explicar reparto real Carolina/Mateus segun colores del PDF.
- Anadir decisiones de diseno: por que camara ortografica, por que filas que caen, por que feedback visual.
- Anadir problemas encontrados:
  - activacion de Unity batchmode/licencia;
  - repo inicial equivocado;
  - fallback runtime para prefabs no asignados;
  - Play Mode smoke sin `-quit` porque el runner sale con `EditorApplication.Exit`.
- Revisar texto final en catalan/castellano segun pida la entrega.
