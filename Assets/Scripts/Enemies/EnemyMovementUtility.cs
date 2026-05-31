using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyMovementUtility
{
    public static bool IsGameplayActive()
    {
        return DungeonGameRuntime.Instance == null || DungeonGameRuntime.Instance.IsPlaying;
    }

    public static void DisableEnemyAfterDeath(GameObject enemy)
    {
        foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        foreach (EnemyTouchDamage damage in enemy.GetComponents<EnemyTouchDamage>())
        {
            damage.enabled = false;
        }
    }
}

