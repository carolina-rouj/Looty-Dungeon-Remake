using UnityEngine;

public class Bat : MonoBehaviour
{
    public float speed = 3.0f;
    public float detectionRange = 4.0f;
    public float rotationSpeed = 5.0f;
    public Animator ani;
    // TODO (player): public GameObject targetPlayer;

    void Start()
    {
        ani = GetComponent<Animator>();
        // TODO (player): targetPlayer = GameObject.Find("Player");
    }

    void MoveBat()
    {
        // TODO (player): descomentar cuando el player esté implementado
        // if (Vector3.Distance(transform.position, targetPlayer.transform.position) > detectionRange)
        // {
        //     ani.SetBool("movementActive", false);
        // }
        // else
        // {
        //     Vector3 lookPos = targetPlayer.transform.position - transform.position;
        //     lookPos.y = 0;
        //     Quaternion targetRotation = Quaternion.LookRotation(lookPos);
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 100f);
        //     ani.SetBool("movementActive", true);
        //     transform.Translate(Vector3.forward * speed * Time.deltaTime);
        // }
    }

    public void Hurt()
    {
        if (isDead) return;
        EnemyHitFeedback.Ensure(gameObject).Hit(0, 1);
        Die();
    }

    private void Die()
    {
        isDead = true;
        EnemyMovementUtility.DisableEnemyAfterDeath(gameObject);
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.NotifyEnemyDefeated(gameObject);
            DungeonGameRuntime.Instance.PlayEnemyDeath(transform.position);
        }
        if (ani != null) ani.SetTrigger("die");
        Destroy(gameObject, 1.0f);
    }

    void Update()
    {
        if (isDead || !EnemyMovementUtility.IsGameplayActive())
        {
            if (ani != null) ani.SetBool("movementActive", false);
            return;
        }

        MoveBat();
    }
}
