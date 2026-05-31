using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private readonly Collider[] hits = new Collider[24];
    private readonly HashSet<GameObject> damagedThisSwing = new HashSet<GameObject>();
    private PlayerMovement movement;
    private float nextAttackTime;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        if (DungeonInput.AttackPressed() && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    public void Attack()
    {
        nextAttackTime = Time.time + 0.34f;
        damagedThisSwing.Clear();

        Vector3 center = transform.position + Vector3.up * 0.75f + movement.Facing * 0.75f;
        int count = Physics.OverlapSphereNonAlloc(center, 0.85f, hits);
        bool hitEnemy = false;

        for (int i = 0; i < count; i++)
        {
            Collider hit = hits[i];
            if (hit == null || hit.transform == transform)
            {
                continue;
            }

            GameObject root = FindEnemyRoot(hit.transform);
            if (root == null || damagedThisSwing.Contains(root))
            {
                continue;
            }

            Vector3 toTarget = root.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.01f && Vector3.Dot(movement.Facing, toTarget.normalized) < -0.15f)
            {
                continue;
            }

            damagedThisSwing.Add(root);
            root.SendMessage("Hurt", SendMessageOptions.DontRequireReceiver);
            hitEnemy = true;
        }

        DungeonGameRuntime.Instance.PlayAttackSfx(hitEnemy);
        RuntimeVfx.Slash(center, movement.Facing, hitEnemy ? Color.yellow : Color.white);
    }

    private static GameObject FindEnemyRoot(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.GetComponent<Slime>() != null ||
                current.GetComponent<Bat>() != null ||
                current.GetComponent<Wizard>() != null ||
                current.GetComponent<Gnome>() != null ||
                current.GetComponent<Boss>() != null)
            {
                return current.gameObject;
            }
            current = current.parent;
        }

        return null;
    }
}

