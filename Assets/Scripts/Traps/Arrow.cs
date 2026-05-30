using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    void Start()
    {
        Destroy(gameObject, 5f); 
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // TODO: other.GetComponent<Player>().TakeDamage(damage);
        }
        Destroy(gameObject); 
    }
}
