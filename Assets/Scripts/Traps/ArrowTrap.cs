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
    // TODO (player): private Player player;
    private float timer = 0f;

    void Start()
    {
        // TODO (player): GameObject playerObj = GameObject.Find("Player");
        // TODO (player): if (playerObj != null) player = playerObj.GetComponent<Player>();
        if (diana != null) distanceToDiana = (int)Vector3.Distance(transform.position, diana.position);
        shootInterval = Mathf.Max(0.5f, distanceToDiana * 0.5f);
    }

    void Update()
    {
        // TODO (player): descomentar cuando el player esté implementado
        // if (player != null && player.isOnDiana)
        // {
        //     timer -= Time.deltaTime;
        //     if (timer <= 0f)
        //     {
        //         Disparo();
        //         timer = shootInterval;
        //     }
        // }
        // else
        // {
        //     timer = 0f;
        // }
    }

    void Disparo()
    {
        if (arrowPrefab == null || diana == null) return;

        Vector3 direction = (diana.position - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject arrow = Instantiate(arrowPrefab, transform.position, rotation);

        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.damage = damage;
            arrowScript.speed = arrowSpeed;
        }
    }
}
