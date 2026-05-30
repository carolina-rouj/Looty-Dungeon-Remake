using UnityEngine;

public class RastroSlime : MonoBehaviour
{
    public float slowDuration = 2f;   // segundos que ralentiza al player (y luego desaparece)
    public float slowFactor   = 0.5f; // multiplicador de velocidad (para futuro PlayerMovement)

    private bool activated = false;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (DungeonGameRuntime.Instance != null && !DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        if (!activated)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                PlayerMovement player = playerObject.GetComponent<PlayerMovement>();
                Vector3 delta = playerObject.transform.position - transform.position;
                delta.y = 0f;
                if (player != null && delta.sqrMagnitude <= 0.55f * 0.55f)
                {
                    Activate(player);
                }
            }
        }

        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.06f;
        transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z * pulse);
    }

    void OnTriggerEnter(Collider other)
    {
        if (DungeonGameRuntime.Instance != null && !DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (!activated && player != null)
        {
            Activate(player);
        }
    }

    private void Activate(PlayerMovement player)
    {
        activated = true;
        player.ApplySlow(slowFactor, slowDuration);
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.ShowMessage("Ralentizado");
            DungeonGameRuntime.Instance.PlayTrapActivated(transform.position, "SlimeTrail");
        }
        Destroy(gameObject, slowDuration);
    }
}
