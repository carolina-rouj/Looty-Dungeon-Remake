using UnityEngine;

public class RetractileFork : MonoBehaviour
{
    public enum ForkState { Idle, Extending, Retracting } // 3 estados del fork state

    public float idleDuration = 1f; //intervalo en cada retraccion
    public float extendDuration = 0.25f;  // tiempo en extenderse
    public float retractDuration = 0.25f;  // tiempo en retraerse
    public float extendDistance = 1f; //maxima distancia del tenedor retractil (1 casilla)
    public float spinSpeed = 720f;  // efecto giratorio del tenedor
    public int damage = 1;

    private ForkState state = ForkState.Idle;
    private float stateTimer = 0f;
    private Vector3 retractedPos;
    private Vector3 extendedPos;

    void Start()
    {
        retractedPos = transform.localPosition;
        extendedPos = retractedPos + new Vector3(extendDistance, 0f, 0f);
        stateTimer = idleDuration;
    }

    void Update()
    {
        if (DungeonGameRuntime.Instance != null && !DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case ForkState.Idle:
                if (stateTimer <= 0f) EnterState(ForkState.Extending);
                break;

            case ForkState.Extending:
            {
                float t = 1f - Mathf.Clamp01(stateTimer/extendDuration);
                transform.localPosition = Vector3.Lerp(retractedPos, extendedPos, t);
                transform.Rotate(Vector3.right * spinSpeed * Time.deltaTime, Space.Self);

                if (stateTimer <= 0f) EnterState(ForkState.Retracting);
                break;
            }

            case ForkState.Retracting:
            {
                float t = Mathf.Clamp01(stateTimer/retractDuration);
                transform.localPosition = Vector3.Lerp(retractedPos, extendedPos, t);
                transform.Rotate(Vector3.right * spinSpeed * Time.deltaTime, Space.Self);

                if (stateTimer <= 0f)
                {
                    transform.localPosition = retractedPos;
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
            ForkState.Idle => idleDuration,
            ForkState.Extending => extendDuration,
            ForkState.Retracting => retractDuration,
            _ => idleDuration,
        };
    }

    void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        if (state == ForkState.Idle) return;

        if (other.CompareTag("Player"))
        {
            DungeonGameRuntime.Instance?.DamagePlayer(damage, transform.position);
        }
    }
}
