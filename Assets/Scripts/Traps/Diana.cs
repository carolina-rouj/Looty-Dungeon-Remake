using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diana : MonoBehaviour
{
    // TODO (player): private Player player;

    void Start()
    {
        // TODO (player): GameObject playerObj = GameObject.Find("Player");
        // TODO (player): if (playerObj != null) player = playerObj.GetComponent<Player>();
    }

    void OnTriggerEnter(Collider other)
    {
        // TODO (player): if (other.CompareTag("Player") && player != null) player.isOnDiana = true;
    }

    void OnTriggerExit(Collider other)
    {
        // TODO (player): if (other.CompareTag("Player") && player != null) player.isOnDiana = false;
    }
}