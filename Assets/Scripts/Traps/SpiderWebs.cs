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
    private bool playerEffectApplied = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        stateTimer = idleDuration;
        UpdateAnimator();
    }

    void Update()
    {
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
        animator?.SetInteger("webState", (int)currentState);
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
            // TODO: playerCollider.GetComponent<Player>().ApplyParalysis(timeParalised);
        }
        else if (currentState == WebState.Full)
        {
            playerEffectApplied = true;
            // TODO: playerCollider.GetComponent<Player>().TakeDamage(1);
        }
    }
}
