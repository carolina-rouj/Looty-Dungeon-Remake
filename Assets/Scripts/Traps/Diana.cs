using UnityEngine;

public class Diana : MonoBehaviour
{
    public ArrowTrap arrowTrap;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && arrowTrap != null)
            arrowTrap.StartShooting();
    }

    void OnTriggerExit(Collider other)
    {
        if (arrowTrap != null)
            arrowTrap.StopShooting();
    }
}
