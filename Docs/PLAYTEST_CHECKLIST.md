# Manual Playtest Checklist

Checklist para la revision humana antes de mergear `mateus` a `develop`.

## Setup

- Abrir `/home/maziu/uni/VJ-3D/repo` con Unity `6000.4.4f1`.
- Abrir `Assets/Scenes/LevelScene.unity`.
- Entrar en Play Mode desde el editor.

## Flujo principal

- Menu inicial visible, jugador oculto y musica sonando.
- Boton `Creditos` abre creditos y `Volver` vuelve al menu.
- `Jugar` o `Enter` empieza en sala 1 con vida `3/3`, monedas `0` y sala `1/10`. Mensaje de bienvenida tipo "Mata enemigos para abrir la puerta".
- El indicador de 10 salas del HUD avanza al saltar o cambiar de sala (amarillo = current, verde = cleared, rojo = boss).
- `WASD`/flechas mueven bien en camara ortografica.
- `Shift` hace dash corto, muestra estela y no deja al jugador atrapado fuera del suelo.
- `Space` o click ataca hacia la direccion de movimiento.
- Al matar todos los enemigos, la puerta cambia a estado abierto.
- Entrar en la puerta carga la sala siguiente.
- `P` o `Esc` durante el juego abre la pausa: mundo congelado, panel con `Continuar`/`Menu`, `Enter`/`P` reanuda y `M` vuelve al menu.
- `Esc` en menus/derrota/victoria vuelve al menu y para la accion del mundo.
- En las pantallas de derrota y victoria los atajos `R`/`Enter`/`M` funcionan tal como indica el hint en pantalla.
- El HUD muestra el cronometro `mm:ss` y los paneles finales muestran resumen (monedas, KO, tiempo, mejor partida).

## Requisitos concretos

- Probar `0` a `9`: cada tecla salta a la sala esperada; `9` lleva al boss.
- En una sala con slime, pisar el rastro y comprobar ralentizacion + mensaje.
- En una sala con wizard, mantenerse a media distancia y comprobar proyectil azul.
- En boss, mantenerse a media distancia y comprobar abanico de proyectiles rojos.
- En sala 6 o superior, tocar `Spikes`, `Flame` y `Blade`; todas deben hacer dano.
- En salas 8-9, probar `SpiderWebs` y `RetractileFork`; la web debe ralentizar/danar por estado y el tenedor debe danar al extenderse.
- Al recibir dano aparece feedback rojo en los bordes; a 1 vida debe quedar un pulso rojo sutil.
- En salas 3-9, esperar a que el suelo avise y caiga por filas.
- En boss, comprobar que el suelo no cae y que el boss requiere varios golpes.
- Con 0 vidas, aparece `Has perdido` y el jugador no sigue interactuando.
- Tras victoria, aparece `Victoria` y el contador queda en `Salas: 10/10`.
- Pulsar `P` mid-game: aparece panel de pausa, mundo y enemigos parados; `Enter`/`P` reanuda sin perder progreso.
- Cambiar entre secciones (saltar 0->3->6->9) y verificar tinte distinto del fondo y la luz; al entrar al boss aparece texto "BOSS" con shake/particulas.
- Al ganar/perder, verificar que el resumen muestra `Monedas`, `KO`, `Tiempo` y `Mejor:` (si hay partida previa guardada).
- Recibir dano, dejar morir un par de enemigos: ocasionalmente debe caer un corazon (pickup rojizo flotante) y restaurar 1 punto al cogerlo. No deberia caer si el jugador esta a 3/3.

## Capturas manuales para memoria

- Menu inicial.
- Creditos.
- HUD durante partida.
- Game over.
- Victoria.

Las capturas 3D de salas ya estan en `Docs/Captures/` y se regeneran con `DungeonScreenshotCapture`.

## Smoke opcional del build

Desde una sesion con display disponible:

```bash
Builds/Linux/VJ3D.x86_64 -batchmode -vjSmokeTest -logFile /tmp/vj-player-smoke.log
```

En el entorno headless de Codex no se pudo ejecutar este smoke porque el player Linux no arranca sin display/X virtual.
