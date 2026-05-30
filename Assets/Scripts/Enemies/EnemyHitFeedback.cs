using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitFeedback : MonoBehaviour
{
    private Renderer[] renderers;
    private MaterialPropertyBlock flashBlock;
    private Coroutine flashRoutine;
    private Transform barRoot;
    private Transform barFill;
    private int maxHealth = 1;

    public static EnemyHitFeedback Ensure(GameObject enemy)
    {
        EnemyHitFeedback feedback = enemy.GetComponent<EnemyHitFeedback>();
        if (feedback == null)
        {
            feedback = enemy.AddComponent<EnemyHitFeedback>();
        }

        feedback.CacheRenderers();
        return feedback;
    }

    private void Awake()
    {
        CacheRenderers();
    }

    private void Update()
    {
        if (barRoot != null && Camera.main != null)
        {
            barRoot.rotation = Camera.main.transform.rotation;
        }
    }

    public void SetHealth(int currentHealth, int configuredMaxHealth)
    {
        maxHealth = Mathf.Max(1, configuredMaxHealth);
        if (maxHealth <= 1)
        {
            return;
        }

        EnsureHealthBar();
        UpdateHealthBar(currentHealth);
    }

    public void Hit(int currentHealth, int configuredMaxHealth)
    {
        SetHealth(currentHealth, configuredMaxHealth);
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.PlayEnemyHit(transform.position);
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(Flash());
    }

    private void CacheRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();
        if (flashBlock == null)
        {
            flashBlock = new MaterialPropertyBlock();
        }
    }

    private IEnumerator Flash()
    {
        CacheRenderers();
        float end = Time.time + 0.18f;
        Color flashColor = new Color(1f, 0.95f, 0.35f);

        while (Time.time < end)
        {
            bool visible = Mathf.FloorToInt(Time.time * 40f) % 2 == 0;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                if (visible)
                {
                    flashBlock.Clear();
                    flashBlock.SetColor("_BaseColor", flashColor);
                    flashBlock.SetColor("_Color", flashColor);
                    renderer.SetPropertyBlock(flashBlock);
                }
                else
                {
                    renderer.SetPropertyBlock(null);
                }
            }

            yield return null;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.SetPropertyBlock(null);
            }
        }
    }

    private void EnsureHealthBar()
    {
        if (barRoot != null)
        {
            return;
        }

        barRoot = new GameObject("Enemy Health Bar").transform;
        barRoot.SetParent(transform, false);
        barRoot.localPosition = Vector3.up * GetBarHeight();

        CreateBarPart("Bar Back", barRoot, new Vector3(0f, 0f, 0.01f), new Vector3(0.92f, 0.09f, 0.04f), new Color(0.08f, 0.05f, 0.05f));
        barFill = CreateBarPart("Bar Fill", barRoot, Vector3.zero, new Vector3(0.86f, 0.07f, 0.045f), new Color(0.25f, 0.95f, 0.28f)).transform;
    }

    private void UpdateHealthBar(int currentHealth)
    {
        if (barFill == null)
        {
            return;
        }

        float fraction = Mathf.Clamp01(currentHealth / (float)maxHealth);
        barFill.localScale = new Vector3(0.86f * fraction, 0.07f, 0.045f);
        barFill.localPosition = new Vector3(-0.43f * (1f - fraction), 0f, 0f);
    }

    private float GetBarHeight()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            return Mathf.Max(1.2f, collider.bounds.size.y + 0.35f);
        }

        return 1.45f;
    }

    private static GameObject CreateBarPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = RuntimeMaterials.Get("enemy_health_" + name, color);
        Object.Destroy(part.GetComponent<Collider>());
        return part;
    }
}

