using UnityEngine;

public class Gnome : MonoBehaviour
{
    public float speed = 3.0f;
    public float detectionRange = 4.0f;
    public float rotationSpeed = 5.0f;
    public Animator ani;
    public GameObject targetPlayer;
    private bool isDead;

    void Start()
    {
        ani = GetComponent<Animator>();
        targetPlayer = GameObject.Find("Player");
        EnemyHitFeedback.Ensure(gameObject).SetHealth(1, 1);
    }

    void MoveGnome()
    {
        if (targetPlayer == null)
        {
            targetPlayer = GameObject.Find("Player");
            if (targetPlayer == null) return;
        }

        if (Vector3.Distance(transform.position, targetPlayer.transform.position) > detectionRange)
        {
            if (ani != null) ani.SetBool("movementActive", false);
        }
        else
        {
            Vector3 lookPos = targetPlayer.transform.position - transform.position;
            lookPos.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 100f);

            if (ani != null) ani.SetBool("movementActive", true);
            EnemyMovementUtility.MoveForwardKeepingFloor(transform, speed * Time.deltaTime);
        }
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

        MoveGnome();
    }
}
