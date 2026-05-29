# Handoff Next Steps

Estado actual de la rama `mateus` tras la ultima iteracion automatizada.

## Estado tecnico

- Juego 3D jugable con 10 salas, boss, trampas, monedas, HUD, menu, pausa, creditos, derrota y victoria.
- Game feel implementado: dash, particulas, SFX/musica runtime, shake, textos flotantes, proyectiles, feedback de dano, overlay rojo, suelo cayendo y barras de vida.
- Estados ampliados: anadido `DungeonState.Paused` con `Time.timeScale=0`, atajos `P`/`Esc`/`Enter`/`M` en los paneles y panel de pausa con `Continuar` y `Menu`.
- HUD ampliado con cronometro, indicador de 10 salas (verde/amarillo/rojo boss) y resumen de partida (monedas, KO, tiempo y mejor partida persistente).
- Ambiente diferenciado por seccion (1-3, 4-6, 7-9, boss) tintando camara, fog, ambient y directional lights desde codigo, con entrada de boss dramatica (shake/particulas/texto "BOSS").
- `mateus` esta sincronizada con `origin/develop` hasta `902efca` e integra la rama `carol-fall-level`.
- Los JSON tienen `fallingFloor`: `false` en salas 1-2 y boss, `true` en salas 3-9; `DungeonBatchValidator` lo comprueba.
- `DungeonRequirementsSmoke` valida efectos de trampas avanzadas: `SpiderWebs` ralentiza y `RetractileFork` hace dano al extenderse.
- `DungeonPauseSmoke` valida el ciclo `Playing` -> `Paused` -> `Playing` -> `Menu` con `Time.timeScale` correcto.
- `Tools/run_validation.sh` tambien escanea logs para errores C# y excepciones runtime comunes del proyecto.
- Capturas 3D automaticas regeneradas en `Docs/Captures/`.
- Build Linux generado correctamente en `Builds/Linux/` mediante `DungeonBuildSmoke`.
- No se ha hecho commit ni push desde Codex.

## Ultima bateria verde

```bash
git diff --check
```

Resultado: sin salida.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-pre-merge-validator.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonDeliveryValidator.Run -logFile /tmp/vj-pre-merge-delivery.log
```

Resultado: `[DungeonDeliveryValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-pre-merge-requirements.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-pre-merge-menu.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPauseSmoke.Run -logFile /tmp/vj-pre-merge-pause.log
```

Resultado: `[DungeonPauseSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-pre-merge-play.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-pre-merge-all-levels.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-pre-merge-screenshot.log
```

Resultado: `[DungeonScreenshotCapture] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-pre-merge-build.log
```

Resultado: `[DungeonBuildSmoke] OK size=99780244 bytes`.

## Revision manual pendiente

- Abrir `Assets/Scenes/LevelScene.unity` en Unity `6000.4.4f1`.
- Entrar en Play Mode y seguir `Docs/PLAYTEST_CHECKLIST.md`.
- Usar `Docs/DEMO_SCRIPT.md` para una demo rapida de requisitos y game feel.
- Revisar `Docs/RUBRIC_SCORECARD.md` antes de cerrar la memoria para comprobar el 4+4+2.
- Ejecutar `Tools/run_validation.sh` como bateria completa antes de mergear.
- Revisar que HUD/menu no se solapan en la resolucion de entrega.
- Tomar capturas manuales de menu, creditos, HUD, derrota y victoria si se quieren incluir en la memoria.
- Ajustar en `MEMORIA_ENTREGA_BORRADOR.md` los nombres completos y reparto real segun colores del PDF.
- Seguir `Docs/MERGE_CHECKLIST.md` antes de commit/push de `mateus` y posterior merge a `develop`.
- Usar `Docs/PR_DESCRIPTION_DRAFT.md` como descripcion base del PR si se abre en GitHub.

## Nota sobre el ejecutable

Existe un flag oculto para probar el player desde una sesion con display:

```bash
Builds/Linux/VJ3D.x86_64 -batchmode -vjSmokeTest -logFile /tmp/vj-player-smoke.log
```

En esta sesion headless no se usa como evidencia principal porque el player Linux no arranca de forma fiable sin display/X virtual.
