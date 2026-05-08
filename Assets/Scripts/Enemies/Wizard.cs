using System.Collections;  
using System.Collections.Generic;
using UnityEngine;

public class Wizard: MonoBehaviour
{
    public float timeWaitMove = 1.0f;
    public float speed = 3.0f;
    public float tamañoCasilla = 1.0f; //REVIEW - por poner algo
    public Animator ani;

    private bool movementActive = false;
    private float timer = 0f;
    private Vector3 targetPosition;
    private Vector3[] directions = new Vector3[] {Vector3.forward, Vector3.back, Vector3.right, Vector3.left};

    void Start()
    {
      ani = GetComponent<Animator>();   
      targetPosition = transform.position;
    }

    void SelectDirection()
    {
        int dir = Random.Range(0,4);
        Vector3 moveDir = directions[dir];
        
        Vector3 definitiveDirection = transform.position + moveDir * tamañoCasilla;

        //REVIEW - añadir caso de colisiones con otros obstaculos o player
    }
    public void MoveSlime()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition; // posición exacta en la casilla
            moving = false;
            timer = 0f; // resetear timer al llegar
            ani.SetBool("move", false);
        }
    }

    void Update()
    {
        if (movementActive) MoveSlime();
        else
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