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

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
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
