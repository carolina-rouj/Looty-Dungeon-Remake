# Requirements Audit

Estado operativo de requisitos del enunciado de VJ 3D. Este archivo sirve como checklist rapido para revisar antes de entregar.

## Parte basica

| Requisito | Estado | Evidencia |
| --- | --- | --- |
| Juego 3D tipo Looty Dungeon | Cubierto | Camara ortografica isometrica, salas por grid, puerta y avance por salas en `DungeonGameRuntime` + `LevelManager`. |
| 10 salas | Cubierto | `Assets/Resources/Levels/level1.json` ... `level9.json` + `final_level.json`; `DungeonAllLevelsSmoke` recorre las 10 salas hasta victoria. |
| Dificultad creciente | Cubierto | JSONs aumentan enemigos, trampas y complejidad; `level9` tiene 5 enemigos y 4 trampas. |
| Puerta cerrada hasta matar enemigos | Cubierto | `DungeonDoor` + `DungeonGameRuntime.NotifyEnemyDefeated`; `DungeonPlaySmoke` y `DungeonAllLevelsSmoke` validan puerta cerrada al inicio y abierta tras derrotar enemigos. |
| 3 tipos de enemigos | Cubierto | `Slime`, `Bat`, `Wizard`, `Gnome`; `DungeonBatchValidator` exige al menos 3 tipos no boss y tipos validos. |
| Enemigo que deja rastro ralentizante | Cubierto | `Slime` instancia `RastroSlime`; `DungeonRequirementsSmoke` valida que el rastro ralentiza al jugador. |
| Boss final | Cubierto | `final_level.json` + `Boss` con 8 vidas y barra de vida runtime. |
| Jugador ataca y elimina enemigos | Cubierto | `DungeonPlaySmoke` posiciona al jugador y derrota enemigos usando `PlayerCombat.Attack()`. |
| Boss requiere varios golpes | Cubierto | `Boss` tiene 8 vidas; `EnemyHitFeedback` muestra barra. |
| Enemigos atacan al jugador | Cubierto | `EnemyTouchDamage` aplicado en `RegisterEnemy`; wizard/boss tienen proyectiles a media distancia; validado por `DungeonRequirementsSmoke`. |
| Jugador con 3 vidas y derrota | Cubierto | `Health = 3`, `GameOver`; validado por `DungeonRequirementsSmoke`. |
| Suelo cae por filas desde sala 3 hasta antes del boss | Cubierto | `fallingFloor` true en salas 3-9 y false en boss; `StartFallingRowsIfNeeded(index >= 2 && index < 9)`; validado por `DungeonBatchValidator` y `DungeonRequirementsSmoke`. |
| Camara ortografica | Cubierto | `CameraFollowDungeon`, `camera.orthographic = true`. |
| 3 trampas | Cubierto | `Spikes`, `Flame`, `Blade`, `SpiderWebs`, `RetractileFork`; `DungeonBatchValidator` exige presencia de todas y `DungeonRequirementsSmoke` valida dano/slow de trampas base y avanzadas. |
| Monedas | Cubierto | `CoinPickup`; validado por `DungeonRequirementsSmoke`. |
| Corazones (extra) | Cubierto | `HealthPickup` drop 18% en enemigos derrotados cuando `Health < 3`. |
| 5 decoraciones | Cubierto | Torch, Crate, Pillar, Statue, Banner, walls; validado por `DungeonRequirementsSmoke`. |
| GUI vida/monedas/salas | Cubierto | `DungeonGameRuntime.DrawHud`, corazones, contador, indicador de 10 salas con boss tinted en rojo, reloj y resumen final con mejor partida. |
| Volver al menu | Cubierto | Boton `Menu` y `Esc` llaman `ShowMenu`; validado por `DungeonMenuSmoke`. |
| 3 pantallas menu/juego/creditos | Cubierto | Estados `Menu`, `Playing`, `Credits`; tambien `Paused`, `GameOver` y `Victory`; validado por `DungeonMenuSmoke` y `DungeonPauseSmoke`. |
| Sonido/musica | Cubierto | `RuntimeDungeonAudio` genera musica y SFX runtime. |
| Teclas 0-9 para saltar salas | Cubierto | `DungeonGameRuntime.WasLevelKeyPressed` + `JumpToLevel`; `DungeonMenuSmoke` valida salto al boss, vida reseteada y 9 salas marcadas como superadas. |

## Game feel

| Requisito | Estado | Evidencia |
| --- | --- | --- |
| Feedback al atacar | Cubierto | Slash visual, SFX, shake y color por hit/miss. |
| Feedback/movilidad jugador | Cubierto | Dash con `Shift`, estela visual, SFX, shake y cooldown; validado por `DungeonRequirementsSmoke`. |
| Feedback al recibir dano | Cubierto | Parpadeo, knockback, particulas, texto flotante, overlay rojo, SFX y shake. |
| Feedback enemigos | Cubierto | Flash, particulas, texto flotante de dano/KO, delay de muerte y barras de vida. |
| Enemigos diferenciados | Cubierto | Slime deja rastro, wizard lanza proyectiles, boss dispara abanico de proyectiles, bat/gnome presionan por contacto. |
| Feedback puerta | Cubierto | Cambio de material, trigger, animacion, SFX, particulas y texto flotante `OPEN`. |
| Feedback monedas | Cubierto | Rotacion, flotacion, texto flotante, SFX y particulas. |
| Feedback trampas | Cubierto | Animacion/activacion, SFX y particulas. |
| Feedback suelo cayendo | Cubierto | Aviso con parpadeo, animacion de caida, SFX, particulas y shake. |
| Arte coherente | Cubierto | Low-poly runtime, iluminacion/fog procedural, antorcha con luz/particulas, decoraciones por sala y capturas 3D regeneradas en `Docs/Captures`. |

## Validaciones actuales

```bash
git diff --check
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-pre-merge-validator.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-pre-merge-play.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-pre-merge-all-levels.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-pre-merge-requirements.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-pre-merge-menu.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-pre-merge-screenshot.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-pre-merge-build.log
```

Smoke opcional del ejecutable Linux con display disponible:

```bash
Builds/Linux/VJ3D.x86_64 -batchmode -vjSmokeTest -logFile /tmp/vj-player-smoke.log
```

En esta sesion headless no se cuenta como validacion porque el player Linux no arranca de forma fiable sin display/X virtual.

Nota: los logs de Unity pueden incluir ruido interno de licencia/Search durante el arranque del editor. Para aceptar una pasada se usa el codigo de salida 0 y el marcador `OK` del runner correspondiente.

## Pendiente no automatizado

- Playtest humano del ritmo de dificultad.
- Revision humana final de HUD/menu en la resolucion de entrega; la UI ya escala por resolucion, pero conviene mirarla en el editor.
- Capturas manuales opcionales de menu, creditos y victoria; las capturas 3D de salas ya estan automatizadas.
- Decision de commit sobre cambios de upgrade generados por Unity 6000.4.4f1.
