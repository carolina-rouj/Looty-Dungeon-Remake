using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diana : MonoBehaviour
{
    private Player player;

    void Start()
    {
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) player = playerObj.GetComponent<player>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && player != null) player.isOnDiana = true; // Si el jugador pisa la diana
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && player != null) player.isOnDiana = false; 
    }
}