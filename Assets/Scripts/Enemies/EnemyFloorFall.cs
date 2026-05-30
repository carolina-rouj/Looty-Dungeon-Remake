using UnityEngine;

// Glue de Mateus: hace que un enemigo se caiga cuando el suelo bajo el (capa "Floor")
// desaparece, p.ej. en los niveles donde el suelo cae por filas. Desactiva la IA del
// enemigo para que no pelee con la fisica, le anade un Rigidbody y lo destruye. Es
// aditivo: no toca los scripts de enemigo de Carolina.
public class EnemyFloorFall : MonoBehaviour
{
    private static readonly Collider[] probe = new Collider[4];
    private float noFloorSince = -1f;
    private bool falling;

    public static void Ensure(GameObject enemy)
    {
        if (enemy.GetComponent<EnemyFloorFall>() == null)
        {
            enemy.AddComponent<EnemyFloorFall>();
        }
    }

    private void Update()
    {
        if (falling || DungeonGameRuntime.Instance == null || !DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        Vector3 footXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        int floorMask = LayerMask.GetMask("Floor");
        bool hasFloor = Physics.OverlapSphereNonAlloc(footXZ, 0.4f, probe, floorMask) > 0;
        if (hasFloor)
        {
            noFloorSince = -1f;
            return;
        }

        if (noFloorSince < 0f)
        {
            noFloorSince = Time.time;
        }
        else if (Time.time - noFloorSince > 0.25f)
        {
            StartFalling();
        }
    }

    private void StartFalling()
    {
        falling = true;
        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour != this)
            {
                behaviour.enabled = false; // congela la IA del enemigo
            }
        }
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
        Rigidbody body = gameObject.AddComponent<Rigidbody>();
        body.useGravity = true;
        Destroy(gameObject, 2f);
    }
}
