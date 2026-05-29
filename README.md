# VJ 3D - Looty Dungeon

Proyecto Unity para la asignatura VJ de la FIB UPC.

## Abrir

Proyecto Unity:

```text
/home/maziu/uni/VJ-3D/repo
```

Escena principal:

```text
Assets/Scenes/LevelScene.unity
```

Rama de trabajo:

```text
mateus
```

## Controles

- `Enter`: empezar desde menu.
- `WASD` o flechas: moverse.
- `Shift`: dash corto para esquivar.
- `Espacio` o click: atacar.
- `P` o `Esc` (jugando): pausar/reanudar. Desde pausa: `Enter`/`P` reanuda, `M` vuelve al menu.
- `Esc` (en otras pantallas): volver al menu.
- `R` o `Enter` (al perder): reintentar la sala.
- `M` o `Enter` (al ganar): volver al menu.
- `0`-`9`: saltar a salas para debug/evaluacion.

## Estado

- 10 salas jugables con puerta cerrada hasta limpiar enemigos.
- 4 tipos de enemigo normal, boss final con varias vidas y 5 tipos de trampa.
- Game feel runtime: SFX/musica procedural, particulas, dash, textos flotantes, shake, barras de vida, proyectiles enemigos, suelo cayendo, antorchas con luz y UI escalable.
- Pausa real con `P`/`Esc` (Time.timeScale=0), indicador de 10 salas en HUD, ambiente diferenciado por seccion (calida/marron/violeta/boss-rojo) y resumen de partida con mejor puntuacion persistente.
- `mateus` esta sincronizada con `origin/develop` hasta `902efca` e integra la rama `carol-fall-level`.
- Ultima bateria verde documentada en `WORKLOG.md` con logs `/tmp/vj-pre-merge-*.log`.

## Validacion rapida

Bateria completa pre-merge:

```bash
Tools/run_validation.sh
```

Por defecto escribe logs en `/tmp/vj-pre-merge-*.log` y falla si detecta errores de compilacion C# o excepciones runtime habituales del proyecto (`MissingComponentException`, `NullReferenceException`, `MissingReferenceException`). Se puede cambiar el prefijo con `LOG_PREFIX=/tmp/otro-prefijo Tools/run_validation.sh`.

Validador estatico/editor:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-validator.log` con `[DungeonBatchValidator] OK`.

Smoke de flujo principal:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-play.log` con `[DungeonPlaySmoke] OK`.

Smoke de las 10 salas completas:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-all-levels-smoke.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-all-levels.log` con `[DungeonAllLevelsSmoke] OK`.

Smoke de requisitos:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-smoke.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-requirements.log` con `[DungeonRequirementsSmoke] OK`.

Validador de entrega/documentacion:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonDeliveryValidator.Run -logFile /tmp/vj-pre-merge-delivery.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-delivery.log` con `[DungeonDeliveryValidator] OK`.

Smoke de menu/creditos/debug:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-smoke.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-menu.log` con `[DungeonMenuSmoke] OK`.

Smoke de pausa/reanudacion:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPauseSmoke.Run -logFile /tmp/vj-pause-smoke.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-pause.log` con `[DungeonPauseSmoke] OK`.

Capturas automaticas:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-screenshot-capture.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-screenshot.log` con `[DungeonScreenshotCapture] OK`.

Build Linux:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke.log
```

Ultima pasada verificada: `/tmp/vj-pre-merge-build.log` con `[DungeonBuildSmoke] OK size=99780244 bytes`.

Smoke opcional del ejecutable Linux, desde una sesion con display disponible:

```bash
Builds/Linux/VJ3D.x86_64 -batchmode -vjSmokeTest -logFile /tmp/vj-player-smoke.log
```

El flag oculto `-vjSmokeTest` hace que el propio runtime limpie las 10 salas, valide victoria y salga. En el entorno headless de Codex el player Linux no arranca de forma fiable sin X virtual; por eso la validacion principal usa los runners de Unity Editor y el build smoke.

Nota: los logs de Unity pueden incluir ruido interno de licencia/Search al arrancar el editor. La pasada se considera valida cuando el proceso acaba con codigo 0 y aparece el marcador `OK` del runner.

## Documentacion

- `WORKLOG.md`: historial tecnico de cambios.
- `MEMORIA_BORRADOR.md`: borrador de memoria.
- `MEMORIA_ENTREGA_BORRADOR.md`: version limpia para convertir en memoria final.
- `Docs/REQUIREMENTS_AUDIT.md`: checklist de requisitos del enunciado.
- `Docs/PLAYTEST_CHECKLIST.md`: checklist de revision manual antes de merge.
- `Docs/DEMO_SCRIPT.md`: guion breve para demo/playtest de entrega.
- `Docs/MERGE_CHECKLIST.md`: pasos previos a commit, push y merge.
- `Docs/PR_DESCRIPTION_DRAFT.md`: borrador de descripcion para PR/merge.
- `Docs/HANDOFF_NEXT_STEPS.md`: estado actual y pasos concretos antes de entregar/subir.
- `Docs/RUBRIC_SCORECARD.md`: mapa 4+4+2 entre rubrica, evidencias y pendientes.
- `Docs/Captures/`: capturas generadas automaticamente para revisar visuales y usar en memoria.
- `Tools/run_validation.sh`: ejecuta la bateria automatizada completa.

Nota: las capturas automaticas cubren la camara 3D del juego. Para menu, creditos, HUD y victoria conviene hacer capturas manuales desde el editor o desde un build, porque Unity batchmode no expone el Game View de forma fiable para UI.
