using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    public GameObject arrowPrefab;
    public float shootInterval = 2.0f; // variable dependiendo de la posicion de la diana
    public float arrowSpeed = 5.0f;
    public float timeToNextShot;
    public int damage = 1;
    public int distanceToDiana;

    public Transform diana;     
    private float timer = 0f;

    void Start()
    {
        if (diana != null) distanceToDiana = (int)Vector3.Distance(transform.position, diana.position);
        shootInterval = Mathf.Max(0.5f, distanceToDiana * 0.5f);
    }

    void Update()
    {
        if (DungeonGameRuntime.Instance == null || !DungeonGameRuntime.Instance.IsPlaying)
        {
            return;
        }

        if (Diana.IsPlayerOnAnyDiana)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Disparo();
                timer = shootInterval;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    void Disparo()
    {
        if (diana == null) return;

        Vector3 direction = (diana.position - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject arrow = arrowPrefab != null
            ? Instantiate(arrowPrefab, transform.position, rotation)
            : GameObject.CreatePrimitive(PrimitiveType.Capsule);

        if (arrowPrefab == null)
        {
            arrow.name = "Arrow";
            arrow.transform.position = transform.position;
            arrow.transform.rotation = rotation;
            arrow.transform.localScale = new Vector3(0.12f, 0.12f, 0.55f);
            Collider collider = arrow.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;
            Rigidbody body = arrow.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            arrow.AddComponent<Arrow>();
        }

        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.damage = damage;
            arrowScript.speed = arrowSpeed;
        }
    }
}
