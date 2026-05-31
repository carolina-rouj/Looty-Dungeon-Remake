using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.radius = 0.12f;
            col.height = 0.6f;
            col.direction = 2; // Z-axis = arrow's forward
        }
    }

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    private static readonly Collider[] hitBuffer = new Collider[4];

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);

        // OnTriggerEnter no detecta CharacterController cuando el trigger se mueve hacia él.
        // OverlapSphere sí lo encuentra directamente.
        int count = Physics.OverlapSphereNonAlloc(rb.position, 0.25f, hitBuffer);
        for (int i = 0; i < count; i++)
        {
            if (!hitBuffer[i].CompareTag("Player")) continue;
            PlayerHealth ph = hitBuffer[i].GetComponent<PlayerHealth>();
            if (ph == null) continue;
            ph.TakeDamage(damage, transform.position);
            Destroy(gameObject);
            return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage, transform.position);
        }
        Destroy(gameObject);
    }
}
