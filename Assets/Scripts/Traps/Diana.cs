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
    private static int activeContacts;
    private bool playerInside;

    public static bool IsPlayerOnAnyDiana => activeContacts > 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInside)
        {
            playerInside = true;
            activeContacts++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // TODO (player): if (other.CompareTag("Player") && player != null) player.isOnDiana = false;
        if (other.CompareTag("Player") && playerInside)
        {
            playerInside = false;
            activeContacts = Mathf.Max(0, activeContacts - 1);
        }
    }

    void OnDisable()
    {
        if (playerInside)
        {
            playerInside = false;
            activeContacts = Mathf.Max(0, activeContacts - 1);
        }
    }
}
