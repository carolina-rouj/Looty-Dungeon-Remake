using UnityEngine;

// Glue de Mateus: el runtime lo engancha a cada enemigo de Carolina al descubrirlo.
// Cuando el enemigo se autodestruye tras morir (su Die() llama a Destroy), avisa al
// runtime para contabilizar la baja, abrir la puerta al limpiar la sala y soltar
// recompensas. No requiere modificar los scripts de enemigo de Carolina.
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
