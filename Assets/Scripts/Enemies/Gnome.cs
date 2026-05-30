using UnityEngine;

public class Gnome : MonoBehaviour
{
    public float speed = 3.0f;
    public float detectionRange = 4.0f;
    public float rotationSpeed = 5.0f;
    public Animator ani;
    public GameObject targetPlayer;

    void Start()
    {
        ani = GetComponent<Animator>();
        targetPlayer = GameObject.Find("Player");
    }

    void MoveGnome()
    {
        if (Vector3.Distance(transform.position, targetPlayer.transform.position) > detectionRange)
        {
            ani.SetBool("movementActive", false);
        }
        else
        {
            Vector3 lookPos = targetPlayer.transform.position - transform.position;
            lookPos.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 100f);

            ani.SetBool("movementActive", true);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    public void Hurt()
    {
        Die();
    }

    private void Die()
    {
        ani.SetTrigger("die");
        Destroy(gameObject, 1.0f);
    }

    void Update()
    {
        MoveGnome();
    }
}
