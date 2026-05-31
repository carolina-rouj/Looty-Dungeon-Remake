using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector3 Facing { get; private set; } = Vector3.forward;
    public bool IsSlowed => Time.time <= slowUntil;
    public bool IsJelled => Time.time <= jellyUntil;
    public float CurrentSpeedMultiplier => IsSlowed ? slowFactor : 1f;

    private CharacterController controller;
    private Transform visual;            // RuntimePlayerVisual (para el arco del saltito)

    private float slowUntil;
    private float slowFactor = 1f;
    private float jellyUntil;
    private float fallGraceUntil;

    private bool hopping;
    private Vector3 hopFromXZ;
    private Vector3 hopToXZ;
    private float hopElapsed;
    private float hopDuration;

    private const float BaseHopDuration = 0.15f;  // segundos por casilla
    private const float HopHeight       = 0.22f;  // altura del saltito visual
    private const float Gravity         = 3f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void ResetAt(Vector3 position)
    {
        controller.enabled = false;
        Vector3 snapped = SnapXZ(position);
        transform.position = new Vector3(snapped.x, position.y + 0.05f, snapped.z);
        transform.rotation = Quaternion.LookRotation(Vector3.forward);
        Facing = Vector3.forward;
        slowUntil = 0f;
        slowFactor = 1f;
        jellyUntil = 0f;
        hopping = false;
        hopElapsed = 0f;
        if (visual != null) visual.localPosition = Vector3.zero;
        controller.enabled = true;
        fallGraceUntil = Time.time + 0.6f;
    }

    public void ApplySlow(float factor, float duration)
    {
        slowFactor = Mathf.Min(slowFactor, factor);
        slowUntil = Mathf.Max(slowUntil, Time.time + duration);
    }

    public void ApplyJelly(float duration)
    {
        slowFactor = 0f;
        slowUntil = Mathf.Max(slowUntil, Time.time + duration);
        jellyUntil = Mathf.Max(jellyUntil, Time.time + duration);
    }

    private void Update()
    {
        if (DungeonGameRuntime.Instance == null || !DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        EnsureVisual();

        if (Time.time > slowUntil)
        {
            slowFactor = 1f;
        }

        float tile = LevelManager.Instance != null ? LevelManager.Instance.tamañoCasilla : 1f;

        if (hopping)
        {
            AdvanceHop();
        }
        else
        {
            Vector3 dir = ReadStepDirection();
            if (dir != Vector3.zero)
            {
                // Encararse a la direccion pulsada aunque no se pueda avanzar (para atacar).
                Facing = dir;
                transform.rotation = Quaternion.LookRotation(dir);

                // Paralizado o engominado (rastro de slime): no avanza
                bool paralysed = IsJelled || CurrentSpeedMultiplier <= 0.1f;
                if (!paralysed)
                {
                    TryStartHop(dir, tile);
                }
            }
            else if (IsJelled && visual != null)
            {
                // temblor mientras esta atrapado, sin desplazarse
                visual.localPosition = new Vector3(Mathf.Sin(Time.time * 30f) * 0.03f, 0f, 0f);
            }
        }

        // Gravedad: mantiene al jugador pegado al suelo (y lo deja caer en el vacio).
        controller.Move(Vector3.down * Gravity * Time.deltaTime);

        if (LevelManager.Instance != null && Time.time > fallGraceUntil && !HasFloorBelow())
        {
            fallGraceUntil = Time.time + 0.7f;
            DungeonGameRuntime.Instance.RespawnPlayerAfterFall();
        }
    }

    private static Vector3 ReadStepDirection()
    {
        float h = DungeonInput.Horizontal();
        float v = DungeonInput.Vertical();
        if (Mathf.Abs(h) < 0.5f && Mathf.Abs(v) < 0.5f)
        {
            return Vector3.zero;
        }
        if (Mathf.Abs(v) >= Mathf.Abs(h))
        {
            return new Vector3(0f, 0f, Mathf.Sign(v));
        }
        return new Vector3(Mathf.Sign(h), 0f, 0f);
    }

    private void TryStartHop(Vector3 dir, float tile)
    {
        Vector3 from = SnapXZ(transform.position);
        Vector3 to = SnapXZ(from + dir * tile);

        // En el borde del nivel, SnapToGrid recorta y el destino == origen: no hay salto.
        if (Mathf.Approximately(from.x, to.x) && Mathf.Approximately(from.z, to.z))
        {
            return;
        }

        hopFromXZ = new Vector3(from.x, 0f, from.z);
        hopToXZ = new Vector3(to.x, 0f, to.z);
        hopElapsed = 0f;
        float mult = Mathf.Clamp(CurrentSpeedMultiplier, 0.25f, 1f);
        hopDuration = BaseHopDuration / mult;   // ralentizado => salto mas lento
        hopping = true;
    }

    private void AdvanceHop()
    {
        hopElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(hopElapsed / hopDuration);

        // Desplazamiento horizontal a traves del controller (respeta paredes/puerta).
        Vector3 desired = Vector3.Lerp(hopFromXZ, hopToXZ, t);
        Vector3 cur = transform.position;
        controller.Move(new Vector3(desired.x - cur.x, 0f, desired.z - cur.z));

        if (visual != null)
        {
            visual.localPosition = new Vector3(0f, Mathf.Sin(Mathf.PI * t) * HopHeight, 0f);
        }

        if (t >= 1f)
        {
            hopping = false;
            if (visual != null) visual.localPosition = Vector3.zero;
            Vector3 snapped = SnapXZ(transform.position);
            transform.position = new Vector3(snapped.x, transform.position.y, snapped.z);
        }
    }

    private Vector3 SnapXZ(Vector3 worldPos)
    {
        if (LevelManager.Instance != null)
        {
            return LevelManager.Instance.SnapToGrid(worldPos);
        }
        return worldPos;
    }

    private void EnsureVisual()
    {
        if (visual == null)
        {
            visual = transform.Find("RuntimePlayerVisual");
        }
    }

    private static readonly Collider[] floorProbe = new Collider[4];

    private bool HasFloorBelow()
    {
        Vector3 footXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        int floorMask = LayerMask.GetMask("Floor");
        return Physics.OverlapSphereNonAlloc(footXZ, 0.35f, floorProbe, floorMask) > 0;
    }
}
