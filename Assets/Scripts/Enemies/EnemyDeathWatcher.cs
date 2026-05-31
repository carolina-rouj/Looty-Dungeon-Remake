using UnityEngine;

public class EnemyDeathWatcher : MonoBehaviour
{
    private DungeonGameRuntime runtime;

    public static void Ensure(GameObject enemy, DungeonGameRuntime runtime)
    {
        EnemyDeathWatcher watcher = enemy.GetComponent<EnemyDeathWatcher>();
        if (watcher == null)
        {
            watcher = enemy.AddComponent<EnemyDeathWatcher>();
        }
        watcher.runtime = runtime;
    }

    private void OnDestroy()
    {
        if (runtime != null)
        {
            runtime.HandleEnemyDestroyed(gameObject);
        }
    }
}
