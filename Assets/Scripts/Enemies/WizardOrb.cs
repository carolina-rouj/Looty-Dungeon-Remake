using UnityEngine;

public class WizardOrb : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;

    private static readonly Color orbColor = new Color(0.15f, 0.95f, 0.35f);

    private Rigidbody rb;
    private Transform player;

    void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.18f;

        // Visual: esfera verde magica con emissive
        GameObject vis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.Destroy(vis.GetComponent<Collider>());
        vis.transform.SetParent(transform, false);
        vis.transform.localScale = Vector3.one * 0.28f;
        Renderer r = vis.GetComponent<Renderer>();
        if (r != null)
            r.material = RuntimeMaterials.GetEmissive("wizard_orb", orbColor, 1.5f);
    }

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        Destroy(gameObject, 4f);
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
                HitPlayer(player);
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
        Impact();
    }

    private void HitPlayer(Transform target)
    {
        PlayerHealth ph = target.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(damage, transform.position);
        Impact();
    }

    private void Impact()
    {
        RuntimeVfx.Burst(transform.position, orbColor, 8, 0.3f);
        Destroy(gameObject);
    }
}
