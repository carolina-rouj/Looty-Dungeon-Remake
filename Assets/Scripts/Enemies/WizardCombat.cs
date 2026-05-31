using System.Collections;
using UnityEngine;

// Componente aditivo sobre Wizard.cs: dispara orbes magicos verdes cuando el jugador
// entra en rango. No modifica Wizard.cs ni su movimiento tile-based.
public class WizardCombat : MonoBehaviour
{
    public float shootRange    = 5.5f;
    public float shootInterval = 2.5f;
    public float orbSpeed      = 6f;

    private static readonly Color orbColor = new Color(0.15f, 0.95f, 0.35f);

    private Transform player;
    private Transform staffOrb;
    private float nextShootTime;
    private bool shooting;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        // La varita se crea en EnemyVisualBuilder.BuildWizard bajo "RuntimeEnemyVisual/Wizard Orb"
        staffOrb = transform.Find("RuntimeEnemyVisual/Wizard Orb");

        nextShootTime = Time.time + 1.5f;
    }

    void Update()
    {
        if (!EnemyMovementUtility.IsGameplayActive()) return;

        if (player == null || !player.gameObject.activeInHierarchy)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (!shooting && dist <= shootRange && Time.time >= nextShootTime)
        {
            nextShootTime = Time.time + shootInterval;
            StartCoroutine(ShootOrb());
        }
    }

    private IEnumerator ShootOrb()
    {
        shooting = true;

        Vector3 spawnPos = staffOrb != null
            ? staffOrb.position
            : transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;

        // Telegrafia: burst verde en la varita
        if (DungeonGameRuntime.Instance != null)
            DungeonGameRuntime.Instance.PlayEnemyCast(spawnPos, orbColor);

        yield return new WaitForSeconds(0.35f);

        if (player != null)
        {
            Vector3 dir = player.position - spawnPos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
            dir.Normalize();

            transform.rotation = Quaternion.LookRotation(dir);

            GameObject orbGo = new GameObject("WizardOrb");
            orbGo.transform.position = spawnPos;
            orbGo.transform.rotation = Quaternion.LookRotation(dir);
            WizardOrb orb = orbGo.AddComponent<WizardOrb>();
            orb.speed = orbSpeed;

            // Evita que el orbe colisione con el collider del propio mago
            Collider wizardCol = GetComponent<Collider>();
            Collider orbCol    = orbGo.GetComponent<Collider>();
            if (wizardCol != null && orbCol != null)
                Physics.IgnoreCollision(wizardCol, orbCol);
        }

        shooting = false;
    }
}
