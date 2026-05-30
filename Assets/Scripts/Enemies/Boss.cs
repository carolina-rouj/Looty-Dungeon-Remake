using UnityEngine;

public class Boss : MonoBehaviour
{
    public float speed = 3.0f;
    public float detectionRange = 5.0f;
    public float rotationSpeed = 5.0f;
    public Animator ani;
    public GameObject targetPlayer;

    private int lives = 8;
    private int maxLives;
    private bool isDead;
    private float nextCastTime;

    void Start()
    {
        ani = GetComponent<Animator>();
        targetPlayer = GameObject.Find("Player");
        maxLives = lives;
        EnemyHitFeedback.Ensure(gameObject).SetHealth(lives, maxLives);
    }

    void MoveBoss()
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

    private void TryCast()
    {
        if (targetPlayer == null || Time.time < nextCastTime)
        {
            return;
        }

        Vector3 toPlayer = targetPlayer.transform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (distance < 2.2f || distance > detectionRange)
        {
            return;
        }

        nextCastTime = Time.time + 2.35f;
        Color spellColor = new Color(1f, 0.28f, 0.08f);
        Vector3 start = transform.position + Vector3.up * 1.05f;
        EnemyProjectile.Launch(start, toPlayer.normalized, 3.8f, spellColor, 0.34f);

        Vector3 side = Vector3.Cross(Vector3.up, toPlayer.normalized);
        EnemyProjectile.Launch(start, (toPlayer.normalized + side * 0.45f).normalized, 3.4f, spellColor, 0.26f);
        EnemyProjectile.Launch(start, (toPlayer.normalized - side * 0.45f).normalized, 3.4f, spellColor, 0.26f);

        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.PlayEnemyCast(transform.position, spellColor);
        }
    }

    public void Hurt()
    {
        if (isDead) return;
        --lives;
        EnemyHitFeedback.Ensure(gameObject).Hit(Mathf.Max(0, lives), maxLives);
        if (lives <= 0) Die();
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

        MoveBoss();
        TryCast();
    }
}
