using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{
    public float timeWaitMove = 1.0f;
    public float speed = 3.0f;
    public float tamañoCasilla = 1.0f;
    public float scaleSpeed = 10f;
    public int lives = 1;
    public GameObject rastroSlimePrefab;
    public Animator ani;

    private bool movementActive = false;
    private bool isLanding = false;
    private bool isPreJumping = false;
    private float timer = 0f;
    private Vector3 targetPosition, lastPosition, groundPosition;
    private Vector3 normalScale, targetScale;
    private Vector3[] directions = new Vector3[] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
    private int obstacleLayerMask;

    void Start()
    {
        ani = GetComponent<Animator>();
        normalScale = transform.localScale;
        targetScale = normalScale;
        obstacleLayerMask = ~LayerMask.GetMask("Floor");

        // Alinear al centro de casilla más cercano al inicio y fijar Y correcta
        if (LevelManager.Instance != null)
        {
            Vector3 snapped = LevelManager.Instance.SnapToGrid(transform.position);
            snapped.y = LevelManager.Instance.enemySpawnHeight;
            transform.position = snapped;
            targetPosition     = snapped;
            lastPosition       = snapped;
        }
        else
        {
            targetPosition = transform.position;
            lastPosition   = transform.position;
        }
    }

    // --- Daño y muerte ---

    public void Hurt()
    {
        --lives;
        if (lives <= 0) Die();
    }

    private void Die()
    {
        movementActive = false;
        isPreJumping = false;
        Destroy(gameObject, 0.5f);
    }

    // --- Movimiento ---

    void SelectDirection()
    {
        ShuffleDirections();

        int floorMask = LayerMask.GetMask("Floor");
        Physics.SyncTransforms(); // asegurar posiciones actualizadas antes de los OverlapSphere
        bool found = false;
        foreach (Vector3 dir in directions)
        {
            Vector3 candidate = transform.position + dir * tamañoCasilla;

            // Check obstáculos a altura del slime (otros enemigos, decoraciones)
            bool blocked = false;
            Collider[] hits = Physics.OverlapSphere(candidate, 0.4f, obstacleLayerMask);
            foreach (Collider c in hits)
                if (c.gameObject != gameObject) { blocked = true; break; }

            // También a nivel del suelo: detecta paredes cortas que no llegan a la altura del slime
            if (!blocked)
            {
                Collider[] floorHits = Physics.OverlapSphere(
                    new Vector3(candidate.x, 0f, candidate.z), 0.4f, obstacleLayerMask);
                foreach (Collider c in floorHits)
                    if (c.gameObject != gameObject) { blocked = true; break; }
            }
            if (blocked) continue;

            // Check suelo: buscamos en Y=0 porque los tiles están ahí
            Vector3 floorCheck = new Vector3(candidate.x, 0f, candidate.z);
            if (Physics.OverlapSphere(floorCheck, 0.3f, floorMask).Length == 0) continue;

            // Snap al centro exacto de la casilla destino
            Vector3 snapped = LevelManager.Instance != null
                ? LevelManager.Instance.SnapToGrid(candidate)
                : candidate;
            if (LevelManager.Instance != null && !LevelManager.Instance.IsInBounds(snapped))
                continue;
            float groundY = LevelManager.Instance != null
                ? LevelManager.Instance.enemySpawnHeight
                : transform.position.y;
            groundPosition = new Vector3(snapped.x, groundY, snapped.z);
            found = true;
            break;
        }

        if (!found) return;

        isLanding = false;
        isPreJumping = true;
        targetScale = new Vector3(normalScale.x * 1.3f, normalScale.y * 0.6f, normalScale.z * 1.3f);
        StartCoroutine(JumpSequence());
    }

    void ShuffleDirections()
    {
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (directions[i], directions[j]) = (directions[j], directions[i]);
        }
    }

    IEnumerator JumpSequence()
    {
        yield return new WaitForSeconds(0.12f);
        targetScale = new Vector3(normalScale.x * 0.7f, normalScale.y * 1.2f, normalScale.z * 0.7f);
        targetPosition = groundPosition + Vector3.up;
        isPreJumping = false;
        movementActive = true;
        ani.SetBool("movementActive", true);
    }

    public void MoveSlime()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            if (!isLanding)
            {
                // El slime acaba de abandonar la casilla anterior: spawneamos el rastro ahora
                if (rastroSlimePrefab != null && lastPosition != groundPosition)
                {
                    Vector3 rastroPos = new Vector3(lastPosition.x, 1f, lastPosition.z);
                    Instantiate(rastroSlimePrefab, rastroPos, Quaternion.identity);
                }

                isLanding = true;
                targetPosition = groundPosition;
            }
            else
            {
                targetScale = new Vector3(normalScale.x * 1.3f, normalScale.y * 0.6f, normalScale.z * 1.3f);
                StartCoroutine(ReturnToNormal());

                lastPosition = groundPosition;
                isLanding = false;
                movementActive = false;
                timer = 0f;
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

        if (movementActive) MoveSlime();
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
