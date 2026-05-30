using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private float invulnerableUntil;
    private Renderer[] renderers;

    private void Awake()
    {
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        if (Time.time < invulnerableUntil)
        {
            return;
        }

        invulnerableUntil = Time.time + 0.8f;
        DungeonGameRuntime.Instance.CommitPlayerDamage(amount);
        RuntimeVfx.Burst(transform.position + Vector3.up, Color.red, 18, 0.45f);
        StartCoroutine(Flash());

        Vector3 away = transform.position - sourcePosition;
        away.y = 0f;
        if (away.sqrMagnitude > 0.01f)
        {
            CharacterController controller = GetComponent<CharacterController>();
            controller.Move(away.normalized * 0.35f);
        }
    }

    private IEnumerator Flash()
    {
        float end = Time.time + 0.55f;
        while (Time.time < end)
        {
            bool visible = Mathf.FloorToInt(Time.time * 18f) % 2 == 0;
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }
            yield return null;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }
}

