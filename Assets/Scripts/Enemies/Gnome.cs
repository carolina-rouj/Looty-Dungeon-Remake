using System.Collections;
using UnityEngine;

public class Gnome : MonoBehaviour
{
    public float timeWaitMove   = 0.6f;
    public float speed          = 4.0f;
    public float detectionRange = 4.0f;
    public float scaleSpeed     = 10f;
    public Animator ani;

    private Transform player;
    private float tamañoCasilla;
    private bool movementActive = false;
    private bool isLanding      = false;
    private bool isPreJumping   = false;
    private float timer         = 0f;
    private Vector3 targetPosition, lastPosition, groundPosition;
    private Vector3 normalScale, targetScale;
    private int obstacleLayerMask;
    private readonly Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };

    void Start()
    {
        ani = GetComponent<Animator>();
        normalScale = transform.localScale;
        targetScale = normalScale;
        obstacleLayerMask = ~LayerMask.GetMask("Floor");
        tamañoCasilla = LevelManager.Instance != null ? LevelManager.Instance.tamañoCasilla : 1f;

        var pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) player = pm.transform;

        if (LevelManager.Instance != null)
        {
            Vector3 snapped = LevelManager.Instance.SnapToGrid(transform.position);
            snapped.y = LevelManager.Instance.enemySpawnHeight;
            transform.position = snapped;
            targetPosition = snapped;
            lastPosition   = snapped;
        }
        else
        {
            targetPosition = transform.position;
            lastPosition   = transform.position;
        }
    }

    // --- Daño y muerte ---

    public void Hurt() => Die();

    private void Die()
    {
        movementActive = false;
        isPreJumping   = false;
        ani.SetTrigger("die");
        Destroy(gameObject, 1.0f);
    }

    // --- Movimiento ---

    void SelectDirection()
    {
        Physics.SyncTransforms();
        int floorMask = LayerMask.GetMask("Floor");

        bool chasing = player != null &&
                       Vector3.Distance(transform.position, player.position) <= detectionRange;

        Vector3 chosenDir = chasing
            ? BestDirToward(player.position, floorMask)
            : RandomFreeDir(floorMask);

        if (chosenDir == Vector3.zero) return;

        Vector3 candidate = transform.position + chosenDir * tamañoCasilla;
        Vector3 snapped   = LevelManager.Instance != null
            ? LevelManager.Instance.SnapToGrid(candidate)
            : candidate;
        if (LevelManager.Instance != null && !LevelManager.Instance.IsInBounds(snapped))
            return;

        float groundY = LevelManager.Instance != null
            ? LevelManager.Instance.enemySpawnHeight
            : transform.position.y;
        groundPosition = new Vector3(snapped.x, groundY, snapped.z);

        transform.rotation = Quaternion.LookRotation(chosenDir);
        isLanding     = false;
        isPreJumping  = true;
        targetScale   = new Vector3(normalScale.x * 1.2f, normalScale.y * 0.7f, normalScale.z * 1.2f);
        StartCoroutine(JumpSequence());
    }

    Vector3 BestDirToward(Vector3 target, int floorMask)
    {
        Vector3 best      = Vector3.zero;
        float   bestDist  = float.MaxValue;
        foreach (Vector3 dir in directions)
        {
            Vector3 candidate = transform.position + dir * tamañoCasilla;
            if (!IsPassable(candidate, floorMask)) continue;
            float dist = Vector3.Distance(candidate, target);
            if (dist < bestDist) { bestDist = dist; best = dir; }
        }
        return best;
    }

    Vector3 RandomFreeDir(int floorMask)
    {
        // Fisher-Yates shuffle
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (directions[i], directions[j]) = (directions[j], directions[i]);
        }
        foreach (Vector3 dir in directions)
        {
            Vector3 candidate = transform.position + dir * tamañoCasilla;
            if (IsPassable(candidate, floorMask)) return dir;
        }
        return Vector3.zero;
    }

    bool IsPassable(Vector3 candidate, int floorMask)
    {
        // Obstáculos a altura del gnomo (otros enemigos, decoraciones)
        Collider[] hits = Physics.OverlapSphere(candidate, 0.4f, obstacleLayerMask);
        foreach (Collider c in hits)
            if (c.gameObject != gameObject) return false;

        // Obstáculos a nivel del suelo (paredes cortas)
        Collider[] floorHits = Physics.OverlapSphere(
            new Vector3(candidate.x, 0f, candidate.z), 0.4f, obstacleLayerMask);
        foreach (Collider c in floorHits)
            if (c.gameObject != gameObject) return false;

        // Tile de suelo existe
        return Physics.OverlapSphere(
            new Vector3(candidate.x, 0f, candidate.z), 0.3f, floorMask).Length > 0;
    }

    IEnumerator JumpSequence()
    {
        yield return new WaitForSeconds(0.1f);
        targetScale    = new Vector3(normalScale.x * 0.8f, normalScale.y * 1.2f, normalScale.z * 0.8f);
        targetPosition = groundPosition + Vector3.up;
        isPreJumping   = false;
        movementActive = true;
        ani.SetBool("movementActive", true);
    }

    void MoveGnome()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            if (!isLanding)
            {
                isLanding      = true;
                targetPosition = groundPosition;
            }
            else
            {
                targetScale = new Vector3(normalScale.x * 1.2f, normalScale.y * 0.8f, normalScale.z * 1.2f);
                StartCoroutine(ReturnToNormal());
                lastPosition   = groundPosition;
                isLanding      = false;
                movementActive = false;
                timer          = 0f;
                ani.SetBool("movementActive", false);
            }
        }
    }

    IEnumerator ReturnToNormal()
    {
        yield return new WaitForSeconds(0.1f);
        targetScale = normalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);

        if (movementActive)
            MoveGnome();
        else if (!isPreJumping)
        {
            timer += Time.deltaTime;
            if (timer >= timeWaitMove)
            {
                timer = 0f;
                SelectDirection();
            }
        }
    }
}
