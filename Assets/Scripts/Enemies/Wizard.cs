using UnityEngine;

public class Wizard : MonoBehaviour
{
    public float speed = 3.0f;
    public float detectionRange = 5.0f;
    public float rotationSpeed = 5.0f;
    public Animator ani;
    // TODO (player): public GameObject targetPlayer;

    private int lives = 3;

    void Start()
    {
        ani = GetComponent<Animator>();
        // TODO (player): targetPlayer = GameObject.Find("Player");
    }

    void MoveWizard()
    {
        // TODO (player): descomentar cuando el player esté implementado
        // if (Vector3.Distance(transform.position, targetPlayer.transform.position) > detectionRange)
        // {
        //     ani.SetBool("movementActive", false);
        // }
        // else
        // {
        //     Vector3 lookPos = targetPlayer.transform.position - transform.position;
        //     lookPos.y = 0;
        //     Quaternion targetRotation = Quaternion.LookRotation(lookPos);
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 100f);
        //     ani.SetBool("movementActive", true);
        //     transform.Translate(Vector3.forward * speed * Time.deltaTime);
        // }
    }

    public void Hurt()
    {
        --lives;
        if (lives <= 0) Die();
    }

    private void Die()
    {
        ani.SetTrigger("die");
        Destroy(gameObject, 1.0f);
    }

    void Update()
    {
        MoveWizard();
    }
}
