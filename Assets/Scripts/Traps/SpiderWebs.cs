using UnityEngine;

public class SpiderWebs : MonoBehaviour
{
    public enum WebState { Idle = 0, Partial = 1, Full = 2 }

    public float idleDuration = 2f;
    public float partialDuration = 2f;
    public float fullDuration = 2f;
    public float timeParalised = 1.5f;

    private WebState currentState = WebState.Idle;
    private float stateTimer = 0f;
    private Animator animator;
    private Renderer[] renderers;
    private bool playerEffectApplied = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
        stateTimer = idleDuration;
        UpdateAnimator();
    }

    void Update()
    {
        if (DungeonGameRuntime.Instance != null && !DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) AdvanceState();
    }

    void AdvanceState()
    {
        currentState = (WebState)(((int)currentState + 1) % 3);
        playerEffectApplied = false;

        stateTimer = currentState switch
        {
            WebState.Idle => idleDuration,
            WebState.Partial => partialDuration,
            WebState.Full => fullDuration,
            _ => idleDuration
        };

        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetInteger("webState", (int)currentState);
        }

        Color color = currentState switch
        {
            WebState.Idle => new Color(0.45f, 0.58f, 0.66f),
            WebState.Partial => new Color(0.2f, 0.85f, 1f),
            WebState.Full => new Color(0.94f, 0.1f, 0.22f),
            _ => Color.white
        };

        if (renderers == null) return;
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            renderer.material.color = color;
            if (renderer.material.HasProperty("_BaseColor"))
            {
                renderer.material.SetColor("_BaseColor", color);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) ApplyEffect(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !playerEffectApplied) ApplyEffect(other);
    }

    void ApplyEffect(Collider playerCollider)
    {
        if (playerEffectApplied) return;

        if (currentState == WebState.Partial)
        {
            playerEffectApplied = true;
            PlayerMovement movement = playerCollider.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.ApplySlow(0.1f, timeParalised);
            }
        }
        else if (currentState == WebState.Full)
        {
            playerEffectApplied = true;
            DungeonGameRuntime.Instance?.DamagePlayer(1, transform.position);
        }
    }
}
