using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector3 Facing { get; private set; } = Vector3.forward;
    public bool IsSlowed => Time.time <= slowUntil;
    public bool IsJelled => Time.time <= jellyUntil;
    public float CurrentSpeedMultiplier => IsSlowed ? slowFactor : 1f;
    public bool IsDashing => Time.time <= dashUntil;
    public float DashCooldownRemaining => Mathf.Max(0f, nextDashTime - Time.time);

    private CharacterController controller;
    private float speed = 4.4f;
    private float slowUntil;
    private float slowFactor = 1f;
    private float jellyUntil;
    private float fallGraceUntil;
    private Vector3 dashDirection;
    private float dashUntil;
    private float nextDashTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void ResetAt(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position + Vector3.up * 0.05f;
        transform.rotation = Quaternion.LookRotation(Vector3.forward);
        Facing = Vector3.forward;
        slowUntil = 0f;
        slowFactor = 1f;
        jellyUntil = 0f;
        dashUntil = 0f;
        nextDashTime = 0f;
        dashDirection = Vector3.zero;
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
        if (!DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        Vector3 input = ReadMovement();
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        if (input.sqrMagnitude > 0.01f)
        {
            Facing = input.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Facing), 16f * Time.deltaTime);
        }

        if (DungeonInput.DashPressed())
        {
            TryDash(input.sqrMagnitude > 0.01f ? input : Facing);
        }

        float multiplier = CurrentSpeedMultiplier;
        if (Time.time > slowUntil)
        {
            slowFactor = 1f;
        }

        if (IsDashing)
        {
            controller.Move(dashDirection * (11.5f * Time.deltaTime));
        }
        else
        {
            controller.Move(input * (speed * multiplier * Time.deltaTime));
        }
        controller.Move(Vector3.down * 3f * Time.deltaTime);

        if (IsJelled)
        {
            Vector3 wobble = new Vector3(
                Mathf.Sin(Time.time * 7f),
                0f,
                Mathf.Cos(Time.time * 5f)
            ) * 0.28f;
            controller.Move(wobble * Time.deltaTime);
        }

        if (LevelManager.Instance != null && Time.time > fallGraceUntil && !HasFloorBelow())
        {
            fallGraceUntil = Time.time + 0.7f;
            DungeonGameRuntime.Instance.RespawnPlayerAfterFall();
        }
    }

    // El LevelManager de Carolina coloca los tiles del suelo en la capa "Floor" (en y=0).
    // Detectamos si el jugador sigue sobre suelo sondeando esa capa bajo sus pies.
    private static readonly Collider[] floorProbe = new Collider[4];

    private bool HasFloorBelow()
    {
        Vector3 footXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        int floorMask = LayerMask.GetMask("Floor");
        return Physics.OverlapSphereNonAlloc(footXZ, 0.35f, floorProbe, floorMask) > 0;
    }

    public bool TryDash(Vector3 direction)
    {
        if (DungeonGameRuntime.Instance == null || !DungeonGameRuntime.Instance.IsPlaying || Time.time < nextDashTime || IsJelled)
        {
            return false;
        }

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
        {
            return false;
        }

        dashDirection = direction.normalized;
        Facing = dashDirection;
        transform.rotation = Quaternion.LookRotation(Facing);
        dashUntil = Time.time + 0.14f;
        nextDashTime = Time.time + 0.82f;
        fallGraceUntil = Time.time + 0.24f;

        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.PlayDash(transform.position, dashDirection);
        }

        return true;
    }

    private static Vector3 ReadMovement()
    {
        return new Vector3(DungeonInput.Horizontal(), 0f, DungeonInput.Vertical());
    }
}

