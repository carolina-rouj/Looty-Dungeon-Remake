using UnityEngine;

public class RetractileFork : MonoBehaviour
{
    public enum ForkState { Idle, Extending, Retracting }

    public Transform forkMesh;
    public float idleDuration = 1f;
    public float extendDuration = 0.25f;
    public float retractDuration = 0.25f;
    public float extendDistance = 1.1f;  // cuánto avanza el fork (eje +X local)
    public int damage = 1;

    private ForkState state = ForkState.Idle;
    private float stateTimer = 0f;
    private Vector3 retractedPos;
    private Vector3 extendedPos;

    void Start()
    {
        retractedPos = forkMesh.localPosition;
        extendedPos  = retractedPos + Vector3.right * extendDistance;
        stateTimer = idleDuration;
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case ForkState.Idle:
                if (stateTimer <= 0f) EnterState(ForkState.Extending);
                break;

            case ForkState.Extending:
            {
                float t = 1f - Mathf.Clamp01(stateTimer / extendDuration);
                forkMesh.localPosition = Vector3.Lerp(retractedPos, extendedPos, t);

                if (stateTimer <= 0f) EnterState(ForkState.Retracting);
                break;
            }

            case ForkState.Retracting:
            {
                float t = Mathf.Clamp01(stateTimer / retractDuration);
                forkMesh.localPosition = Vector3.Lerp(retractedPos, extendedPos, t);

                if (stateTimer <= 0f)
                {
                    forkMesh.localPosition = retractedPos;
                    EnterState(ForkState.Idle);
                }
                break;
            }
        }
    }

    void EnterState(ForkState newState)
    {
        state = newState;
        stateTimer = newState switch
        {
            ForkState.Idle       => idleDuration,
            ForkState.Extending  => extendDuration,
            ForkState.Retracting => retractDuration,
            _ => idleDuration,
        };
    }

    void OnTriggerStay(Collider other)
    {
        if (state == ForkState.Idle) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage, transform.position);
        }
    }
}
