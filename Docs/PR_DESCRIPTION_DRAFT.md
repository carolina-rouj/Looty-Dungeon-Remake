# PR Description Draft

## Summary

Implementa la entrega jugable de VJ 3D en la rama `mateus`, sincronizada con `origin/develop` hasta `902efca`.

- Juego 3D tipo dungeon crawler con 10 salas, boss final, puerta condicionada por enemigos, monedas, HUD, menu, creditos, derrota y victoria.
- Game feel completo: dash, particulas, SFX/musica runtime, textos flotantes, shake, barras de vida, feedback de dano, puerta, monedas, trampas y suelo cayendo.
- Integracion de trabajo de `carol-fall-level`: `fallingFloor`, `Arrow`, `ArrowTrap`, `Diana`, `SpiderWebs` y `RetractileFork`, adaptados al runtime actual.
- Trampas disponibles: `Spikes`, `Flame`, `Blade`, `SpiderWebs`, `RetractileFork`.
- Documentacion de entrega: memoria borrador/final, auditoria de requisitos, scorecard 4+4+2, checklist de playtest, guion de demo, handoff y checklist de merge.

## Validation

```bash
Tools/run_validation.sh
```

Resultado esperado: `[validation] OK`.

La bateria ejecuta:

- `git diff --check`
- `DungeonBatchValidator`
- `DungeonDeliveryValidator`
- `DungeonRequirementsSmoke`
- `DungeonMenuSmoke`
- `DungeonPlaySmoke`
- `DungeonAllLevelsSmoke`
- `DungeonScreenshotCapture`
- `DungeonBuildSmoke`

Ultima evidencia documentada:

- `/tmp/vj-pre-merge-validator.log`: `[DungeonBatchValidator] OK`
- `/tmp/vj-pre-merge-delivery.log`: `[DungeonDeliveryValidator] OK`
- `/tmp/vj-pre-merge-requirements.log`: `[DungeonRequirementsSmoke] SpiderWebs slow and RetractileFork damage work` y `[DungeonRequirementsSmoke] OK`
- `/tmp/vj-pre-merge-menu.log`: `[DungeonMenuSmoke] OK`
- `/tmp/vj-pre-merge-play.log`: `[DungeonPlaySmoke] OK`
- `/tmp/vj-pre-merge-all-levels.log`: `[DungeonAllLevelsSmoke] OK`
- `/tmp/vj-pre-merge-screenshot.log`: `[DungeonScreenshotCapture] OK`
- `/tmp/vj-pre-merge-build.log`: `[DungeonBuildSmoke] OK size=99780244 bytes`

El script tambien falla si detecta `error CS*`, `MissingComponentException`, `NullReferenceException` o `MissingReferenceException` en los logs de Unity.

## Manual Checklist Before Merge

- Abrir `Assets/Scenes/LevelScene.unity` en Unity `6000.4.4f1`.
- Seguir `Docs/PLAYTEST_CHECKLIST.md`.
- Revisar HUD/menu en la resolucion de entrega.
- Completar nombres y reparto real segun colores del PDF en `MEMORIA_ENTREGA_BORRADOR.md`.
- Tomar capturas manuales de menu, creditos, HUD, derrota y victoria si se quieren incluir.
- Exportar la memoria final a PDF.

## Known Notes

- Commit hecho, falta push: el trabajo de runtime, validadores y docs ya esta consolidado en `mateus` por delante de `origin/develop`; queda decidir el push.
- El player Linux tiene un smoke oculto `-vjSmokeTest`, pero en entorno headless no se usa como evidencia principal porque puede requerir display/X.
- Unity puede imprimir ruido interno de `UnityEditor.Search.SearchDatabase` durante el arranque; la bateria acepta solo los marcadores OK y filtra errores relevantes del proyecto.
