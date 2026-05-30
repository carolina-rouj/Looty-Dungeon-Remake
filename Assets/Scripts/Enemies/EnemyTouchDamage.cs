using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    private float nextHitTime;

    public static void Ensure(GameObject enemy)
    {
        EnemyTouchDamage damage = enemy.GetComponent<EnemyTouchDamage>();
        if (damage == null)
        {
            damage = enemy.AddComponent<EnemyTouchDamage>();
        }

        SphereCollider trigger = enemy.GetComponent<SphereCollider>();
        if (trigger == null)
        {
            trigger = enemy.AddComponent<SphereCollider>();
        }
        trigger.isTrigger = true;
        trigger.radius = 0.62f;
        trigger.center = Vector3.up * 0.55f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < nextHitTime || !other.CompareTag("Player"))
        {
            return;
        }

        nextHitTime = Time.time + 1.0f;
        DungeonGameRuntime.Instance.DamagePlayer(1, transform.position);
    }

    private void Update()
    {
        if (DungeonGameRuntime.Instance == null || !DungeonGameRuntime.Instance.IsPlaying || Time.time < nextHitTime)
        {
            return;
        }

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            return;
        }

        Vector3 delta = player.transform.position - transform.position;
        delta.y = 0f;
        if (delta.sqrMagnitude <= 0.75f * 0.75f)
        {
            nextHitTime = Time.time + 1.0f;
            DungeonGameRuntime.Instance.DamagePlayer(1, transform.position);
        }
    }
}

