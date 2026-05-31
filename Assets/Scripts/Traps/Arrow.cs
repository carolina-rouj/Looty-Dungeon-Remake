using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    private Rigidbody rb;
    private Transform player;

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
        var pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) player = pm.transform;
        Destroy(gameObject, 5f);
    }

    private static readonly Collider[] hitBuffer = new Collider[4];

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);

        if (player != null)
        {
            Vector3 delta = rb.position - player.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.5f * 0.5f)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage, transform.position);
                Destroy(gameObject);
                return;
            }
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
