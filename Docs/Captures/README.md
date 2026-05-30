# Captures

Capturas generadas automaticamente desde Unity batchmode con:

```bash
/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity -batchmode -projectPath /home/maziu/uni/VJ-3D/repo -executeMethod DungeonScreenshotCapture.Run -logFile /tmp/vj-screenshot-capture-light-ui.log
```

Archivos actuales:

- `01_level1_start.png`: sala inicial con jugador, slime, monedas, iluminacion y decoraciones.
- `02_level6_traps.png`: sala avanzada con enemigos, antorcha y las 3 trampas base.
- `03_level9_falling_floor.png`: suelo cayendo por filas.
- `04_boss_room.png`: sala final con boss.

El capturador valida que cada imagen tenga contraste suficiente para evitar capturas negras o vacias.
