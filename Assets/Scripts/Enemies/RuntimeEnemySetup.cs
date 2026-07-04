using UnityEngine;

public class RuntimeEnemySetup : MonoBehaviour
{
    public string enemyType = "Slime"; // "Slime", "Wizard", "Gnome", "Boss"

    private void Awake()
    {
        bool isBoss = enemyType == "Boss";

        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        collider.height = isBoss ? 1.8f : 1.2f;
        collider.radius = isBoss ? 0.65f : 0.38f;
        collider.center = Vector3.up * collider.height * 0.5f;

        gameObject.AddComponent<Animator>();

        switch (enemyType)
        {
            case "Wizard":
                gameObject.AddComponent<Wizard>();
                gameObject.AddComponent<WizardCombat>();
                break;
            case "Gnome":  gameObject.AddComponent<Gnome>();  break;
            case "Boss":   gameObject.AddComponent<Boss>(); break;
            case "Slime":  gameObject.AddComponent<Slime>();  break;
            default:       gameObject.AddComponent<Gnome>();  break;
        }

        EnemyVisualBuilder.Build(enemyType, transform);
        EnemyFloorFall.Ensure(gameObject);
    }
}
