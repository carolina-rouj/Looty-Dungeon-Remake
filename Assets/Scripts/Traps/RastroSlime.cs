using UnityEngine;

public class RastroSlime : MonoBehaviour
{
    public float slowDuration = 2f;   // segundos que ralentiza al player (y luego desaparece)
    public float slowFactor   = 0.5f; // multiplicador de velocidad del player

    private bool activated = false;
    private bool falling = false;
    private float noFloorSince = -1f;
    private static readonly Collider[] probe = new Collider[4];

    void Start()
    {
        // El slime instancia el rastro a una Y fija (1) que flota; lo apoyamos en la
        // superficie real del suelo.
        int floorMask = LayerMask.GetMask("Floor");
        if (Physics.Raycast(transform.position + Vector3.up * 4f, Vector3.down, out RaycastHit hit, 10f, floorMask))
        {
            Vector3 p = transform.position;
            p.y = hit.point.y + 0.02f;
            transform.position = p;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!activated && other.CompareTag("Player"))
        {
            activated = true;
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null) pm.ApplySlow(slowFactor, slowDuration);
            Destroy(gameObject, slowDuration);
        }
    }

    void Update()
    {
        if (falling) return;

        // Si el bloque de suelo bajo el rastro se ha caido (suelo que cae), la marca verde
        // cae con el en vez de quedar suspendida en el aire.
        Vector3 footXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        int floorMask = LayerMask.GetMask("Floor");
        bool hasFloor = Physics.OverlapSphereNonAlloc(footXZ, 0.3f, probe, floorMask) > 0;
        if (hasFloor)
        {
            noFloorSince = -1f;
            return;
        }

        if (noFloorSince < 0f) noFloorSince = Time.time;
        else if (Time.time - noFloorSince > 0.2f) StartFalling();
    }

    void StartFalling()
    {
        falling = true;
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
        }
        Destroy(gameObject, 2f);
    }
}
