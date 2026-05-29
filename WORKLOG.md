# VJ 3D Worklog

## 2026-05-26 - Repo correcto y base jugable

Rama de trabajo: `mateus`, creada desde `origin/develop`.

Repositorio real clonado en:

```text
/home/maziu/uni/VJ-3D/repo
```

El proyecto equivocado anterior quedó en:

```text
/tmp/VJ-3D-wrong-backup-20260526-002640
```

### Hecho

- Clonado `https://github.com/carolina-rouj/VJ-3D.git`.
- Checkout de `develop` y creación de rama local `mateus`.
- Inspeccionado el trabajo existente de Carol:
  - `LevelScene.unity`.
  - `LevelManager` por JSON.
  - prefabs de suelo.
  - prefab/script de `Slime`.
  - `RastroSlime`.
  - scripts básicos `Bat`, `Wizard`, `Gnome`, `Boss`.
- Añadido runtime jugable en `Assets/Scripts/Game/DungeonGameRuntime.cs`:
  - menú;
  - créditos;
  - jugador runtime;
  - movimiento;
  - ataque;
  - vida;
  - HUD;
  - monedas;
  - puerta;
  - cámara ortográfica isométrica;
  - audio runtime;
  - partículas runtime;
  - victoria/derrota.
- Ampliado `LevelManager`:
  - paredes;
  - decoraciones;
  - monedas;
  - puerta por sala;
  - caída de filas desde sala 3 hasta antes del boss;
  - fallback visual para enemigos sin prefab asignado;
  - spawn del jugador y salida;
  - detección de suelo para caídas.
- Rellenados todos los JSON de niveles:
  - `level1.json` a `level9.json`;
  - `final_level.json`.
- Conectado `RastroSlime` con `PlayerMovement.ApplySlow`.
- Conectados enemigos con el runtime para notificar muerte y abrir puerta.
- Añadido tag `Player`.
- `EditorBuildSettings` apunta a `Assets/Scenes/LevelScene.unity`.
- `activeInputHandler` cambiado a `2` para permitir input antiguo+nuevo.

### Validado

```bash
cd /home/maziu/uni/VJ-3D/repo
python -m json.tool Assets/Resources/Levels/level1.json >/tmp/level1.ok
python -m json.tool Assets/Resources/Levels/level2.json >/tmp/level2.ok
python -m json.tool Assets/Resources/Levels/level3.json >/tmp/level3.ok
python -m json.tool Assets/Resources/Levels/level4.json >/tmp/level4.ok
python -m json.tool Assets/Resources/Levels/level5.json >/tmp/level5.ok
python -m json.tool Assets/Resources/Levels/level6.json >/tmp/level6.ok
python -m json.tool Assets/Resources/Levels/level7.json >/tmp/level7.ok
python -m json.tool Assets/Resources/Levels/level8.json >/tmp/level8.ok
python -m json.tool Assets/Resources/Levels/level9.json >/tmp/level9.ok
python -m json.tool Assets/Resources/Levels/final_level.json >/tmp/final.ok
git diff --check
```

Compilación estática C# contra ensamblados de Unity: OK.

Unity batchmode ya funciona despues de activar licencia local.

Primera pasada: hubo errores temporales en `Library/PackageCache/com.unity.shadergraph...` durante el import inicial. Unity ejecuto API Updater y acabo con `Exiting batchmode successfully now!`.

Segunda pasada validada:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -logFile /tmp/vj-real-unity-compile-2.log
rg -n "error CS|Exception|No valid Unity Editor license|Aborting batchmode|Scripts have compiler errors|Compilation failed|Exiting batchmode" /tmp/vj-real-unity-compile-2.log
```

Resultado relevante:

```text
Exiting batchmode successfully now!
```

Sin errores C# en la segunda pasada.

### Pendiente inmediato

- Abrir el proyecto manualmente en Unity y confirmar que compila.
- Playtest de `LevelScene`.
- Revisar errores de consola.
- Ajustar:
  - cámara;
  - escala del jugador;
  - puerta;
  - caída de filas;
  - vida/velocidad de enemigos;
  - boss.
- Crear tests/smoke scene runner si batchmode queda habilitado.
- Cuando algo funcione bien, commitear en `mateus` y luego merge/push a `develop`.

### Cómo probar manualmente

1. Abrir `/home/maziu/uni/VJ-3D/repo` en Unity.
2. Abrir `Assets/Scenes/LevelScene.unity`.
3. Pulsar Play.
4. Controles:
   - `Enter`: empezar;
   - `WASD` o flechas: moverse;
   - `Espacio` o click: atacar;
   - `Esc`: volver al menú;
   - `0`-`9`: saltar a nivel.

### Para que Codex pueda iterar solo

Hay que dejar funcionando Unity batchmode con licencia válida para el usuario local.

Comando objetivo:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -logFile /tmp/vj-real-unity-compile.log
```

Ese comando ya no falla por licencia. Codex puede:

- compilar automáticamente;
- leer logs;
- crear tests de playmode/editor;
- ejecutar iteraciones;
- corregir errores;
- repetir en bucle.

### Nota de estado tras abrir Unity 6000.4.4f1

Unity genero cambios adicionales de import/upgrade:

- `.vscode/`
- `repo.slnx`
- `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/ProjectVersion.txt`

No se han limpiado todavia. Revisar antes de hacer commit final para decidir que se conserva.

## 2026-05-26 - Iteracion game feel + smoke test

### Hecho

- Revalidado el estado actual desde rama `mateus`.
- Ampliado `DungeonGameRuntime.cs`:
  - feedback al golpear enemigos con flash, particulas y sonido;
  - barras de vida runtime para enemigos con mas de 1 golpe, especialmente wizard/boss;
  - SFX nuevos para trampas y caida de suelo;
  - HUD con pips de vida y sala en formato `1/10` en vez de indice `0`;
  - victoria marca `RoomsCleared = 10` para que el HUD no quede en `9/10`;
  - creditos actualizados con Carolina Rouj y Mateus.
- Ampliados enemigos:
  - `Slime`, `Bat`, `Gnome`, `Wizard`, `Boss` llaman a `EnemyHitFeedback`;
  - boss mantiene 8 vidas y ahora comunica visualmente los golpes restantes.
- Mejorado `LevelManager`:
  - las filas que caen avisan antes con parpadeo de color;
  - al caer una fila hay particulas, sonido y shake;
  - los fallbacks de enemigos/trampas pasan de `LogWarning` a `Log` porque son ruta soportada.
  - `levelFileName` por defecto pasa a `level1`.
  - expone `GetActiveFloorCount()` para validadores.
- Ajustada `LevelScene.unity` para que el `LevelManager` serializado arranque en `level1`.
- Mejorado `RastroSlime`:
  - aplica ralentizacion tambien por proximidad, no solo por trigger;
  - tiene pulso visual;
  - reproduce feedback de trampa al activarse.
- Mejorado arte runtime de enemigos sin prefab:
  - bat con alas y ojos;
  - wizard con sombrero, baston y orbe;
  - gnome con nariz, hacha y gorro;
  - boss con corona/cuernos/ojos/hombros;
  - animacion visual ligera de bob para estos enemigos.
- Añadido `EnemyMovementUtility`:
  - `Bat`, `Wizard`, `Gnome` y `Boss` ya no avanzan si su nueva posicion cae fuera del suelo activo;
  - reduce enemigos atravesando huecos o saliendose del tablero.
- Ajustada la camara:
  - pasa a encuadre fijo por sala, mas cercano a Looty Dungeon;
  - corrige capturas donde la sala salia cortada arriba y con demasiado vacio abajo;
  - `CameraFollowDungeon` ahora soporta foco estatico.
- Añadido `Assets/Scripts/Editor/DungeonPlaySmoke.cs`:
  - entra en Play Mode desde batchmode;
  - inicia partida;
  - mata enemigos de sala 1;
  - verifica puerta y transicion a sala 2;
  - salta al boss;
  - mata enemigos finales;
  - verifica puerta final y victoria.
- Reforzado `DungeonBatchValidator.cs`:
  - valida contratos runtime principales;
  - valida `Slime` + `RastroSlime` conectados;
  - valida que `LevelScene` arranca en `level1`;
  - valida SFX requeridos;
  - valida componentes de feedback/arte runtime.
- Añadido `MEMORIA_BORRADOR.md` con:
  - resumen del juego;
  - checklist de requisitos basicos;
  - game feel implementado;
  - assets usados/generados;
  - comandos de validacion;
  - pendientes para memoria final.
- Actualizado `.gitignore` para ignorar `.vscode/` y `*.slnx`, generados localmente por Unity/IDE.
- Añadido `Assets/Scripts/Editor/DungeonScreenshotCapture.cs`:
  - genera capturas automaticas desde Play Mode en batchmode;
  - valida que no salgan negras/vacias;
  - guarda PNGs en `Docs/Captures`.
- Añadido `Assets/Scripts/Editor/DungeonBuildSmoke.cs`:
  - hace build Linux standalone desde `LevelScene`;
  - falla si `BuildPipeline` no termina en `BuildResult.Succeeded`.
- Añadido `Assets/Scripts/Editor/DungeonRequirementsSmoke.cs`:
  - valida monedas;
  - valida decoraciones;
  - valida daño de trampa;
  - valida game over por daño repetido;
  - valida que el suelo cae en niveles normales avanzados;
  - valida que el suelo no cae en boss.
- Añadido `Assets/Scripts/Editor/DungeonMenuSmoke.cs`:
  - valida menu inicial, creditos, vuelta al menu, inicio de partida y salto debug al boss.
- Añadido `Docs/Captures/README.md`.
- Añadido `Docs/REQUIREMENTS_AUDIT.md` con checklist de requisitos, evidencia y comandos de validacion.
- Añadido `README.md` con controles, comandos de validacion y enlaces a documentacion.

### Validado

```bash
cd /home/maziu/uni/VJ-3D/repo
git diff --check
```

OK.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-final-pass.log
rg -n "DungeonBatchValidator|error CS|warning CS|Exception|No valid Unity Editor license|Aborting batchmode|Scripts have compiler errors|Compilation failed|Exiting batchmode|OK" /tmp/vj-validator-final-pass.log
```

Resultado relevante:

```text
[DungeonBatchValidator] OK
```

Ultima repeticion tras `EnemyMovementUtility`:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-movement.log
```

Resultado: `[DungeonBatchValidator] OK`.

Ultima repeticion tras camara/capturas:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-camera.log
```

Resultado: `[DungeonBatchValidator] OK`.

Ultima repeticion tras nivel inicial `level1` y requirements smoke:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-start-level1.log
```

Resultado: `[DungeonBatchValidator] OK`.

Play smoke real. Importante: aqui no usar `-quit`; el propio runner sale con `EditorApplication.Exit`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-final.log
rg -n "DungeonPlaySmoke|FAILED|OK|error CS|warning CS|Timeout|Exiting batchmode|ArgumentOutOfRangeException" /tmp/vj-play-smoke-final.log
```

Resultado relevante:

```text
[DungeonPlaySmoke] OK
```

Nota: Unity lanza un `ArgumentOutOfRangeException` interno de Unity Search al indexar en startup. No viene del codigo del juego ni hace fallar el smoke test; el proceso termina con exit code `0` y `[DungeonPlaySmoke] OK`.

Ultima repeticion tras `EnemyMovementUtility`:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-movement.log
```

Resultado: `[DungeonPlaySmoke] OK`.

Ultima repeticion tras camara/capturas:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-camera.log
```

Resultado: `[DungeonPlaySmoke] OK`.

Ultima repeticion tras nivel inicial `level1`:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-start-level1.log
```

Resultado: `[DungeonPlaySmoke] OK`.

Requirements smoke:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-smoke-start-level1.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

Checks cubiertos por el log:

- `Coin pickup increments HUD counter`
- `Trap damage decrements health`
- `Game over reached from repeated damage`
- `Falling floor reduced tiles 58 -> 51`
- `Boss room floor stayed stable`

Menu smoke:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-smoke.log
```

Resultado: `[DungeonMenuSmoke] OK`.

Play smoke tras corregir contador de salas en victoria:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-victory-rooms.log
```

Resultado: `[DungeonPlaySmoke] OK`.

Capturas automaticas:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-screenshot-capture-camera.log
```

Resultado: `[DungeonScreenshotCapture] OK`.

Archivos generados:

- `Docs/Captures/01_level1_start.png`
- `Docs/Captures/02_level6_traps.png`
- `Docs/Captures/03_level9_falling_floor.png`
- `Docs/Captures/04_boss_room.png`

Build smoke:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke.log
```

Resultado: `[DungeonBuildSmoke] OK size=99759508 bytes`.

Ultima repeticion tras nivel inicial `level1`:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke-start-level1.log
```

Resultado: `[DungeonBuildSmoke] OK size=99760020 bytes`.

El build se genera en `Builds/Linux/`, que esta ignorado por `.gitignore`.

### Pasada final automatizada

Tras retirar un runner experimental de capturas de UI que no funcionaba en `-batchmode` porque Unity no escribe el Game View desde esa ruta, se ha repetido la bateria principal:

```bash
git diff --check
```

Resultado: OK.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-after-ui-runner-removal.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-final-pass.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-smoke-final-pass.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-smoke-final-pass.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-screenshot-capture-final-pass.log
```

Resultado: `[DungeonScreenshotCapture] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke-final-pass.log
```

Resultado: `[DungeonBuildSmoke] OK size=99760020 bytes`.

Nota: los logs de Play Mode pueden incluir un `ArgumentOutOfRangeException` de `UnityEditor.Search` durante el indexado. No viene de codigo del juego y los runners salen con codigo 0 y marcador `OK`.

### Ajuste final de enemigos pausados/muerte

- Los enemigos ahora consultan `EnemyMovementUtility.IsGameplayActive()` y dejan de moverse cuando el juego no esta en `Playing` (menu, creditos, game over o victoria).
- Al morir, `EnemyMovementUtility.DisableEnemyAfterDeath()` desactiva colisiones y `EnemyTouchDamage` para evitar dano residual durante la animacion/destruccion retardada.

Validacion tras este ajuste:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-enemy-freeze.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-enemy-freeze.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-smoke-enemy-freeze.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-smoke-enemy-freeze.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke-enemy-freeze.log
```

Resultado: `[DungeonBuildSmoke] OK size=99760532 bytes`.

Se ha reforzado `DungeonPlaySmoke` para comprobar tambien que los enemigos derrotados no conservan `EnemyTouchDamage` ni colliders activos durante el retardo antes de `Destroy`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-harmless-dead-enemies.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-harmless-dead-enemies.log
```

Resultado: `[DungeonPlaySmoke] OK`.

### Parada de efectos fuera de partida

- `LevelManager.StopFallingRows()` permite parar la caida de filas al volver al menu, entrar en creditos, morir o ganar.
- `RastroSlime` no activa ralentizacion ni feedback si el estado actual no es `Playing`.

Validacion tras este ajuste:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-stop-nonplaying-effects.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-smoke-stop-nonplaying-effects.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-stop-nonplaying-effects.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-smoke-stop-nonplaying-effects.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke-stop-nonplaying-effects.log
```

Resultado: `[DungeonBuildSmoke] OK size=99760532 bytes`.

### Respawn seguro tras suelo caido

- `GetPlayerSpawnPosition()` ahora busca una casilla activa si la fila inicial ya ha caido.
- `DungeonRequirementsSmoke` valida que `RespawnPlayerAfterFall()` aterriza sobre suelo activo despues de una caida de filas.

Validacion tras este ajuste:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-safe-respawn.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-smoke-safe-respawn.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-safe-respawn.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke-safe-respawn.log
```

Resultado: `[DungeonBuildSmoke] OK size=99760532 bytes`.

### Pausa del mundo visible

- `CoinPickup` y `RuntimeEnemyVisualAnimator` ahora respetan `EnemyMovementUtility.IsGameplayActive()`, de forma que las monedas y el bobbing visual de enemigos tambien se congelan fuera de partida.

Validacion tras este ajuste:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-paused-visible-world.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-smoke-paused-visible-world.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke-paused-visible-world.log
```

Resultado: `[DungeonBuildSmoke] OK size=99760532 bytes`.

### Cobertura final de requisitos interactivos

- `DungeonBatchValidator` ahora exige tipos de enemigos/trampas validos, al menos 3 tipos de enemigos no boss, boss en `final_level` y que los niveles avanzados tengan mas amenaza que `level1`.
- `DungeonPlaySmoke` derrota enemigos usando `PlayerCombat.Attack()` en vez de llamar directamente a `Hurt`, y valida que la puerta empieza cerrada antes de cada limpieza de sala.
- `DungeonRequirementsSmoke` valida ralentizacion real por `RastroSlime`, limpieza del slow al resetear jugador y daño real por `EnemyTouchDamage`.
- `PlayerMovement.ResetAt()` limpia efectos temporales para que respawn, cambio de sala y salto debug no arrastren ralentizacion.

Validacion actual:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-final-current.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-combat-door-final.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-smoke-slow-reset.log
```

Resultado: `[DungeonRequirementsSmoke] OK`, incluyendo:

- `Coin pickup increments HUD counter`
- `Slime trail slows player`
- `Enemy touch damage decrements health`
- `Trap damage decrements health`
- `Game over reached from repeated damage`
- `Falling floor reduced tiles 58 -> 51`
- `Respawn after fall lands on active floor`
- `Boss room floor stayed stable`

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-smoke-final-current.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-screenshot-capture-final-current.log
```

Resultado: `[DungeonScreenshotCapture] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-smoke-final-current.log
```

Resultado: `[DungeonBuildSmoke] OK size=99761044 bytes`.

### Recorrido completo de 10 salas

- Añadido `Assets/Scripts/Editor/DungeonAllLevelsSmoke.cs`.
- Recorre `level1` a `final_level` sin saltos debug.
- En cada sala valida que la puerta empieza cerrada, derrota todos los enemigos con `PlayerCombat.Attack()`, valida que los enemigos muertos quedan inofensivos y entra por la puerta.
- Al final valida `DungeonState.Victory` y `RoomsCleared == 10`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-all-levels-smoke.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

### Pendiente siguiente

- Playtest visual manual en el editor:
  - ajustar escala/camara si algo se ve raro;
  - comprobar si el HUD ocupa demasiado en la resolucion de entrega;
  - comprobar que el suelo que cae da tiempo justo pero no injusto;
  - revisar velocidades de enemigos en salas 7-9 y boss.
- Capturas manuales de menu, creditos, HUD y victoria si se quieren incluir en la memoria.
- Seguir `Docs/PLAYTEST_CHECKLIST.md` durante el playtest humano.
- Decidir antes de commit si se conservan todos los cambios de upgrade generados por Unity 6000.4.4f1:
  - `ProjectVersion.txt`;
  - paquetes URP;
  - settings URP/Graphics/Editor;
  - `Assets/Settings/DefaultVolumeProfile.asset`.
- Cuando se apruebe visualmente: commit en `mateus`; despues PR/merge a `develop`.

## 2026-05-28 - Pulido visual, luz y UI escalable

### Hecho

- Reforzado el punto de arte/game feel que quedaba mas dependiente de revision humana:
  - `RuntimeSceneLighting` crea luz direccional principal, rim light, ambiente tricolor y fog suave.
  - La camara ahora limpia contra un fondo solido oscuro coherente con la sala.
  - Las antorchas runtime tienen esfera emisiva, particulas y luz puntual.
  - Estatua y banner tienen detalles extra para que las decoraciones no parezcan solo placeholders.
- Mejorada la GUI IMGUI:
  - escala con la resolucion;
  - recalcula estilos al cambiar de escala;
  - el HUD reserva ancho para el boton de menu y reduce riesgo de solapes.
- Reforzado `DungeonRequirementsSmoke`:
  - valida que existen las luces runtime;
  - valida antorcha con llama/luz;
  - deja log explicito de cobertura visual runtime.
- Reforzado `DungeonBatchValidator` para exigir el contrato `RuntimeSceneLighting`.
- Regeneradas capturas 3D en `Docs/Captures/` y revisadas visualmente.

### Validado

```bash
git diff --check
```

Resultado: OK.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-light-ui.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-light-ui.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-all-levels-light-ui.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`, limpiando las 10 salas.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-light-ui.log
```

Resultado: `[DungeonRequirementsSmoke] OK`, incluyendo `Runtime lighting and torch feedback are present`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-light-ui.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-screenshot-capture-light-ui.log
```

Resultado: `[DungeonScreenshotCapture] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-light-ui.log
```

Resultado: `[DungeonBuildSmoke] OK size=99763604 bytes`.

### Pendiente siguiente

- Playtest humano en editor/build para valorar ritmo y feeling real con teclado.
- Capturas manuales de menu, creditos, HUD y victoria si se quieren poner en la memoria.
- Decidir commit/push desde `mateus` cuando se apruebe visualmente.

## 2026-05-28 - Diferenciacion extra de enemigos

### Hecho

- Añadido `EnemyProjectile` runtime:
  - esfera emisiva;
  - movimiento propio;
  - daño al jugador por trigger o proximidad;
  - explosion visual al impactar o caducar.
- `Wizard` ahora lanza proyectiles azules si el jugador esta a media distancia.
- `Boss` ahora lanza un abanico de 3 proyectiles rojos si el jugador esta a media distancia.
- Ambos evitan lanzar cuando el jugador esta ya en melee, para no castigar injustamente el ataque con espada ni romper el recorrido automatizado.
- Añadido SFX `Cast`.
- `DungeonRequirementsSmoke` valida que se puede spawnear un proyectil enemigo.
- `DungeonBatchValidator` exige `EnemyProjectile` y el SFX `Cast`.

### Validado

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-projectiles.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-projectiles.log
```

Resultado: `[DungeonRequirementsSmoke] OK`, incluyendo `Enemy projectile attack can be spawned`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-all-levels-projectiles.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-projectiles.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-projectiles.log
```

Resultado: `[DungeonBuildSmoke] OK size=99766164 bytes`.

## 2026-05-28 - Memoria de entrega

### Hecho

- Añadido `MEMORIA_ENTREGA_BORRADOR.md`:
  - estructura limpia de memoria;
  - tabla requisito -> implementacion;
  - seccion de game feel;
  - arquitectura tecnica;
  - reparto pendiente de ajustar al PDF;
  - rutas de capturas;
  - comandos y resultados de validacion;
  - problemas encontrados y soluciones;
  - checklist final antes de entregar.
- Actualizado `README.md` para enlazar esta memoria limpia.

## 2026-05-28 - Smoke test opcional del ejecutable

### Hecho

- Añadido flag oculto `-vjSmokeTest` en `DungeonGameRuntime`.
- Cuando el player arranca con ese flag:
  - inicia partida;
  - recorre las 10 salas;
  - derrota enemigos usando `PlayerCombat.Attack()`;
  - valida puerta cerrada/abierta;
  - valida que enemigos derrotados quedan inofensivos;
  - valida victoria y `RoomsCleared == 10`;
  - sale con codigo `0` si todo va bien.
- Documentado el comando en `README.md`, `Docs/REQUIREMENTS_AUDIT.md` y `MEMORIA_ENTREGA_BORRADOR.md`.

### Validado

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-standalone-smoke.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-all-levels-standalone-smoke.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-standalone-smoke.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-standalone-smoke.log
```

Resultado: `[DungeonBuildSmoke] OK size=99769748 bytes`.

### Nota de entorno

Intento de ejecutar el player Linux en esta sesion:

```bash
Builds/Linux/VJ3D.x86_64 -batchmode -nographics -vjSmokeTest -logFile /tmp/vj-player-standalone-smoke.log
```

Resultado: el player crashea antes de entrar al juego con `Caught fatal signal - signo:11` durante inicializacion de backend nulo/audio. Sin `-nographics`, el entorno no tiene display (`Video subsystem has not been initialized`). No hay `xvfb-run` instalado. Por eso este smoke queda como herramienta opcional para ejecutar desde una sesion con display disponible, no como evidencia automatica en Codex.

## 2026-05-29 - Textos flotantes de feedback

### Hecho

- Añadido `FloatingTextFx` runtime.
- `RuntimeVfx.FloatingText(...)` crea texto 3D orientado a camara, con subida, escala y fade.
- Conectado feedback flotante a:
  - dano recibido por el jugador (`-1`);
  - golpe a enemigos (`-1`);
  - recogida de monedas (`+1`).
- `DungeonBatchValidator` exige `FloatingTextFx`.
- `DungeonRequirementsSmoke` valida que el feedback flotante puede spawnear.

### Validado

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-floating-text.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-floating-text.log
```

Resultado: `[DungeonRequirementsSmoke] OK`, incluyendo `Floating text feedback can be spawned`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-all-levels-floating-text.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-floating-text.log
```

Resultado: `[DungeonBuildSmoke] OK size=99771284 bytes`.

## 2026-05-29 - Dash del jugador

### Hecho

- Anadido dash corto con `Shift`:
  - cooldown;
  - estela visual;
  - SFX propio `Dash`;
  - camera shake leve;
  - limpieza de estado al hacer reset/respawn/cambio de sala.
- `DungeonRequirementsSmoke` valida que el dash entra en estado activo/cooldown y que `ResetAt()` lo limpia.
- Actualizada documentacion de controles y checklist de playtest.

### Validado

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-dash.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-requirements-dash.log
```

Resultado: `[DungeonRequirementsSmoke] OK`, incluyendo `Player dash activates and resets cleanly`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-all-levels-dash.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-dash.log
```

Resultado: `[DungeonBuildSmoke] OK size=99772308 bytes`.

## 2026-05-29 - Feedback KO/Open

### Hecho

- Al derrotar enemigos aparece texto flotante `KO`.
- Al abrir la puerta aparece texto flotante `OPEN`.
- No cambia reglas ni balance; solo mejora legibilidad de eventos importantes.

### Validado

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-play-smoke-ko-open.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-all-levels-ko-open.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-ko-open.log
```

Resultado: `[DungeonBuildSmoke] OK size=99772308 bytes`.

Nota: en los logs finales vuelve a aparecer ruido interno de Unity de licencia/Search durante el arranque del editor. No viene del codigo del juego; las pasadas aceptadas terminaron con codigo 0 y marcador `OK`.

## 2026-05-29 - Cierre UI de controles y resumen final

### Hecho

- Menu principal actualizado para mencionar `Shift dash`.
- Pantallas finales ahora muestran resumen:
  - derrota: sala alcanzada y monedas;
  - victoria: 10 salas superadas y monedas.
- Actualizados los logs vigentes de validacion en memoria/audit.

### Validado

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-validator-ui-summary.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-menu-ui-summary.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-build-ui-summary.log
```

Resultado: `[DungeonBuildSmoke] OK size=99772308 bytes`.

## 2026-05-29 - Bateria final actualizada

### Hecho

- Reejecutada la bateria automatizada completa con logs `vj-final-*`.
- Regeneradas capturas 3D en `Docs/Captures/`.
- Revisadas visualmente capturas de sala inicial, sala con suelo cayendo y boss: no estan en blanco y el encuadre muestra el gameplay principal.
- Actualizados `README.md`, `Docs/REQUIREMENTS_AUDIT.md`, `MEMORIA_BORRADOR.md` y `MEMORIA_ENTREGA_BORRADOR.md` para apuntar a los logs actuales.

### Validado

```bash
git diff --check
```

Resultado: sin salida.

## 2026-05-29 - Validacion de entrega y guion demo

### Hecho

- Anadido `DungeonDeliveryValidator` para comprobar:
  - documentacion principal;
  - memoria borrador/entrega;
  - capturas generadas;
  - referencias a la ultima bateria verde;
  - handoff y guion de demo.
- `DungeonBatchValidator` ahora comprueba que existe `DungeonDeliveryValidator`.
- Anadido `Docs/DEMO_SCRIPT.md` con recorrido recomendado para ensenar menu, sala inicial, suelo cayendo, trampas, proyectiles, boss, victoria y derrota.
- Referenciado el guion en `README.md` y `Docs/HANDOFF_NEXT_STEPS.md`.

### Validado

```bash
git diff --check
```

Resultado: sin salida.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonDeliveryValidator.Run -logFile /tmp/vj-delivery-validator.log
```

Resultado: `[DungeonDeliveryValidator] OK`.

## 2026-05-29 - Script de validacion completa

### Hecho

- Anadido `Tools/run_validation.sh` para ejecutar en orden:
  - `git diff --check`;
  - `DungeonBatchValidator`;
  - `DungeonDeliveryValidator`;
  - `DungeonRequirementsSmoke`;
  - `DungeonMenuSmoke`;
  - `DungeonPlaySmoke`;
  - `DungeonAllLevelsSmoke`;
  - `DungeonScreenshotCapture`;
  - `DungeonBuildSmoke`.
- El script admite `UNITY_BIN`, `PROJECT_PATH` y `LOG_PREFIX` por variables de entorno.
- `DungeonDeliveryValidator` ahora comprueba que existe el script y que incluye validacion batch, build y delivery.
- Referenciado desde `README.md`, `Docs/MERGE_CHECKLIST.md` y `Docs/HANDOFF_NEXT_STEPS.md`.

### Validado

```bash
git diff --check
```

Resultado: sin salida.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-delivery-batch.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-final-validator.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-final-requirements.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-final-menu.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-final-play.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-final-all-levels.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-final-screenshot.log
```

Resultado: `[DungeonScreenshotCapture] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-final-build.log
```

Resultado: `[DungeonBuildSmoke] OK size=99772308 bytes`.

Nota: en los logs finales vuelve a aparecer ruido interno de Unity de licencia/Search durante el arranque del editor. No viene del codigo del juego; las pasadas aceptadas terminaron con codigo 0 y marcador `OK`.

## 2026-05-29 - Pulido HUD y feedback de dano

### Hecho

- HUD con barra de progreso de salas (`RoomProgress01`) ademas de vida, monedas y contadores.
- Overlay rojo al recibir dano y pulso rojo sutil al quedar a 1 vida.
- `DungeonRequirementsSmoke` valida que el dano activa el estado visual de overlay y que el progreso del HUD es positivo jugando.
- Actualizada documentacion de auditoria, memoria, README y checklist manual.

### Validado

```bash
git diff --check
```

Resultado: sin salida.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-ui-polish-validator.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-ui-polish-requirements.log
```

Resultado: `[DungeonRequirementsSmoke] OK`, incluyendo `Enemy touch damage decrements health and activates damage UI`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-ui-polish-menu.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-ui-polish-play.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-ui-polish-all-levels.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-ui-polish-screenshot.log
```

Resultado: `[DungeonScreenshotCapture] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-ui-polish-build.log
```

Resultado: `[DungeonBuildSmoke] OK size=99773332 bytes`.

## 2026-05-29 - Consistencia de saltos debug

### Hecho

- `JumpToLevel()` ahora sincroniza `RoomsCleared` con la sala destino.
- Saltar al boss con `9` deja el HUD en `Salas: 9/10` y al ganar pasa a `10/10`.
- `DungeonMenuSmoke` valida que el salto debug al boss resetea vida y marca 9 salas superadas.

### Validado

```bash
git diff --check
```

Resultado: sin salida.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-debug-jump-validator.log
```

Resultado: `[DungeonBatchValidator] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-debug-jump-requirements.log
```

Resultado: `[DungeonRequirementsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-debug-jump-menu.log
```

Resultado: `[DungeonMenuSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonPlaySmoke.Run -logFile /tmp/vj-debug-jump-play.log
```

Resultado: `[DungeonPlaySmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-debug-jump-all-levels.log
```

Resultado: `[DungeonAllLevelsSmoke] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-debug-jump-screenshot.log
```

Resultado: `[DungeonScreenshotCapture] OK`.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-debug-jump-build.log
```

Resultado: `[DungeonBuildSmoke] OK size=99773332 bytes`.

## 2026-05-29 - Handoff de entrega

### Hecho

- Anadido `Docs/HANDOFF_NEXT_STEPS.md` con:
  - estado tecnico actual;
  - ultima bateria verde;
  - checklist de revision manual pendiente;
  - nota del smoke opcional del ejecutable con display.
- Referenciado el handoff desde `README.md`.

### Validado

```bash
git diff --check
```

Resultado: sin salida.

## 2026-05-29 - Checklist de merge

### Hecho

- Anadido `Docs/MERGE_CHECKLIST.md` con:
  - comandos de revision local;
  - bateria pre-merge;
  - playtest manual minimo;
  - tareas de memoria;
  - commit/push recomendado para `mateus`.
- `DungeonDeliveryValidator` ahora valida que existe y contiene pasos criticos.
- Referenciado desde `README.md` y `Docs/HANDOFF_NEXT_STEPS.md`.

### Validado

```bash
git diff --check
```

Resultado: sin salida.

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonDeliveryValidator.Run -logFile /tmp/vj-delivery-validator.log
```

Resultado: `[DungeonDeliveryValidator] OK`.

## 2026-05-29 - Bateria pre-merge via script

### Hecho

- Ejecutado `Tools/run_validation.sh` completo fuera del sandbox.
- Los documentos de entrega apuntan ahora a los logs `/tmp/vj-pre-merge-*.log`.
- La primera ejecucion dentro del sandbox fallo porque Unity no podia conectar con display/licencia; fuera del sandbox el script completo paso correctamente. En uso local normal no hace falta ningun ajuste adicional.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK`.

Marcadores comprobados:

- `/tmp/vj-pre-merge-validator.log`: `[DungeonBatchValidator] OK`.
- `/tmp/vj-pre-merge-delivery.log`: `[DungeonDeliveryValidator] OK`.
- `/tmp/vj-pre-merge-requirements.log`: `[DungeonRequirementsSmoke] OK`.
- `/tmp/vj-pre-merge-menu.log`: `[DungeonMenuSmoke] OK`.
- `/tmp/vj-pre-merge-play.log`: `[DungeonPlaySmoke] OK`.
- `/tmp/vj-pre-merge-all-levels.log`: `[DungeonAllLevelsSmoke] OK`.
- `/tmp/vj-pre-merge-screenshot.log`: `[DungeonScreenshotCapture] OK`.
- `/tmp/vj-pre-merge-build.log`: `[DungeonBuildSmoke] OK size=99773332 bytes`.

## 2026-05-29 - Sync con develop y rama carol-fall-level

### Hecho

- Ejecutado `git fetch origin`; `origin/develop` habia avanzado de `51e756b` a `902efca` por merge de `carol-fall-level`.
- Sincronizada `mateus` con `origin/develop` mediante stash temporal, fast-forward y reaplicacion del trabajo local.
- Resueltos conflictos en `LevelData`, `LevelManager` y niveles `4`, `6`, `8` y `final_level`.
- Integrados los flags `fallingFloor` en todos los JSON:
  - `false` en salas 1-2 y boss;
  - `true` en salas 3-9.
- Adaptados los scripts nuevos de trampas de Carolina (`Arrow`, `ArrowTrap`, `Diana`, `RetractileFork`, `SpiderWebs`) al runtime actual sin depender de una clase `Player` inexistente.
- Anadidos fallbacks runtime para `SpiderWebs` y `RetractileFork`, y colocadas estas trampas en salas avanzadas para subir variedad.
- `DungeonBatchValidator` ahora valida los scripts nuevos de trampas y la regla de suelo cayendo del enunciado.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK`.

Marcadores comprobados tras el sync:

- `/tmp/vj-pre-merge-validator.log`: `[DungeonBatchValidator] OK`.
- `/tmp/vj-pre-merge-delivery.log`: `[DungeonDeliveryValidator] OK`.
- `/tmp/vj-pre-merge-requirements.log`: `[DungeonRequirementsSmoke] OK`.
- `/tmp/vj-pre-merge-menu.log`: `[DungeonMenuSmoke] OK`.
- `/tmp/vj-pre-merge-play.log`: `[DungeonPlaySmoke] OK`.
- `/tmp/vj-pre-merge-all-levels.log`: `[DungeonAllLevelsSmoke] OK`.
- `/tmp/vj-pre-merge-screenshot.log`: `[DungeonScreenshotCapture] OK`.
- `/tmp/vj-pre-merge-build.log`: `[DungeonBuildSmoke] OK size=99780244 bytes`.

## 2026-05-29 - Validacion avanzada de trampas

### Hecho

- `DungeonBatchValidator` exige ahora que los niveles incluyan tambien `SpiderWebs` y `RetractileFork`, ademas de `Spikes`, `Flame` y `Blade`.
- `DungeonRequirementsSmoke` crea fallbacks runtime de `SpiderWebs` y `RetractileFork` y valida efectos reales sobre el jugador:
  - `SpiderWebs` en estado parcial ralentiza y el reset limpia el slow;
  - `RetractileFork` en extension hace dano.
- Corregido `SpiderWebs.UpdateAnimator` para no lanzar excepciones cuando el fallback runtime no tiene `Animator`.
- `Diana` limpia correctamente su contador estatico al desactivarse y `RetractileFork` pausa su animacion fuera del estado de juego.
- `Tools/run_validation.sh` escanea cada log y falla si encuentra `error CS*`, `MissingComponentException`, `NullReferenceException` o `MissingReferenceException`.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK`.

Marcadores relevantes:

- `/tmp/vj-pre-merge-validator.log`: `[DungeonBatchValidator] OK`.
- `/tmp/vj-pre-merge-requirements.log`: `[DungeonRequirementsSmoke] SpiderWebs slow and RetractileFork damage work` y `[DungeonRequirementsSmoke] OK`.
- `/tmp/vj-pre-merge-build.log`: `[DungeonBuildSmoke] OK size=99780244 bytes`.
- Filtro de logs de `Tools/run_validation.sh`: sin `error CS*`, `MissingComponentException`, `NullReferenceException` ni `MissingReferenceException`.

Nota: en `/tmp/vj-pre-merge-requirements.log` puede aparecer una excepcion interna de `UnityEditor.Search.SearchDatabase` durante la indexacion de arranque del editor. Es ruido de Unity Search, no referencia scripts del proyecto y el runner termina con codigo 0 y marcador `OK`.

## 2026-05-29 - Scorecard de rubrica

### Hecho

- Anadido `Docs/RUBRIC_SCORECARD.md` con mapa de la entrega 4+4+2:
  - base;
  - game feel y arte;
  - memoria;
  - pendientes humanos restantes.
- Referenciado desde `README.md`, `Docs/HANDOFF_NEXT_STEPS.md` y `MEMORIA_ENTREGA_BORRADOR.md`.
- `DungeonDeliveryValidator` exige ahora que el scorecard exista y cubra `Base - 4 puntos`, `Game Feel y Arte - 4 puntos` y `Memoria - 2 puntos`.
- `DungeonDeliveryValidator` tambien exige que `Tools/run_validation.sh` conserve el filtro de errores de logs.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK`.

Filtro de logs: sin `error CS*`, `MissingComponentException`, `NullReferenceException` ni `MissingReferenceException`.

## 2026-05-29 - Borrador de PR

### Hecho

- Anadido `Docs/PR_DESCRIPTION_DRAFT.md` con:
  - resumen de cambios;
  - bateria de validacion;
  - checklist manual antes de merge;
  - notas conocidas.
- Referenciado desde `README.md`, `Docs/MERGE_CHECKLIST.md` y `Docs/HANDOFF_NEXT_STEPS.md`.

## 2026-05-29 - Higiene Unity meta

### Hecho

- `DungeonDeliveryValidator` valida ahora que todas las carpetas y scripts bajo `Assets/Scripts` tienen su `.meta`.
- Comprobacion orientada a evitar commits incompletos de scripts nuevos de Unity.

## 2026-05-29 - Pausa, indicador de salas y mensaje de inicio

### Hecho

- Anadido estado `DungeonState.Paused` a `DungeonGameRuntime`:
  - `P` o `Esc` pausa durante el juego;
  - `Time.timeScale = 0f` congela mundo, animaciones, VFX y coroutines (incluida la caida del suelo);
  - `Enter`/`P` reanuda, `M` vuelve al menu desde el panel de pausa.
- Sustituida la barra unica de progreso por un indicador de 10 puntos con verde (cleared), amarillo (current con pulso) y rojo (boss). Reescala automaticamente al ancho de pantalla.
- Mensajes contextuales por sala: sala 1 explica "Mata enemigos para abrir la puerta", sala 3 anuncia caida de suelo y sala boss muestra etiqueta especifica con `ShowMessage(text, duration)`.
- Panel de derrota/victoria ahora muestra atajos de teclado y panel reescalado mas grande.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK`.

## 2026-05-29 - Ambiente por seccion, drama de boss y stats de partida

### Hecho

- `DungeonGameRuntime.ApplySectionAmbience(int index)`:
  - 1-3: tonos calidos (rojos/dorados);
  - 4-6: marrones/violetas;
  - 7-9: violetas frios con rim verde;
  - boss: rojo dramatico con niebla mas densa y key light fuerte.
  - Actualiza `Camera.main.backgroundColor`, `RenderSettings.fog/ambient*` y las dos directional lights nombradas mediante `RuntimeSceneLighting.TintMainLights`.
- `PlayBossEntryDrama`: shake fuerte, burst rojo y texto flotante "BOSS" al entrar.
- Stats de partida: `EnemiesDefeated` y `RunSeconds` (basado en `Time.unscaledDeltaTime` para ignorar pausa); HUD muestra reloj `mm:ss`.
- Persistencia en `PlayerPrefs`: `BestCoins` y `BestVictorySeconds` se actualizan al ganar/perder; el panel final muestra "Mejor: ..." cuando hay datos.
- Anadido smoke test `DungeonPauseSmoke` (validator y `run_validation.sh`) que confirma:
  - estado inicial `Menu`;
  - `StartNewGame` -> `Playing` con `Time.timeScale = 1`;
  - `Pause()` -> `Paused`, `Time.timeScale = 0`, `IsPlaying == false`;
  - `Resume()` -> `Playing` y `timeScale = 1`;
  - `ShowMenu()` desde pausa restaura `timeScale = 1`.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK` incluyendo `[DungeonPauseSmoke] OK`.

Marcadores comprobados:

- `/tmp/vj-pre-merge-validator.log`: `[DungeonBatchValidator] OK`.
- `/tmp/vj-pre-merge-delivery.log`: `[DungeonDeliveryValidator] OK`.
- `/tmp/vj-pre-merge-requirements.log`: `[DungeonRequirementsSmoke] OK`.
- `/tmp/vj-pre-merge-menu.log`: `[DungeonMenuSmoke] OK`.
- `/tmp/vj-pre-merge-pause.log`: `[DungeonPauseSmoke] OK`.
- `/tmp/vj-pre-merge-play.log`: `[DungeonPlaySmoke] OK`.
- `/tmp/vj-pre-merge-all-levels.log`: `[DungeonAllLevelsSmoke] OK`.
- `/tmp/vj-pre-merge-screenshot.log`: `[DungeonScreenshotCapture] OK`.
- `/tmp/vj-pre-merge-build.log`: `[DungeonBuildSmoke] OK`.

## 2026-05-29 - Musica de boss, banner de seccion y menu enriquecido

### Hecho

- `RuntimeDungeonAudio.PlayBossMusic()` y `BossLoop` procedural con drone, quinta y modulacion 3.4 Hz; se selecciona automaticamente al cargar `final_level`.
- Banner de seccion en HUD: al entrar por primera vez a cada bloque (1-3, 4-6, 7-9 y boss) aparece un cartel con el nombre de la seccion (`Atrios de la mazmorra`, `Galerias profundas`, `Catacumbas oscuras`, `Salon del boss`) y fundido suave.
- Menu y creditos enriquecidos:
  - en menu se muestra mejor partida persistente (monedas y tiempo de victoria) si hay datos;
  - creditos incluyen detalles de musica/SFX procedurales e iluminacion por seccion.
- Bug fix: `LoadLevelIndex` reactiva el cronometro (`runTimerActive = true`) tras game over o salto, evitando que el reloj quedara congelado en partidas posteriores.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK`.

## 2026-05-29 - Pickup de corazon

### Hecho

- Anadido `HealthPickup`:
  - drop ~18% al matar un enemigo, solo si `Health < 3` y el juego esta en `Playing`;
  - prefab procedural en runtime (dos esferas + punta cubica rotada) con emisivo rosa;
  - flotacion y rotacion suaves;
  - trigger que llama a `DungeonGameRuntime.AddHealth` y se autodestruye.
- `DungeonGameRuntime.AddHealth`: clamp a 3, refresca visuales del jugador, dispara SFX/VFX/floating text.
- Cubierto en `Docs/REQUIREMENTS_AUDIT.md`, `MEMORIA_ENTREGA_BORRADOR.md` y `Docs/PLAYTEST_CHECKLIST.md`.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK`.

## 2026-05-29 - Fade entre salas y contador de enemigos en HUD

### Hecho

- Fade a negro durante la transicion entre salas:
  - `DungeonGameRuntime.DoorEntered` marca `transitionStartedAt`;
  - `NotifyLevelBuilt` marca `transitionFinishedAt`;
  - `DrawTransitionFade` renderiza overlay alfa con fade-out 0.18s y fade-in 0.34s;
  - se resetea al volver al menu.
- HUD muestra `Quedan: N` mientras hay enemigos vivos en la sala; cuando se vacia el HUD pasa a `Puerta abierta`.
- `DungeonGameRuntime.EnemiesAlive` queda expuesto como propiedad publica para posibles tests.

### Validado

```bash
Tools/run_validation.sh
```

Resultado: `[validation] OK`.
