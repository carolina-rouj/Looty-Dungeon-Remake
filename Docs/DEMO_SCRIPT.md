# Demo Script

Guion rapido para ensenar el juego en 3-5 minutos.

## Preparacion

- Abrir `Assets/Scenes/LevelScene.unity`.
- Entrar en Play Mode.
- Tener abierto `Docs/PLAYTEST_CHECKLIST.md` por si se quiere verificar punto a punto.

## Recorrido recomendado

1. Menu principal
   - Mostrar menu, musica y boton de creditos.
   - Entrar en creditos y volver.

2. Sala 1
   - Empezar con `Enter`.
   - Mover con `WASD`/flechas.
   - Recoger una moneda para ensenar contador, SFX y particulas.
   - Atacar al slime con `Space` o click.
   - Mostrar que la puerta se abre solo cuando no quedan enemigos.

3. Sala 3 o 4
   - Usar `3` o `4` para saltar si se quiere ir rapido.
   - Mostrar que el contador del HUD queda coherente con la sala.
   - Esperar al aviso del suelo cayendo y ensenar filas que desaparecen.
   - Si el jugador cae, mostrar respawn y perdida de vida.

4. Sala 6
   - Usar `6`.
   - Mostrar las trampas principales: `Spikes`, `Flame`, `Blade`; si hay tiempo, saltar a sala 8-9 para `SpiderWebs` y `RetractileFork`.
   - Recibir dano una vez para ensenar overlay rojo y feedback.
   - Mostrar dash con `Shift`.

5. Sala 8 o 9
   - Usar `8` o `9`.
   - Mostrar wizard con proyectiles y enemigos combinados.
   - Enfatizar dificultad creciente: mas enemigos, trampas y suelo cayendo mas presionante.

6. Boss
   - Usar `9`.
   - Mostrar boss con varias vidas, barra de vida y proyectiles en abanico.
   - Derrotarlo y cruzar la puerta para mostrar pantalla de victoria.

7. Derrota
   - Si hay tiempo, repetir una sala con trampas o enemigos hasta perder.
   - Mostrar pantalla de `Has perdido`, resumen de sala/monedas y retorno al menu.

## Puntos que conviene mencionar

- El juego cumple la base: 10 salas, puerta condicionada por enemigos, 3+ enemigos, boss, trampas, monedas, GUI, menu/creditos y teclas 0-9.
- El game feel no depende de assets externos complejos: se generan runtime particulas, sonidos, luces, textos flotantes, shake y feedback de dano.
- Hay validadores automatizados en `Assets/Scripts/Editor/` para gameplay, requisitos, menu, capturas, build y documentacion.
- La memoria limpia esta en `MEMORIA_ENTREGA_BORRADOR.md`.
