using UnityEngine;

public class Diana : MonoBehaviour
{
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
