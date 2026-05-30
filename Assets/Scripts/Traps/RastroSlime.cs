using UnityEngine;

public class RastroSlime : MonoBehaviour
{
    public float slowDuration = 2f;   // segundos que ralentiza al player (y luego desaparece)
    public float slowFactor   = 0.5f; // multiplicador de velocidad (para futuro PlayerMovement)

    private bool activated = false;

    void OnTriggerEnter(Collider other)
    {
        if (!activated && other.CompareTag("Player"))
        {
            activated = true;
            // TODO: cuando exista el script de movimiento del player →
            // other.GetComponent<PlayerMovement>().ApplySlow(slowFactor, slowDuration);
            Debug.Log("Player pisó rastro de slime — ralentizando " + slowDuration + "s");
            Destroy(gameObject, slowDuration);
        }
    }
}
