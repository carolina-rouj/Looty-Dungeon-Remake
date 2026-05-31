using System.Collections.Generic;
using UnityEngine;

// Reproduce sonidos de las trampas de Carolina (flecha al dispararse, pinchos al subir)
// OBSERVANDO su estado, sin modificar sus scripts (ArrowTrap/RetractileFork/Arrow): asi no
// chocamos con su trabajo en curso sobre las trampas. Vive en el GameObject del runtime.
public class RuntimeTrapAudio : MonoBehaviour
{
    private readonly HashSet<int> knownArrows = new HashSet<int>();
    private readonly Dictionary<int, bool> forkExtended = new Dictionary<int, bool>();

    // El fork parte retraido (forkMesh.localPosition.x ~= 0.3) y se extiende hasta ~1.1.
    private const float ForkExtendThreshold = 0.7f;

    private void Update()
    {
        DungeonGameRuntime game = DungeonGameRuntime.Instance;
        if (game == null || !game.IsPlaying)
        {
            return;
        }

        // --- Flechas: suena cuando aparece una flecha nueva ---
        Arrow[] arrows = FindObjectsByType<Arrow>(FindObjectsSortMode.None);
        if (arrows.Length == 0)
        {
            knownArrows.Clear(); // sin flechas vivas: reseteamos para detectar las siguientes
        }
        else
        {
            foreach (Arrow a in arrows)
            {
                if (a != null && knownArrows.Add(a.GetInstanceID()))
                {
                    game.PlayTrapArrow(a.transform.position);
                }
            }
        }

        // --- Pinchos: suena en el flanco retraido -> extendido ---
        foreach (RetractileFork fork in FindObjectsByType<RetractileFork>(FindObjectsSortMode.None))
        {
            if (fork == null || fork.forkMesh == null) continue;

            int id = fork.GetInstanceID();
            bool extended = fork.forkMesh.localPosition.x > ForkExtendThreshold;
            bool wasExtended = forkExtended.TryGetValue(id, out bool v) && v;
            if (extended && !wasExtended)
            {
                game.PlayTrapSpikes(fork.transform.position);
            }
            forkExtended[id] = extended;
        }
    }
}
