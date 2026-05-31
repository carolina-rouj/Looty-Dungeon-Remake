using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public float moveSpeed = 1.9f;
    public float detectRange = 11f;
    public float keepDistance = 1.1f;
    public float lungeInterval = 2.6f;

    private Transform player;
    private Transform visual;
    private Vector3 visualBaseScale = Vector3.one;
    private float nextLunge;
    private bool lunging;
    private float groundedY;

    private void Start()
    {
        TryFindPlayer();
        visual = transform.Find("RuntimeEnemyVisual");
        if (visual != null) visualBaseScale = visual.localScale;
        nextLunge = Time.time + 1.5f;
        groundedY = transform.position.y;
    }

    // Mantiene al boss dentro de la sala y a ras de suelo. Asi su embestida no lo saca del
    // mapa ni lo hace atravesar el suelo (cerca del trono) y caer.
    private void ClampToRoom()
    {
        LevelManager lm = LevelManager.Instance;
        if (lm != null)
        {
            float margin = 0.6f;
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -lm.MaxBoundX + margin, lm.MaxBoundX - margin);
            p.z = Mathf.Clamp(p.z, lm.MinBoundZ + margin, lm.MaxBoundZ - margin);
            transform.position = p;
        }

        // Apoya la Y sobre el suelo si hay; si no hay (agujero), conserva la ultima Y
        // valida en vez de caer.
        int floorMask = LayerMask.GetMask("Floor");
        if (Physics.Raycast(transform.position + Vector3.up * 4f, Vector3.down, out RaycastHit hit, 10f, floorMask))
        {
            groundedY = hit.point.y;
        }
        Vector3 q = transform.position;
        q.y = groundedY;
        transform.position = q;
    }

    private void TryFindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (!EnemyMovementUtility.IsGameplayActive())
        {
            return;
        }
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            TryFindPlayer();
            if (player == null) return;
        }

        Vector3 to = player.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;
        Vector3 dir = dist > 0.01f ? to / dist : transform.forward;

        if (to.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 6f * Time.deltaTime);
        }

        // Persigue (manteniendo una distancia minima) salvo durante la embestida.
        if (!lunging && dist > keepDistance && dist < detectRange)
        {
            transform.position += dir * moveSpeed * Time.deltaTime;
        }

        if (!lunging && dist < detectRange && Time.time >= nextLunge)
        {
            nextLunge = Time.time + lungeInterval;
            StartCoroutine(Lunge(dir));
        }

        ClampToRoom();
    }

    private IEnumerator Lunge(Vector3 dir)
    {
        lunging = true;

        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.PlayEnemyCast(transform.position, new Color(1f, 0.4f, 0.12f));
        }
        float t = 0f;
        while (t < 0.45f)
        {
            if (visual != null)
            {
                float k = 1f + 0.18f * Mathf.Sin(t / 0.45f * Mathf.PI);
                visual.localScale = new Vector3(visualBaseScale.x * k, visualBaseScale.y * (2f - k), visualBaseScale.z * k);
            }
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null) visual.localScale = visualBaseScale;

        t = 0f;
        while (t < 0.28f)
        {
            transform.position += dir * 7.5f * Time.deltaTime;
            ClampToRoom();
            t += Time.deltaTime;
            yield return null;
        }

        lunging = false;
    }
}
