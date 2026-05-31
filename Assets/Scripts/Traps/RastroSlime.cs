using System.Collections;
using UnityEngine;

public class RastroSlime : MonoBehaviour
{
    public float lifetime = 8f;

    private bool activated = false;
    private bool falling = false;
    private float noFloorSince = -1f;
    private static readonly Collider[] probe = new Collider[4];

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated || !other.CompareTag("Player")) return;
        activated = true;
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null) StartCoroutine(TrapSequence(pm));
    }

    IEnumerator TrapSequence(PlayerMovement pm)
    {
        Vector3 center = LevelManager.Instance != null
            ? LevelManager.Instance.SnapToGrid(transform.position)
            : transform.position;
        center.y = pm.transform.position.y;

        CharacterController cc = pm.GetComponent<CharacterController>();
        cc.enabled = false;
        pm.transform.position = center;
        cc.enabled = true;

        pm.ApplyJelly(1.5f);

        RuntimeVfx.Burst(center + Vector3.up * 0.3f, new Color(0.2f, 0.85f, 0.2f, 0.7f), 22, 0.55f);
        DungeonGameRuntime.Instance?.PlayTrapActivated(transform.position, "Slime");

        StartCoroutine(BounceScale(transform,    new Vector3(1.35f, 0.5f,  1.35f)));
        StartCoroutine(BounceScale(pm.transform, new Vector3(1.2f,  0.65f, 1.2f)));

        yield return new WaitForSeconds(1.5f);

        RuntimeVfx.Burst(transform.position + Vector3.up * 0.2f, new Color(0.2f, 0.85f, 0.2f, 0.7f), 14, 0.4f);
        DungeonGameRuntime.Instance?.PlayTrapActivated(transform.position, "Slime");
        yield return StartCoroutine(ShrinkOut(0.18f));
        Destroy(gameObject);
    }

    IEnumerator ShrinkOut(float duration)
    {
        Vector3 orig = transform.localScale;
        for (float e = 0f; e < duration; e += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(orig, Vector3.zero, e / duration);
            yield return null;
        }
    }

    IEnumerator BounceScale(Transform t, Vector3 squashedScale)
    {
        Vector3 orig = t.localScale;
        const float squashTime = 0.12f;
        const float returnTime = 0.22f;

        for (float e = 0f; e < squashTime; e += Time.deltaTime)
        {
            t.localScale = Vector3.Lerp(orig, squashedScale, e / squashTime);
            yield return null;
        }
        for (float e = 0f; e < returnTime; e += Time.deltaTime)
        {
            t.localScale = Vector3.Lerp(squashedScale, orig, e / returnTime);
            yield return null;
        }
        t.localScale = orig;
    }

    void Update()
    {
        if (falling) return;

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
            c.enabled = false;

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
        }
        Destroy(gameObject, 2f);
    }
}
