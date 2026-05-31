using UnityEngine;

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
            if (behaviour == this) continue;
            // StopAllCoroutines para que un salto en curso no pelee con el Rigidbody.
            // enabled=false solo detiene Update, las corutinas siguen si no se paran.
            behaviour.StopAllCoroutines();
            behaviour.enabled = false;
        }
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
        // Reutiliza el Rigidbody si ya tuviera uno (no se puede añadir un segundo: daría null
        // y un NullReferenceException al tocar useGravity).
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = false;
        body.useGravity = true;
        Destroy(gameObject, 2f);
    }
}
