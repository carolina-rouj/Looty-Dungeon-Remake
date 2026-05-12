using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime: MonoBehaviour
{
    public float timeWaitMove = 1.0f;
    public float speed = 3.0f;
    public float tamañoCasilla = 1.0f; //REVIEW - por poner algo
    public float scaleSpeed = 10f;
    public Animator ani;
    // public GameObject RastroSlime;

    private bool movementActive = false;
    private bool isLanding = false;
    private bool isPreJumping = false;
    private float timer = 0f;
    private Vector3 targetPosition, lastPosition, groundPosition;
    private Vector3 normalScale, targetScale;
    private Vector3[] directions = new Vector3[] {Vector3.forward, Vector3.back, Vector3.right, Vector3.left};

    void Start()
    {
        ani = GetComponent<Animator>();
        targetPosition = transform.position;
        lastPosition = transform.position;
        normalScale = transform.localScale;
        targetScale = normalScale;
    }

    void SelectDirection()
    {
        int dir = Random.Range(0,4);
        Vector3 moveDir = directions[dir];
        groundPosition = transform.position + moveDir * tamañoCasilla;
        isLanding = false;
        isPreJumping = true;
        targetScale = new Vector3(normalScale.x * 1.3f, normalScale.y * 0.6f, normalScale.z * 1.3f);
        StartCoroutine(JumpSequence());
        //TODO - añadir caso de colisiones con otros obstaculos o player
        //TODO - debe dejar rastro de slime al moverse
    }

    // efecto de gelatina en el salto
    IEnumerator JumpSequence()
    {
        yield return new WaitForSeconds(0.12f);
        targetScale = new Vector3(normalScale.x * 0.7f, normalScale.y * 1.2f, normalScale.z * 0.7f);
        targetPosition = groundPosition + Vector3.up;
        isPreJumping = false;
        movementActive = true;
        ani.SetBool("movementActive", true);
    }

    public void MoveSlime()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            if (!isLanding)
            {
                isLanding = true;
                targetPosition = groundPosition;
            }
            else
            {
                targetScale = new Vector3(normalScale.x * 1.3f, normalScale.y * 0.6f, normalScale.z * 1.3f);
                StartCoroutine(ReturnToNormal());
                //if (lastPosition != groundPosition) Instantiate(RastroSlime, lastPosition, Quaternion.identity);
                lastPosition = groundPosition;
                isLanding = false;
                movementActive = false;
                timer = 0f;
                ani.SetBool("movementActive", false);
            }
        }
    }

    IEnumerator ReturnToNormal()
    {
        yield return new WaitForSeconds(0.1f);
        targetScale = normalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);

        if (movementActive) MoveSlime();
        else if (!isPreJumping)
        {
            timer += Time.deltaTime;
            if (timer >= timeWaitMove)
            {
                timer = 0f;
                SelectDirection();
            }
        }
    }
}
