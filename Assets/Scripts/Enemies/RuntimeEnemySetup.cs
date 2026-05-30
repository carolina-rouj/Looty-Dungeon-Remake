using UnityEngine;

// Va en los prefabs minimos de enemigos (Bat/Wizard/Gnome/Boss). En Awake monta el
// enemigo completo: collider, Animator (sin controller, evita NRE en los scripts de
// Carolina que hacen GetComponent<Animator>()), el script de IA de Carolina y el visual
// procedural. Asi el prefab en disco es trivial y el "look" se genera en runtime, igual
// que el del jugador.
public class RuntimeEnemySetup : MonoBehaviour
{
    public string enemyType = "Bat"; // "Bat", "Wizard", "Gnome", "Boss", "Slime"

    private void Awake()
    {
        bool isBoss = enemyType == "Boss";

        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        collider.height = isBoss ? 1.8f : 1.2f;
        collider.radius = isBoss ? 0.65f : 0.38f;
        collider.center = Vector3.up * collider.height * 0.5f;

        // Animator sin controller: los scripts de enemigo de Carolina hacen
        // ani.SetTrigger/SetBool; con un Animator presente esas llamadas son no-op en vez
        // de lanzar NullReferenceException.
        gameObject.AddComponent<Animator>();

        switch (enemyType)
        {
            case "Bat":    gameObject.AddComponent<Bat>();    break;
            case "Wizard": gameObject.AddComponent<Wizard>(); break;
            case "Gnome":  gameObject.AddComponent<Gnome>();  break;
            case "Boss":   gameObject.AddComponent<Boss>();   break;
            case "Slime":  gameObject.AddComponent<Slime>();  break;
            default:       gameObject.AddComponent<Gnome>();  break;
        }

        EnemyVisualBuilder.Build(enemyType, transform);
    }
}
