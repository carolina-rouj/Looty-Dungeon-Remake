using UnityEngine;

public class SpiderWebs : MonoBehaviour
{
    public enum WebState { Idle = 0, Partial = 1, Full = 2 }

    public float idleDuration = 1.2f;
    public float partialDuration = 1.2f;
    public float fullDuration = 1.2f;
    public float timeParalised = 1.5f;
    public GameObject tela1Prefab;

    private WebState currentState = WebState.Idle;
    private float stateTimer = 0f;
    private bool playerEffectApplied = false;

    private Transform[] barTransforms;
    private float[] currentHeights;

    const float lerpSpeed     = 6f;
    const float tubeThickness = 0.06f;
    const float floorSurface  = 1.0f;

    static readonly Vector2[] positions = {
        new(0f, 0f), new(-0.3f, 0.2f),   new(0.3f, -0.25f),
        new(-0.2f, -0.32f), new(0.22f, 0.32f), new(-0.38f, 0.05f),
        new(0.38f, -0.05f), new(0.05f, -0.38f), new(-0.08f, 0.38f)
    };

    static readonly float[] idleTargets    = { 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f };
    static readonly float[] partialTargets = { 0.08f, 0.14f, 0.10f, 0.18f, 0.07f, 0.15f, 0.11f, 0.13f, 0.09f };
    static readonly float[] fullTargets    = { 0.38f, 0.55f, 0.42f, 0.31f, 0.48f, 0.35f, 0.52f, 0.30f, 0.44f };

    void Start()
    {
        if (tela1Prefab != null)
        {
            var deco = Instantiate(tela1Prefab, transform);
            deco.transform.localPosition = Vector3.zero;
            deco.transform.localScale = Vector3.one * 0.25f;
        }

        Material mat = RuntimeMaterials.GetEmissive("web_tubes", Color.white, 1.2f);
        barTransforms = new Transform[9];
        currentHeights = new float[9];

        for (int i = 0; i < 9; i++)
        {
            currentHeights[i] = idleTargets[i];
            float h = currentHeights[i];

            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(bar.GetComponent<BoxCollider>());
            bar.transform.SetParent(transform, false);
            bar.transform.localPosition = new Vector3(positions[i].x, floorSurface + h * 0.5f, positions[i].y);
            bar.transform.localScale    = new Vector3(tubeThickness, h, tubeThickness);
            bar.GetComponent<Renderer>().material = mat;
            barTransforms[i] = bar.transform;
        }

        stateTimer = idleDuration;
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) AdvanceState();

        float[] targets = currentState switch
        {
            WebState.Idle    => idleTargets,
            WebState.Partial => partialTargets,
            _                => fullTargets
        };

        for (int i = 0; i < 9; i++)
        {
            currentHeights[i] = Mathf.Lerp(currentHeights[i], targets[i], Time.deltaTime * lerpSpeed);
            barTransforms[i].localScale    = new Vector3(tubeThickness, currentHeights[i], tubeThickness);
            barTransforms[i].localPosition = new Vector3(positions[i].x, floorSurface + currentHeights[i] * 0.5f, positions[i].y);
        }
    }

    void AdvanceState()
    {
        currentState = (WebState)(((int)currentState + 1) % 3);
        playerEffectApplied = false;

        stateTimer = currentState switch
        {
            WebState.Idle    => idleDuration,
            WebState.Partial => partialDuration,
            WebState.Full    => fullDuration,
            _                => idleDuration
        };
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
            playerCollider.GetComponent<PlayerMovement>()?.ApplySlow(0f, timeParalised);
            DungeonGameRuntime.Instance?.PlayTrapActivated(transform.position, "Web");
        }
        else if (currentState == WebState.Full)
        {
            playerEffectApplied = true;
            playerCollider.GetComponent<PlayerHealth>()?.TakeDamage(1, transform.position);
            DungeonGameRuntime.Instance?.PlayTrapActivated(transform.position, "Web");
        }
    }
}
