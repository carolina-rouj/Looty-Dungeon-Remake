using UnityEngine;

public class Diana : MonoBehaviour
{
    public ArrowTrap arrowTrap;

    private void Awake()
    {
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        // Diana actúa como tile de suelo: layer "Floor" para que HasFloorAt lo detecte
        gameObject.layer = LayerMask.NameToLayer("Floor");

        BoxCollider floor = gameObject.AddComponent<BoxCollider>();
        floor.isTrigger = false;
        floor.size   = new Vector3(1f, 1f, 1f);
        floor.center = Vector3.zero;

        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size   = new Vector3(0.9f, 1.5f, 0.9f);
        trigger.center = new Vector3(0f, 0.75f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            arrowTrap?.StartShooting();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            arrowTrap?.StopShooting();
    }
}
