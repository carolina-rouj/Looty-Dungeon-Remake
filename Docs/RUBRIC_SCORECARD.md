# Rubric Scorecard

Mapa rapido entre la rubrica del enunciado y la evidencia actual del proyecto. Sirve para preparar la memoria, la demo y la revision antes de entregar.

## Base - 4 puntos

| Punto | Estado | Evidencia |
| --- | --- | --- |
| 10 salas jugables | Cubierto | `Assets/Resources/Levels/level1.json` a `final_level.json`; `DungeonAllLevelsSmoke` limpia las 10 salas. |
| Dificultad creciente | Cubierto | Mas enemigos, mas trampas, mas tipos combinados y suelo cayendo desde sala 3. |
| Puerta condicionada | Cubierto | `DungeonDoor` se abre solo con cero enemigos vivos; validado por `DungeonPlaySmoke`. |
| 3+ enemigos | Cubierto | `Slime`, `Bat`, `Wizard`, `Gnome` y `Boss`. |
| Rastro ralentizante | Cubierto | `RastroSlime`; validado por `DungeonRequirementsSmoke`. |
| Boss multi-hit | Cubierto | `Boss` con 8 vidas, barra de vida y proyectiles en abanico. |
| Jugador ataca y enemigos atacan | Cubierto | `PlayerCombat`, contacto enemigo, proyectiles; validado por smokes. |
| 3 vidas y derrota | Cubierto | `Health = 3`, invulnerabilidad breve y `GameOver`; validado por `DungeonRequirementsSmoke`. |
| Suelo cae desde sala 3 | Cubierto | `fallingFloor` true en salas 3-9, false en boss; validado por `DungeonBatchValidator` y `DungeonRequirementsSmoke`. |
| Camara ortografica | Cubierto | `CameraFollowDungeon`, camara isometrica ortografica. |
| 3+ trampas | Cubierto | `Spikes`, `Flame`, `Blade`, `SpiderWebs`, `RetractileFork`; efectos validados. |
| Monedas y GUI | Cubierto | `CoinPickup`, contador, corazones, sala/progreso y resumen final. |
| Menu, juego y creditos | Cubierto | Estados `Menu`, `Playing`, `Credits`, ademas `Paused`, `GameOver` y `Victory`; validado por `DungeonMenuSmoke` y `DungeonPauseSmoke`. |
| Sonido/musica | Cubierto | `RuntimeDungeonAudio` genera musica y SFX runtime. |
| Teclas 0-9 | Cubierto | `JumpToLevel`; validado por `DungeonMenuSmoke`. |

## Game Feel y Arte - 4 puntos

| Punto | Estado | Evidencia |
| --- | --- | --- |
| Feedback de ataque | Cubierto | Slash, SFX, particulas, shake y texto flotante. |
| Feedback de dano | Cubierto | Overlay rojo, parpadeo, knockback, SFX, particulas y texto `-1`. |
| Feedback enemigos | Cubierto | Flash, barras de vida, muerte retardada, KO y particulas. |
| Feedback puerta | Cubierto | Cambio visual, SFX, texto `OPEN` y burst. |
| Feedback monedas | Cubierto | Rotacion, flotacion, SFX, texto y particulas. |
| Feedback trampas | Cubierto | Activacion visual, dano/slow y SFX. |
| Feedback suelo cayendo | Cubierto | Aviso por color, caida animada, SFX, particulas y shake. |
| Movimiento expresivo | Cubierto | Dash con estela y cooldown. |
| Arte coherente | Cubierto | Low-poly runtime, iluminacion/fog, antorchas, paredes, decoraciones, ambiente tintado por seccion (calida/marron/violeta/boss rojo) y capturas en `Docs/Captures/`. |
| Diferenciacion mecanica | Cubierto | Slime ralentiza, wizard/boss disparan, boss dispara abanico, trampas avanzadas ralentizan o castigan timing. |
| Pausa con Time.timeScale | Cubierto | `DungeonState.Paused`, panel propio y atajos `P`/`Esc`/`Enter`/`M`; validado por `DungeonPauseSmoke`. |
| Resumen y persistencia | Cubierto | Stats (monedas, KO, tiempo `mm:ss`) en panel final y mejor partida persistente en `PlayerPrefs`. |

## Memoria - 2 puntos

| Punto | Estado | Evidencia |
| --- | --- | --- |
| Estructura de memoria | Preparado | `MEMORIA_ENTREGA_BORRADOR.md` cubre juego, controles, requisitos, game feel, arquitectura, reparto, capturas, validacion y problemas. |
| Capturas 3D | Preparado | `Docs/Captures/01_level1_start.png`, `02_level6_traps.png`, `03_level9_falling_floor.png`, `04_boss_room.png`. |
| Evidencia tecnica | Preparado | Logs `/tmp/vj-pre-merge-*.log`, `Tools/run_validation.sh`, `Docs/REQUIREMENTS_AUDIT.md`. |
| Reparto real | Pendiente humano | Completar nombres y reparto segun colores del PDF. |
| Capturas UI opcionales | Pendiente humano | Menu, creditos, HUD, derrota y victoria si se quieren insertar. |
| Export final a PDF | Pendiente humano | Exportar desde `MEMORIA_ENTREGA_BORRADOR.md` tras ajustes finales. |

## Validacion recomendada final

```bash
Tools/run_validation.sh
```

La pasada final esperada debe terminar con `[validation] OK` y los logs no deben contener `error CS*`, `MissingComponentException`, `NullReferenceException` ni `MissingReferenceException`.
