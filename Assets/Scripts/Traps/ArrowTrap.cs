using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    public GameObject arrowPrefab;
    public float shootInterval = 2.0f;
    public float arrowSpeed = 5.0f;
    public int damage = 1;
    public int distanceToDiana;

    public Transform diana;
    public Vector3 arrowSpawnOffset = new Vector3(0.2f, 0.92f, 0f);
    private bool shooting = false;
    private float timer = 0f;

    void Start()
    {
        if (diana != null) distanceToDiana = (int)Vector3.Distance(transform.position, diana.position);
        shootInterval = Mathf.Max(0.5f, distanceToDiana * 0.5f);
    }

    void Update()
    {
        // TODO: descomentar cuando se integre el Player — dispara solo si jugador está en diana
        // if (!shooting) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Disparo();
            timer = shootInterval;
        }
    }

    public void StartShooting() => shooting = true;
    public void StopShooting() { shooting = false; timer = 0f; }

    void Disparo()
    {
        if (arrowPrefab == null || diana == null) return;

        Vector3 spawnPos = transform.TransformPoint(arrowSpawnOffset);
        Vector3 direction = diana.position - spawnPos;
        direction.y = 0f;
        direction.Normalize();
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, rotation);

        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.damage = damage;
            arrowScript.speed = arrowSpeed;
        }
    }
}
