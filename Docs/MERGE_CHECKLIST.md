# Merge Checklist

Checklist antes de hacer commit, push o merge de `mateus` a `develop`.

## 1. Revisar estado local

```bash
git status --short --branch
git diff --check
git diff --stat
```

Comprobar que no se estan incluyendo archivos temporales no deseados. `Builds/`, `Library/` y `Logs/` deben seguir ignorados.

## 2. Ejecutar bateria automatizada

Opcion recomendada:

```bash
Tools/run_validation.sh
```

El script falla si algun runner devuelve error o si los logs contienen errores de compilacion C# o excepciones runtime habituales del proyecto.

Tambien se puede ejecutar comando a comando:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBatchValidator.Run -logFile /tmp/vj-pre-merge-validator.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonRequirementsSmoke.Run -logFile /tmp/vj-pre-merge-requirements.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonMenuSmoke.Run -logFile /tmp/vj-pre-merge-menu.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonAllLevelsSmoke.Run -logFile /tmp/vj-pre-merge-all-levels.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonBuildSmoke.Run -logFile /tmp/vj-pre-merge-build.log
```

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -quit -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonDeliveryValidator.Run -logFile /tmp/vj-pre-merge-delivery.log
```

Todos deben terminar con `OK`.

## 3. Playtest manual minimo

- Seguir `Docs/PLAYTEST_CHECKLIST.md`.
- Usar `Docs/DEMO_SCRIPT.md` para verificar una ruta de demo corta.
- Probar resolucion de entrega y confirmar que HUD/menu no se solapan.
- Probar victoria y derrota desde el editor.

## 4. Memoria

- Completar nombres completos.
- Ajustar reparto real Carolina/Mateus segun colores del PDF.
- Insertar capturas manuales de UI si se usan.
- Exportar la memoria a PDF desde `MEMORIA_ENTREGA_BORRADOR.md`.

## 5. Commit recomendado

Cuando el playtest humano este conforme:

```bash
git add .
git commit -m "Implement 3D dungeon gameplay and delivery docs"
git push origin mateus
```

Despues, abrir PR o mergear a `develop` segun el flujo que useis con Carolina. Usar `Docs/PR_DESCRIPTION_DRAFT.md` como base para la descripcion del PR.
